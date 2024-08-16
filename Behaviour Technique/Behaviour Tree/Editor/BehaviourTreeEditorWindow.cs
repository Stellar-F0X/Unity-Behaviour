using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviourTechnique.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public partial class BehaviourTreeEditorWindow : EditorWindow
    {
        private bool _isEditorAvailable;

        private BehaviourTree _tree;
        private BehaviourActor _actor;

        //UI Elements
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;


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

        public BehaviourActor Actor
        {
            get
            {
                if (_actor == null || _actor.runtimeTree == null || _actor.runtimeTree != Tree)
                {
                    BehaviourActor[] actors = Object.FindObjectsByType<BehaviourActor>(FindObjectsSortMode.None);
                    _actor = actors.FirstOrDefault(actor => actor.runtimeTree?.cloneGroupID == Tree?.cloneGroupID);
                }
                return _actor;
            }
        }

        [MenuItem("Tools/BehaviourTreeEditor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            Instance = wnd;
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
            Instance = this;

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
            BehaviourTree tree = this.GetBehaviourTreeFromSelectedObject();
            _isEditorAvailable = tree != null && !Application.isPlaying;

            if (tree != null)
            {
                if (tree != _tree)
                {
                    var actorList = FindObjectsByType<BehaviourActor>(FindObjectsSortMode.None);
                    _actor = actorList.FirstOrDefault(actor => actor.runtimeTree == tree);
                }

                _tree = tree;

                if (Application.isPlaying || AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID()))
                {
                    _inspectorView.Clear();
                    _treeView.OnGraphEditorView(_tree);
                }
            }
        }


        private BehaviourTree GetBehaviourTreeFromSelectedObject()
        {
            BehaviourTree tree = Selection.activeObject as BehaviourTree;

            if (Selection.activeGameObject != null)
            {
                GameObject targetObject = Selection.activeGameObject;
                _actor = targetObject.GetComponent<BehaviourActor>();
                tree ??= _actor?.runtimeTree;
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
