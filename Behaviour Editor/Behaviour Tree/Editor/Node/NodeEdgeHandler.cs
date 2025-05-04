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
            foreach (var child in childrenNodes)
            {
                NodeView parentView = treeView.FindNodeView(parentNodeBase);
                NodeView childView = treeView.FindNodeView(child);

                if (parentView == null || childView == null)
                {
                    continue;
                }

                treeView.AddElement(parentView.output.ConnectTo(childView.input));
            }
        }


        public void ConnectEdges(BehaviourTree tree, List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView == null || childView == null)
                {
                    continue;
                }
                
                tree.AddChild(parentView?.node, childView?.node);
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
            
            tree.RemoveChild(parentView?.node, childView?.node);
        }
    }
}
