using System.Linq;
using BehaviourTechnique.BehaviourTreeEditor.Setting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviourTechnique.UIElements;
using UnityEditor.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Behaviour Tree Editor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BT Editor");
            Instance         = wnd;
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


        public static BehaviourTreeEditorWindow Instance
        {
            get;
            private set;
        }

        public static BehaviourTreeEditorSettings Settings
        {
            get { return BTEditorUtility.FindAssetByName<BehaviourTreeEditorSettings>($"t: {nameof(BehaviourTreeEditorSettings)}"); }
        }

        private BehaviourTree _tree;
        private BehaviourActor _actor;

        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;
        private BlackboardPropertyViewList _blackboardPropList;


        public bool CanEditTree
        {
            get;
            private set;
        }

        public BehaviourTreeView View
        {
            get { return _treeView; }
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
        }


        private void Update()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_actor is null)
            {
                return;
            }

            _treeView.UpdateNodeView();
        }


        private void CreateGUI()
        {
            Settings.behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(Settings.behaviourTreeStyle);

            Instance                 =  this;
            _inspectorView           =  rootVisualElement.Q<InspectorView>();
            _treeView                =  rootVisualElement.Q<BehaviourTreeView>();
            _blackboardPropList      =  rootVisualElement.Q<BlackboardPropertyViewList>();
            _treeView.onNodeSelected += _inspectorView.UpdateSelection;
            
            this.OnSelectionChange();
            this._blackboardPropList.SetUp(rootVisualElement.Q<ToolbarMenu>("add-element-button"));
        }


        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode: this.OnSelectionChange(); break;

                case PlayModeStateChange.EnteredPlayMode: this.OnSelectionChange(); break;
            }
        }
        

        private void OnSelectionChange()
        {
            switch (Selection.activeObject)
            {
                case BehaviourTree treeObj: _tree = treeObj; break;

                case GameObject gameObj: _tree = gameObj.TryGetComponent(out _actor) ? _actor.runtimeTree : null; break;
            }
            
            CanEditTree = false;
            
            if (_tree is not null)
            {
                CanEditTree = Application.isPlaying == false;
                
                bool openedEditorWindow = AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID());

                if (_actor is not null && Application.isPlaying || openedEditorWindow)
                {
                    _blackboardPropList?.ClearBlackboardPropertyViews();
                    _blackboardPropList?.ChangeBlackboardData(_tree.blackboardData);
                    
                    _inspectorView.Clear();
                    _treeView.OnGraphEditorView(_tree);
                }
            }
        }
    }
}