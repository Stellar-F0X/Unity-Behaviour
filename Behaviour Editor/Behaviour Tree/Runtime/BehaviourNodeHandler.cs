using System.Collections.Generic;
using UnityEngine;

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

        
#region 필드들

        /// <summary>
        /// 런타임 호출 스택들의 리스트
        /// 각 병렬 실행 브랜치마다 별도의 스택을 유지
        /// </summary>
        private readonly List<Stack<NodeBase>> _runtimeCallStack = new List<Stack<NodeBase>>();

#endregion

#region 공개 메서드들

        /// <summary>
        /// 트리 클론 생성 및 CallStack 구축 - 기존 순회 로직을 활용하여 트리 구조를 복제하고 동시에 CallStack 생성
        /// JobSystem으로 병렬화 가능한 설계
        /// </summary>
        /// <param name="originalRoot">원본 루트 노드</param>
        /// <param name="clonedRoot">클론된 루트 노드</param>
        /// <param name="treeRunner">트리 러너</param>
        /// <param name="clonedSet">클론된 노드셋</param>
        public void CloneTree(NodeBase originalRoot, NodeBase clonedRoot, BehaviourTreeRunner treeRunner, BehaviourNodeSet clonedSet)
        {
            Queue<CloneInfo> cloneQueue = new Queue<CloneInfo>();
            Stack<NodeBase> postInitStack = new Stack<NodeBase>();
            int callStackID = 0;

            cloneQueue.Enqueue(new CloneInfo(originalRoot, clonedRoot, 0, callStackID));

            while (cloneQueue.Count > 0)
            {
                CloneInfo currentClone = cloneQueue.Dequeue();
                this.ProcessCloneBranch(currentClone, cloneQueue, postInitStack, treeRunner, clonedSet, ref callStackID);
            }

            // PostTreeCreation을 위한 후처리
            this.ProcessPostInitialization(postInitStack);
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
            this.EnsureCallStackExists(callStackID);
            
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
            Queue<AbortInfo> abortQueue = new Queue<AbortInfo>();
            abortQueue.Enqueue(new AbortInfo(callStackID, node));

            this.ProcessAbortQueue(abortQueue, true);
        }

        /// <summary> 지정된 호출 스택의 전체 서브트리를 중단 </summary>
        /// <param name="callStackID">중단할 호출 스택 ID</param>
        public void AbortSubtree(in int callStackID)
        {
            Queue<AbortInfo> abortQueue = new Queue<AbortInfo>();
            abortQueue.Enqueue(new AbortInfo(callStackID));

            this.ProcessAbortQueue(abortQueue, false);
        }

#endregion

#region 클론 관련 내부 메서드들 (추후 JobSystem으로 분리)

        /// <summary>
        /// 단일 클론 브랜치를 처리
        /// JobSystem으로 병렬화할 수 있는 영역
        /// </summary>
        /// <param name="cloneInfo">클론 정보</param>
        /// <param name="cloneQueue">클론 작업 큐</param>
        /// <param name="postInitStack">후처리 스택</param>
        /// <param name="treeRunner">트리 러너</param>
        /// <param name="clonedSet">클론된 노드셋</param>
        /// <param name="callStackID">CallStack ID (참조로 전달하여 증가)</param>
        private void ProcessCloneBranch(CloneInfo cloneInfo, Queue<CloneInfo> cloneQueue, Stack<NodeBase> postInitStack, BehaviourTreeRunner treeRunner, BehaviourNodeSet clonedSet, ref int callStackID)
        {
            NodeBase clone = cloneInfo.clone;
            
            // 이름에서 Unity의 (Clone) 접미사 제거
            if (clone.name.EndsWith("(Clone)"))
            {
                clone.name = clone.name.Remove(clone.name.Length - 7);
            }
            
            clone.depth = cloneInfo.depth;
            clone.treeRunner = treeRunner;
            clonedSet.nodeList.Add(clone);
            postInitStack.Push(cloneInfo.clone);
            
            // CallStack ID 설정 및 스택 생성
            this.SetNodeCallStackID(cloneInfo.clone, cloneInfo.stackID);
            this.EnsureCallStackExists(cloneInfo.stackID);
            this.ProcessCloneChildren(cloneInfo, cloneQueue, ref callStackID);
            this.ProcessBlackboardProperties(cloneInfo.clone, treeRunner);
        }


        /// <summary>
        /// 클론 노드의 자식들을 처리
        /// 기존 순회 로직과 동일한 패턴 사용 - 병렬 노드는 새로운 CallStack ID 할당
        /// </summary>
        private void ProcessCloneChildren(CloneInfo cloneInfo, Queue<CloneInfo> cloneQueue, ref int callStackID)
        {
            NodeBase origin = cloneInfo.origin;
            NodeBase clone = cloneInfo.clone;
            int nextDepth = cloneInfo.depth + 1;

            switch (origin.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    this.ProcessRootNodeClone((RootNode)origin, (RootNode)clone, nextDepth, cloneInfo.stackID, cloneQueue);
                    break;
                    
                case NodeBase.ENodeType.Decorator:
                    this.ProcessDecoratorNodeClone((DecoratorNode)origin, (DecoratorNode)clone, nextDepth, cloneInfo.stackID, cloneQueue);
                    break;
                    
                case NodeBase.ENodeType.Composite:
                    this.ProcessCompositeNodeClone((CompositeNode)origin, (CompositeNode)clone, nextDepth, cloneInfo.stackID, cloneQueue, ref callStackID);
                    break;
            }
        }

        
        /// <summary> 루트 노드 클론 처리 </summary>
        private void ProcessRootNodeClone(RootNode origin, RootNode clone, int nextDepth, int stackID, Queue<CloneInfo> cloneQueue)
        {
            if (origin.child == null) return;
            
            NodeBase childClone = Object.Instantiate(origin.child);
            childClone.parent = clone;
            clone.child = childClone;
            
            cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
        }

        
        /// <summary> 데코레이터 노드 클론 처리 </summary>
        private void ProcessDecoratorNodeClone(DecoratorNode origin, DecoratorNode clone, int nextDepth, int stackID, Queue<CloneInfo> cloneQueue)
        {
            if (origin.child == null) return;
            
            NodeBase childClone = Object.Instantiate(origin.child);
            childClone.parent = clone;
            clone.child = childClone;
            
            cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
        }

        
        /// <summary> 컴포지트 노드 클론 처리 - 병렬 노드의 경우 새로운 CallStack ID 할당 </summary>
        private void ProcessCompositeNodeClone(CompositeNode origin, CompositeNode clone, int nextDepth, int stackID, Queue<CloneInfo> cloneQueue, ref int callStackID)
        {
            if (origin.children == null || origin.children.Count == 0) return;
            
            bool isParallelNode = origin is ParallelNode;
            
            if (isParallelNode)
            {
                // 병렬 노드: 각 자식마다 새로운 CallStack ID 할당
                for (int i = 0; i < origin.children.Count; ++i)
                {
                    NodeBase childClone = Object.Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;
                    
                    int newStackID = ++callStackID;
                    this.EnsureCallStackExists(newStackID);
                    cloneQueue.Enqueue(new CloneInfo(origin.children[i], childClone, nextDepth, newStackID));
                }
            }
            else
            {
                // 일반 컴포지트 노드: 같은 CallStack ID 사용
                for (int i = 0; i < origin.children.Count; ++i)
                {
                    NodeBase childClone = Object.Instantiate(origin.children[i]);
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
                if (!typeof(IBlackboardProperty).IsAssignableFrom(fieldInfo.FieldType)) continue;
                
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

        
        /// <summary>
        /// 후처리 초기화 실행
        /// JobSystem에서는 메인 스레드에서 실행되어야 함
        /// </summary>
        private void ProcessPostInitialization(Stack<NodeBase> postInitStack)
        {
            while (postInitStack.Count > 0)
            {
                NodeBase currentNode = postInitStack.Pop();
                currentNode.PostTreeCreation();
            }
        }
#endregion

        
#region 내부 헬퍼 메서드들 (추후 JobSystem으로 분리)
        /// <summary>
        /// 중단 큐를 처리하여 노드들을 정리
        /// JobSystem으로 병렬화 가능하지만 상태 변경이 있어 주의 필요
        /// </summary>
        /// <param name="abortQueue">중단할 노드들의 큐</param>
        /// <param name="hasTargetNode">특정 노드까지만 중단할지 여부</param>
        private void ProcessAbortQueue(Queue<AbortInfo> abortQueue, bool hasTargetNode)
        {
            while (abortQueue.Count > 0)
            {
                AbortInfo current = abortQueue.Dequeue();

                if (this.IsValidCallStack(current.callStackID) == false || _runtimeCallStack[current.callStackID].Count == 0)
                {
                    continue;
                }

                if (hasTargetNode)
                {
                    this.ProcessTargetedAbort(current, abortQueue);
                }
                else
                {
                    this.ProcessFullStackAbort(current, abortQueue);
                }
            }
        }

        
        /// <summary> 특정 노드까지의 타겟 중단을 처리 </summary>
        private void ProcessTargetedAbort(AbortInfo abortInfo, Queue<AbortInfo> abortQueue)
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
                this.ProcessNodeExit(stackNode, abortQueue);
                
                if (_runtimeCallStack[currentID].Count == 0)
                {
                    break;
                }

                stackNode = _runtimeCallStack[currentID].Peek();
            }
        }

        
        /// <summary> 전체 스택 중단을 처리 </summary>
        private void ProcessFullStackAbort(AbortInfo abortInfo, Queue<AbortInfo> abortQueue)
        {
            int currentID = abortInfo.callStackID;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            NodeBase stackNode = _runtimeCallStack[currentID].Peek();
            this.ProcessNodeExit(stackNode, abortQueue);

            if (_runtimeCallStack[currentID].Count > 0)
            {
                // 다음 노드도 중단 큐에 추가
                NodeBase nextNode = _runtimeCallStack[currentID].Peek();
                abortQueue.Enqueue(new AbortInfo(nextNode.callStackID));
            }
        }

        
        /// <summary> 노드 종료 처리 (병렬 노드의 경우 자식들도 중단) </summary>
        private void ProcessNodeExit(NodeBase node, Queue<AbortInfo> abortQueue)
        {
            if (node is ParallelNode parallelNode)
            {
                parallelNode.Stop();

                // 병렬 노드의 모든 자식들을 중단 큐에 추가
                foreach (var child in parallelNode.GetChildren())
                {
                    abortQueue.Enqueue(new AbortInfo(child.callStackID));
                }
            }

            node.ExitNode();
        }

#endregion
        

        /// <summary> 노드에 호출 스택 ID를 설정 </summary>
        private void SetNodeCallStackID(NodeBase node, int stackID)
        {
            node.callStackID = stackID;
        }

        /// <summary> 호출 스택이 유효한지 확인  </summary>
        private bool IsValidCallStack(int callStackID)
        {
            return callStackID >= 0 && callStackID < _runtimeCallStack.Count;
        }

        /// <summary>  호출 스택이 존재하는지 확인하고 없으면 생성 </summary>
        private void EnsureCallStackExists(int callStackID)
        {
            while (_runtimeCallStack.Count <= callStackID)
            {
                _runtimeCallStack.Add(new Stack<NodeBase>());
            }
        }
    }
}