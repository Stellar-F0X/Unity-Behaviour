using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GraphNode = UnityEditor.Experimental.GraphView.Node;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeSearchHelper
    {
        public enum ESearchOptions
        {
            Tag,
            Name,
            Both
        };

        private readonly List<NodeView> _viewList = new List<NodeView>();

        
        public bool HasSyntaxes(string searchText, out string[] syntax)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                syntax = null;
                return false;
            }

            string delimiter = null;

            if (searchText.Contains("t:", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = "t:";
            }
            else if (searchText.Contains("n:", StringComparison.OrdinalIgnoreCase))
            {
                delimiter = "n:";
            }

            if (delimiter is not null)
            {
                string[] syntaxList = Regex.Split(searchText, delimiter, RegexOptions.IgnoreCase);

                if (syntaxList.Length == 2)
                {
                    syntaxList[0] = delimiter;
                    syntaxList[1] = syntaxList[1].TrimStart();
                    syntax        = syntaxList;
                    return true;
                }

                syntax = null;
                return false;
            }

            syntax = new string[1] { searchText };
            return true;
        }



        public NodeView[] GetNodeView(string name, ESearchOptions options, IEnumerable<GraphNode> nodes)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            
            _viewList.Clear();

            for (int index = 0; index < nodes.Count(); ++index)
            {
                if (nodes.ElementAt(index) is NodeView nodeView)
                {
                    if (HasFoundNode(nodeView, name, options))
                    {
                        _viewList.Add(nodeView);
                    }
                }
            }

            return _viewList.ToArray();
        }


        private bool HasFoundNode(NodeView nodeView, string name, ESearchOptions options)
        {
            switch (options)
            {
                case ESearchOptions.Tag:
                {
                    return string.Compare(nodeView.node.tag, name, StringComparison.OrdinalIgnoreCase) == 0;
                }

                case ESearchOptions.Name:
                {
                    return string.Compare(nodeView.node.name, name, StringComparison.OrdinalIgnoreCase) == 0;
                }

                case ESearchOptions.Both:
                {
                    bool flag = string.Compare(nodeView.node.name, name, StringComparison.OrdinalIgnoreCase) == 0;
                    return flag || string.Compare(nodeView.node.tag, name, StringComparison.OrdinalIgnoreCase) == 0;
                }
            }

            return false;
        }
    }
}