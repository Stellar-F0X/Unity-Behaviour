using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class NodeEdgeHandler
    {
        public void ConnectEdges(BehaviourTreeView treeView, NodeBase parentNodeBase, List<NodeBase> childrenNodes)
        {
            if (childrenNodes is null || childrenNodes.Count == 0)
            {
                return;
            }
            
            foreach (var child in childrenNodes)
            {
                NodeView parentView = treeView.FindNodeView(parentNodeBase);
                NodeView childView = treeView.FindNodeView(child);

                if (parentView == null || childView == null)
                {
                    continue;
                }

                parentView.toChildEdge = parentView.output.ConnectTo(childView.input);
                childView.toParentEdge = parentView.toChildEdge;
                treeView.AddElement(parentView.toChildEdge);
            }
        }


        public void ConnectEdges(BehaviourTree tree, List<Edge> edges)
        {
            if (edges is null || edges.Count == 0)
            {
                return;
            }
            
            foreach (var edge in edges)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView == null || childView == null)
                {
                    continue;
                }
                
                tree.nodeSet.AddChild(parentView.node, childView.node);
            }
        }


        public void DeleteEdges(BehaviourTree tree, Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;
            
            if (parentView == null || childView == null)
            {
                return;
            }
            
            tree.nodeSet.RemoveChild(parentView?.node, childView?.node);
            edge.RemoveFromHierarchy();
        }
    }
}
