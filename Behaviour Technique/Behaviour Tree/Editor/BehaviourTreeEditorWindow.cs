using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviourTechnique.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        public static BehaviourTreeEditorWindow Editor
        {
            get;
            private set;
        }

        #region Static Style Properties
        
        public static VisualTreeAsset BehaviourTreeEditorXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditor.uxml"); }
        }

        public static StyleSheet BehaviourTreeStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditorStyle.uss"); }
        }

        public static VisualTreeAsset NodeViewXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeView.uxml"); }
        }

        public static StyleSheet NodeViewStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeViewStyle.uss"); }
        }

        #endregion

        private bool _isEditorAvailable;
        
        private BehaviourTree _tree;

        //UI Elements
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;


        #region Properties
        
        public bool Editable
        {
            get { return _isEditorAvailable && !Application.isPlaying; }
        }

        public BehaviourTree Tree
        {
            get { return _tree; }
        }

        public BehaviourTreeView View
        {
            get { return _treeView; }
        }

        #endregion
        

        [MenuItem("Tools/BehaviourTreeEditor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
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
            Editor ??= this;

            BehaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(BehaviourTreeStyle);

            _treeView = rootVisualElement.Q<BehaviourTreeView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();

            _treeView.onNodeSelected += _inspectorView.UpdateSelection;

            this.OnSelectionChange();
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayNodeStateChanged;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayNodeStateChanged;
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
            BehaviourTree tree = this.GetBehaviourTreeForSelectedObject();
            _isEditorAvailable = tree != null && !Application.isPlaying;

            if (tree != null)
            {
                _tree = tree;

                if (Application.isPlaying || AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                {
                    _treeView.OnGraphEditorView(tree);
                }
            }
        }


        private BehaviourTree GetBehaviourTreeForSelectedObject()
        {
            BehaviourTree tree = Selection.activeObject as BehaviourTree;

            if (Selection.activeGameObject != null)
            {
                GameObject targetObject = Selection.activeGameObject;
                var runner = targetObject.GetComponent<BehaviourActor>();
                tree ??= runner?.runtimeTree;
            }
            return tree;
        }


        private void Update()
        {
            if (!Application.isPlaying || _treeView == null)
            {
                return;
            }

            _treeView.UpdateNodeView();
        }
    }
}
