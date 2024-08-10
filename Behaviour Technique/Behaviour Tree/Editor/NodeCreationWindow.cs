using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeCreationWindow :  ScriptableObject, ISearchWindowProvider
    {
        private NodeView _nodeView;
        private BehaviourTreeView _treeView;


        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));
            searchTree.AddRange(CreateSearchTreeEntry<ActionNode>("", 1, t => () => CreateNode(t, context)));

            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            throw new System.NotImplementedException();
        }


        private SearchTreeEntry[] CreateSearchTreeEntry<T>(string title, int layerLevel, Func<Type, Action> invoke) where T : Node
        {
            SearchTreeGroupEntry entry = new SearchTreeGroupEntry(new GUIContent(title)) {
                level = layerLevel,
            };

            TypeCache.TypeCollection typeList = TypeCache.GetTypesDerivedFrom<T>();
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Count];

            for (int i = 0; i < typeList.Count; ++i)
            {
                entries[i].content.text = typeList[i].Name;
                entries[i].userData = invoke.Invoke(typeList[i]);
                entries[i].level = layerLevel + 1;
            }

            return entries;
        }
        
        
        private NodeView CreateNode(Type type, SearchWindowContext context)
        {
            return null;
        } 
    }
}
