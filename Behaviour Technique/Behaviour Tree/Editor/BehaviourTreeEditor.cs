using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using StateMachine.BT;

public class BehaviourTreeEditor : EditorWindow
{
    public static bool Activated
    {
        get { return _isEditorAvailable && !Application.isPlaying; }
    }

    private static bool _isEditorAvailable;

    private BehaviourTree _tree;
    private BehaviourTreeView _treeView;
    private InspectorView _inspectorView;


    [MenuItem("Tools/BehaviourTreeEditor")]
    private static void OpenWindow()
    {
        BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
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


    public static string FindEditorGraphicAssetFolder(string searchFilter, string folderPath)
    {
        foreach (var guid in AssetDatabase.FindAssets(searchFilter) ?? Enumerable.Empty<string>())
        {
            string parentPath = AssetDatabase.GUIDToAssetPath(guid);
            string resultPath = $"{parentPath}{folderPath}";

            if (Directory.Exists(resultPath))
            {
                return resultPath;
            }
        }

        return string.Empty;
    }


    private void CreateGUI()
    {
        string basePath = FindEditorGraphicAssetFolder("Behaviour Technique t:Folder", "/Behaviour Tree/Layout");

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(basePath + "/BehaviourTreeEditor.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(basePath + "/BehaviourTreeEditorStyle.uss");

        visualTree.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(styleSheet);

        _treeView = rootVisualElement.Q<BehaviourTreeView>();
        _inspectorView = rootVisualElement.Q<InspectorView>();

        _treeView.OnNodeSelected += OnNodeSelectionChanged;

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
            case PlayModeStateChange.EnteredEditMode: this.OnSelectionChange(); break;
            case PlayModeStateChange.EnteredPlayMode: this.OnSelectionChange(); break;

            case PlayModeStateChange.ExitingEditMode: break;
            case PlayModeStateChange.ExitingPlayMode: break;
        }
    }


    private void OnSelectionChange()
    {
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
