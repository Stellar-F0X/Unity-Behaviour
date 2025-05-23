using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    internal class BehaviourNodeHandler
    {
        /// <summary> 클론 작업 정보를 담는 구조체 </summary>
        private struct CloneInfo
        {
            public CloneInfo(NodeBase origin, NodeBase clone, int depth, int stackID)
            {
                this.origin = origin;
                this.clone = clone;
                this.depth = depth;
                this.stackID = stackID;
            }

            public readonly NodeBase origin;
            public readonly NodeBase clone;
            public readonly int depth;
            public readonly int stackID;
        }

        /// <summary> 중단(Abort) 작업 정보를 담는 구조체 </summary>
        private struct AbortInfo
        {
            public AbortInfo(int callStackID, NodeBase targetNode = null)
            {
                this.callStackID = callStackID;
                this.targetNode = targetNode;
            }

            public readonly int callStackID;
            public readonly NodeBase targetNode; //null이면 전체 스택 중단
        }
        
        
        private FixedSizeList<Stack<NodeBase>> _runtimeCallStack;

        private FixedSizeQueue<AbortInfo> _abortQueue;


#region Public Methods
        /// <summary> 트리에서 지정된 태그가 수식된 노드들을 찾습니다. </summary>
        /// <param name="nodeTag">수식된 태그</param>
        /// <param name="nodeSet">트리 집합</param>
        /// <param name="accessors">찾은 노드들</param>
        /// <returns>노드 탐색 성공 여부</returns>
        public NodeAccessor[] GetNodeByTag(string nodeTag, BehaviourNodeSet nodeSet)
        {
            Span<int> indexArray = stackalloc int[nodeSet.nodeList.Count];
            int count = 0;
            
            for (int i = 0; i < nodeSet.nodeList.Count; ++i)
            {
                NodeBase currentNode = nodeSet.nodeList[i];

                if (currentNode.tag.CompareTo(nodeTag) == 0)
                {
                    indexArray[count] = i;
                    count++;
                }
            }

            if (count > 0)
            {
                NodeAccessor[] accessors = new NodeAccessor[count];
                
                //만약 Tag가 모든 노드를 대상으로 한다면 시간 복잡도는 O(2n)이 되므로 GC 측면에선 좋으나, 빠른 검색의 관점에선 잘 모르겠다. 
                for (int i = 0; i < count; ++i)
                {
                    NodeBase targetNode = nodeSet.nodeList[indexArray[i]];
                    accessors[i] = new NodeAccessor(targetNode);
                }

                return accessors;
            }
            
            return null;
        }
        

        /// <summary> 트리 디렉토리를 토대로 경로상에 위치한 노드를 찾습니다. </summary>
        /// <param name="treePath">트리 디렉토리</param>
        /// <param name="nodeSet">트리 집합</param>
        /// <param name="node">찾은 노드</param>
        /// <returns>노드 탐색 성공 여부</returns>
        public bool TryGetNodeByPath(string treePath, BehaviourNodeSet nodeSet, out NodeAccessor node)
        {
            if (string.IsNullOrEmpty(treePath) || string.IsNullOrWhiteSpace(treePath))
            {
                node = default;
                return false;
            }
            
            Span<char> pathBuffer = stackalloc char[256];
            Span<int> pathStartIndices = stackalloc int[128];
            Span<int> pathLengths = stackalloc int[128];
            
            int pathCount = 0;
            int currentStart = 0;
            int pathLength = Math.Min(treePath.Length, 256);
            
            treePath.AsSpan(0, pathLength).CopyTo(pathBuffer);
            
            for (int i = 0; i < pathLength; i++)
            {
                if (pathBuffer[i] == '/')
                {
                    if (i > currentStart) // 빈 경로 세그먼트 방지
                    {
                        pathStartIndices[pathCount] = currentStart;
                        pathLengths[pathCount] = i - currentStart;
                        pathCount++;
                    }
                    currentStart = i + 1;
                }
            }
            
            if (currentStart < pathLength)
            {
                pathStartIndices[pathCount] = currentStart;
                pathLengths[pathCount] = pathLength - currentStart;
                pathCount++;
            }
            
            if (pathCount == 0)
            {
                node = default;
                return false;
            }
            
            ReadOnlySpan<char> rootNamePath = pathBuffer.Slice(pathStartIndices[0], pathLengths[0]);
            
            if (rootNamePath.Equals(nodeSet.rootNode.name.AsSpan(), StringComparison.Ordinal) == false)
            {
                node = default;
                return false;
            }
            
            NodeBase nodeBase = nodeSet.rootNode;
            
            for (int i = 1; i < pathCount; i++)
            {
                bool find = false;
                ReadOnlySpan<char> currentPath = pathBuffer.Slice(pathStartIndices[i], pathLengths[i]);
                
                if (nodeBase is IBehaviourIterable iterable)
                {
                    foreach (NodeBase child in iterable.GetChildren())
                    {
                        if (currentPath.Equals(child.name.AsSpan(), StringComparison.Ordinal))
                        {
                            nodeBase = child;
                            find = true;
                            break;
                        }
                    }
                    
                    if (find == false)
                    {
                        node = default;
                        return false;
                    }
                }
            }

            node = new NodeAccessor(nodeBase);
            return true;
        }



        /// <summary> 노드 클론 생성 및 CallStack 구축, 순회를 통해 트리의 노드들을 복제하고 동시에 콜 스택을 생성합니다. </summary>
        /// <param name="originalRoot">원본 루트 노드</param>
        /// <param name="clonedRoot">클론된 루트 노드</param>
        /// <param name="treeRunner">트리 러너</param>
        /// <param name="originalSet">원본 노드셋</param>
        /// <param name="clonedSet">클론된 노드셋</param>
        public void CloneNodeSet(NodeBase originalRoot, NodeBase clonedRoot, BehaviourTreeRunner treeRunner, BehaviourNodeSet originalSet, BehaviourNodeSet clonedSet)
        {
            FixedSizeQueue<CloneInfo> cloneQueue = new FixedSizeQueue<CloneInfo>(originalSet.nodeList.Count);
            FixedSizeStack<NodeBase> postInitStack = new FixedSizeStack<NodeBase>(originalSet.nodeList.Count);
            int callStackID = 0;

            cloneQueue.Enqueue(new CloneInfo(originalRoot, clonedRoot, 0, callStackID));

            while (cloneQueue.count > 0)
            {
                CloneInfo currentClone = cloneQueue.Dequeue();
                this.ProcessCloneBranch(currentClone, cloneQueue, postInitStack, treeRunner, clonedSet, ref callStackID);
            }

            _abortQueue = new FixedSizeQueue<AbortInfo>(clonedSet.nodeList.Count - 1);
            _runtimeCallStack = new FixedSizeList<Stack<NodeBase>>(callStackID + 1);

            for (int i = 0; i < callStackID + 1; ++i)
            {
                _runtimeCallStack.Add(new Stack<NodeBase>());
            }

            // PostTreeCreation을 위한 후처리
            while (postInitStack.count > 0)
            {
                NodeBase currentNode = postInitStack.Pop();
                currentNode.PostTreeCreation();
            }
        }


        /// <summary> 지정된 호출 스택의 현재 실행 중인 노드를 반환 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <returns>현재 노드, 없으면 null</returns>
        public NodeBase GetCurrentNode(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                return null;
            }

            return _runtimeCallStack[callStackID].Peek();
        }


        /// <summary> 지정된 호출 스택에 노드를 푸시 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">푸시할 노드</param>
        public void PushInCallStack(in int callStackID, NodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        /// <summary> 지정된 호출 스택에서 최상단 노드를 팝 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        public void PopInCallStack(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                Debug.LogWarning($"호출 스택 ID {callStackID}에서 꺼낼 노드가 없습니다.");
                return;
            }

            _runtimeCallStack[callStackID].Pop();
        }


        /// <summary>
        /// 지정된 노드부터 상위까지의 서브트리를 중단
        /// 해당 노드보다 깊은 depth의 노드들을 모두 정리
        /// </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">중단할 기준 노드</param>
        public void AbortSubtreeFrom(in int callStackID, NodeBase node)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID, node));

            this.ProcessAbortQueue(true);
        }


        /// <summary> 지정된 호출 스택의 전체 서브트리를 중단 </summary>
        /// <param name="callStackID">중단할 호출 스택 ID</param>
        public void AbortSubtree(in int callStackID)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID));

            this.ProcessAbortQueue(false);
        }

#endregion


#region Internal cloning methods (추후 JobSystem으로 분리)

        /// <summary>
        /// 단일 클론 브랜치를 처리
        /// JobSystem으로 병렬화할 수 있는 영역
        /// </summary>
        /// <param name="cloneInfo">클론 정보</param>
        /// <param name="cloneQueue">클론 작업 큐</param>
        /// <param name="postInitStack">후처리 스택</param>
        /// <param name="treeRunner">트리 러너</param>
        /// <param name="clonedSet">클론된 노드셋</param>
        /// <param name="callStackID">CallStack ID</param>
        private void ProcessCloneBranch(CloneInfo cloneInfo, FixedSizeQueue<CloneInfo> cloneQueue, FixedSizeStack<NodeBase> postInitStack, BehaviourTreeRunner treeRunner, BehaviourNodeSet clonedSet, ref int callStackID)
        {
            NodeBase clone = cloneInfo.clone;

            // 이름에서 Unity의 (Clone) 접미사 제거
            if (clone.name.EndsWith("(Clone)"))
            {
                clone.name = clone.name.Remove(clone.name.Length - 7);
            }

            clone.depth = cloneInfo.depth;
            clone.treeRunner = treeRunner;
            clone.callStackID = cloneInfo.stackID;
            clonedSet.nodeList.Add(clone);
            postInitStack.Push(cloneInfo.clone);

            NodeBase origin = cloneInfo.origin;
            int nextDepth = cloneInfo.depth + 1;

            switch (origin.nodeType)
            {
                case NodeBase.ENodeType.Root: 
                    this.ProcessRootNodeClone((RootNode)origin, (RootNode)clone, nextDepth, cloneInfo.stackID, cloneQueue); break;

                case NodeBase.ENodeType.Decorator:
                    this.ProcessDecoratorNodeClone((DecoratorNode)origin, (DecoratorNode)clone, nextDepth, cloneInfo.stackID, cloneQueue); break;

                case NodeBase.ENodeType.Composite:
                    this.ProcessCompositeNodeClone((CompositeNode)origin, (CompositeNode)clone, nextDepth, cloneInfo.stackID, cloneQueue, ref callStackID); break;
            }
            
            this.ProcessBlackboardProperties(cloneInfo.clone, treeRunner);
        }


        /// <summary> 루트 노드 클론 처리 </summary>
        private void ProcessRootNodeClone(RootNode origin, RootNode clone, int nextDepth, int stackID, FixedSizeQueue<CloneInfo> cloneQueue)
        {
            if (origin.child == null)
            {
                return;
            }

            NodeBase childClone = UObject.Instantiate(origin.child);
            childClone.parent = clone;
            clone.child = childClone;

            cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
        }


        /// <summary> 데코레이터 노드 클론 처리 </summary>
        private void ProcessDecoratorNodeClone(DecoratorNode origin, DecoratorNode clone, int nextDepth, int stackID, FixedSizeQueue<CloneInfo> cloneQueue)
        {
            if (origin.child == null)
            {
                return;
            }

            NodeBase childClone = UObject.Instantiate(origin.child);
            childClone.parent = clone;
            clone.child = childClone;

            cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
        }


        /// <summary> 컴포지트 노드 클론 처리 - 병렬 노드의 경우 새로운 CallStack ID 할당 </summary>
        private void ProcessCompositeNodeClone(CompositeNode origin, CompositeNode clone, int nextDepth, int stackID, FixedSizeQueue<CloneInfo> cloneQueue, ref int callStackID)
        {
            if (origin.children == null || origin.children.Count == 0)
            {
                return;
            }

            if (origin is ParallelNode)
            {
                // 병렬 노드: 각 자식마다 새로운 CallStack ID 할당
                for (int i = 0; i < origin.children.Count; ++i)
                {
                    NodeBase childClone = UObject.Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;

                    int newStackID = ++callStackID;
                    cloneQueue.Enqueue(new CloneInfo(origin.children[i], childClone, nextDepth, newStackID));
                }
            }
            else
            {
                // 일반 컴포지트 노드: 같은 CallStack ID 사용
                for (int i = 0; i < origin.children.Count; ++i)
                {
                    NodeBase childClone = UObject.Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;

                    cloneQueue.Enqueue(new CloneInfo(origin.children[i], childClone, nextDepth, stackID));
                }
            }
        }


        /// <summary>
        /// 블랙보드 프로퍼티 처리
        /// JobSystem에서는 메인 스레드에서 실행되어야 함
        /// </summary>
        private void ProcessBlackboardProperties(NodeBase clonedNode, BehaviourTreeRunner treeRunner)
        {
            foreach (var fieldInfo in ReflectionHelper.GetCachedFieldInfo(clonedNode?.GetType()))
            {
                if (typeof(IBlackboardProperty).IsAssignableFrom(fieldInfo.FieldType) == false)
                {
                    continue;
                }

                ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(fieldInfo);

                if (accessor.getter(clonedNode) is IBlackboardProperty property)
                {
                    IBlackboardProperty foundProperty = treeRunner.runtimeTree.blackboard.FindProperty(property.key);

                    if (foundProperty != null)
                    {
                        accessor.setter(clonedNode, foundProperty);
                    }
                }
            }
        }

#endregion


#region Private Helper Methods

        /// <summary>
        /// 중단 큐를 처리하여 노드들을 정리
        /// JobSystem으로 병렬화 가능하지만 상태 변경이 있어 주의 필요
        /// </summary>
        /// <param name="abortQueue">중단할 노드들의 큐</param>
        /// <param name="hasTargetNode">특정 노드까지만 중단할지 여부</param>
        private void ProcessAbortQueue(bool hasTargetNode)
        {
            while (_abortQueue.count > 0)
            {
                AbortInfo current = _abortQueue.Dequeue();

                if (this.IsValidCallStack(current.callStackID) == false || _runtimeCallStack[current.callStackID].Count == 0)
                {
                    continue;
                }

                if (hasTargetNode)
                {
                    this.ProcessTargetedAbort(current);
                }
                else
                {
                    this.ProcessFullStackAbort(current);
                }
            }
        }


        /// <summary> 특정 노드까지의 타겟 중단을 처리 </summary>
        private void ProcessTargetedAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;
            NodeBase targetNode = abortInfo.targetNode;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            NodeBase stackNode = _runtimeCallStack[currentID].Peek();

            // 타겟 노드보다 깊은 depth의 노드들을 모두 정리
            while (stackNode.Equals(targetNode) == false && stackNode.depth > targetNode.depth)
            {
                this.ProcessNodeExit(stackNode);

                if (_runtimeCallStack[currentID].Count == 0)
                {
                    break;
                }

                stackNode = _runtimeCallStack[currentID].Peek();
            }
        }


        /// <summary> 전체 스택 중단을 처리 </summary>
        private void ProcessFullStackAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            NodeBase stackNode = _runtimeCallStack[currentID].Peek();
            this.ProcessNodeExit(stackNode);

            if (_runtimeCallStack[currentID].Count > 0)
            {
                // 다음 노드도 중단 큐에 추가
                NodeBase nextNode = _runtimeCallStack[currentID].Peek();
                _abortQueue.Enqueue(new AbortInfo(nextNode.callStackID));
            }
        }


        /// <summary> 노드 종료 처리 (병렬 노드의 경우 자식들도 중단) </summary>
        private void ProcessNodeExit(NodeBase node)
        {
            if (node is ParallelNode parallelNode)
            {
                parallelNode.Stop();

                // 병렬 노드의 모든 자식들을 중단 큐에 추가
                foreach (var child in parallelNode.GetChildren())
                {
                    _abortQueue.Enqueue(new AbortInfo(child.callStackID));
                }
            }

            node.ExitNode();
        }

        /// <summary> 호출 스택이 유효한지 확인  </summary>
        private bool IsValidCallStack(int callStackID)
        {
            return callStackID >= 0 && callStackID < _runtimeCallStack.count;
        }

#endregion
    }
}