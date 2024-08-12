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
        public static BehaviourTreeEditorWindow editorWindow;

        #region Static Style Properties
        
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

        #endregion

        private bool _isEditorAvailable;
        
        private BehaviourTree _tree;
        private BehaviourNodeViewUpdater _nodeViewUpdater;
        
        //UI Elements
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;


        #region Properties
        
        public bool editable
        {
            get { return _isEditorAvailable && !Application.isPlaying; }
        }

        public BehaviourTree tree
        {
            get { return _tree; }
        }

        public BehaviourTreeView view
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
            editorWindow ??= this;

            behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(behaviourTreeStyle);

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
                    this._nodeViewUpdater.Dispose();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    this.OnSelectionChange();
                    this._nodeViewUpdater = new BehaviourNodeViewUpdater(_treeView.GetNodeViewsToUpdate);
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
            if (!Application.isPlaying)
            {
                return;
            }
            
            _nodeViewUpdater.UpdateViewsState();
        }
    }
}
