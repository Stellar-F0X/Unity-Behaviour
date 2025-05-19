using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditor : EditorWindow
    {
        [MenuItem("Tools/Behaviour Tree Editor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BT Editor");
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


        private static BehaviourTreeEditorSettings _settings;

        public static BehaviourTreeEditor Instance
        {
            get;
            private set;
        }

        public static BehaviourTreeEditorSettings Settings
        {
            get
            {
                if (_settings is null)
                {
                    string filter = $"t:{nameof(BehaviourTreeEditorSettings)}";

                    _settings = EditorHelper.FindAssetByName<BehaviourTreeEditorSettings>(filter);
                }

                return _settings;
            }
        }

        public static bool CanEditTree
        {
            get;
            private set;
        }

        public static bool isInLoadingBTAsset
        {
            get;
            private set;
        }


        private BehaviourTree _tree;
        private BehaviourActor _actor;

        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;
        private BlackboardPropertyListView _blackboardProp;


        public BehaviourTree Tree
        {
            get { return _tree; }
        }

        public BehaviourTreeView View
        {
            get { return _treeView; }
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            Undo.undoRedoPerformed += this.BehaviourEditorUndoPerformed;

            EditorApplication.update -= this.RuntimeUpdate;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            
            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            
            EditorApplication.update -= this.RuntimeUpdate;
        }


        /// <summary> 새로운 Behaviour Tree Asset이 추가되거나 제거될 때 호출됨. </summary>
        private void OnProjectChange()
        {
            if (_tree is not null && AssetDatabase.Contains(_tree) == false)
            {
                //상호 연관이 적은 것부터 삭제.
                this._blackboardProp.ClearBlackboardView();
                this._inspectorView.Clear();
                this._treeView?.ClearEditorView();

                CanEditTree = false;
                this._actor = null;
                this._tree = null;
            }
            else
            {
                EditorApplication.delayCall -= this.OnSelectionChange;
                EditorApplication.delayCall += this.OnSelectionChange;
            }
        }
        
        

        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false || _treeRunner is null || _treeRunner.runtimeTree is null)
            {
                return;
            }
            
            _treeView.UpdateNodeView();
        }



        private void BehaviourEditorUndoPerformed()
        {
            _treeView?.OnGraphEditorView(_tree);
            _inspectorView?.ClearInspectorView();
            AssetDatabase.SaveAssets();
        }



        private void CreateGUI()
        {
            Settings.behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(Settings.behaviourTreeStyle);

            Instance = this;

            _treeView = rootVisualElement.Q<BehaviourTreeView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();

            _blackboardProp = rootVisualElement.Q<BlackboardPropertyListView>();
            _blackboardProp.Setup(rootVisualElement.Q<ToolbarMenu>("add-element-button"));

            _treeView.popupSearchField = rootVisualElement.Q<ToolbarPopupSearchField>("search-node-field");
            _treeView.popupSearchField.RegisterValueChangedCallback(_treeView.SearchNodeByNameOrTag);
            _treeView.onNodeSelected += _inspectorView.UpdateSelection;

            this.OnSelectionChange();
        }


        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode: 
                    EditorApplication.update -= this.RuntimeUpdate;
                    this.OnSelectionChange();
                    break;

                case PlayModeStateChange.EnteredPlayMode: 
                    EditorApplication.update += this.RuntimeUpdate;
                    this.OnSelectionChange(); 
                    break;
            }
        }


        private void OnSelectionChange()
        {
            switch (Selection.activeObject)
            {
                case BehaviourTree treeObj: _tree = treeObj; break;

                case GameObject gameObj: _tree = gameObj.TryGetComponent(out _actor) ? _actor.runtimeTree : null; break;

                default: return;
            }

            CanEditTree = false;

            if (_tree is not null)
            {
                CanEditTree = Application.isPlaying == false;

                bool openedEditorWindow = AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID());

                if (_actor is not null && Application.isPlaying || openedEditorWindow)
                {
                    isInLoadingBTAsset = true;

                    _inspectorView?.ClearInspectorView();
                    _blackboardProp?.ClearBlackboardView();

                    _treeView?.OnGraphEditorView(_tree);
                    _blackboardProp?.OnBehaviourTreeChanged(_tree);

                    isInLoadingBTAsset = false;
                }
            }
        }
    }
}