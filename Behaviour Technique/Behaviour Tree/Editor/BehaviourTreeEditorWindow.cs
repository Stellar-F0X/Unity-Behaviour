using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviourTechnique.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        public static BehaviourTreeEditorWindow editorWindow;


        private bool _isEditorAvailable;

        private BehaviourTree _tree;
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;

        public bool editable
        {
            get { return _isEditorAvailable && !Application.isPlaying; }
        }

        public BehaviourTree tree
        {
            get { return _tree; }
        }

        public static VisualTreeAsset behaviourTreeEditorXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditor.uxml"); }
        }

        public static StyleSheet behaviourTreeStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditorStyle.uss"); }
        }

        public static VisualTreeAsset nodeViewXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeView.uxml"); }
        }

        public static StyleSheet nodeViewStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeViewStyle.uss"); }
        }


        [MenuItem("Tools/BehaviourTreeEditor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            editorWindow = wnd;
        }


        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }


        private void CreateGUI()
        {
            behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(behaviourTreeStyle);

            _treeView = rootVisualElement.Q<BehaviourTreeView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();

            _treeView.onNodeSelected += OnNodeSelectionChanged;

            this.OnSelectionChange();
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayNodeStateChanged;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged += OnPlayNodeStateChanged;
        }


        private void OnPlayNodeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    this.OnSelectionChange();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    this.OnSelectionChange();
                    break;

                case PlayModeStateChange.ExitingEditMode: break;

                case PlayModeStateChange.ExitingPlayMode: break;
            }
        }


        private void OnSelectionChange()
        {
            editorWindow ??= this;
            BehaviourTree tree = Selection.activeObject as BehaviourTree;

            if (tree == null && _treeView != null)
            {
                var runner = Selection.activeGameObject?.GetComponent<BehaviourActor>();

                if (runner != null)
                {
                    tree = runner?.runtimeTree;
                }
            }

            if (tree != null && _treeView != null)
            {
                if (AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                {
                    _tree = tree;
                    _treeView.PopulateView(tree);
                    _isEditorAvailable = true;
                    return;
                }
                else if (Application.isPlaying)
                {
                    _treeView?.PopulateView(tree);
                }

                _isEditorAvailable = false;
            }
        }


        private void OnNodeSelectionChanged(NodeView node)
        {
            _inspectorView.UpdateSelection(node);
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _treeView.UpdateNodeState();
        }
    }
}
