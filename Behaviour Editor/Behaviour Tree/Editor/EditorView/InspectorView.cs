using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        private Editor _editor;
        
        
        public void ClearInspectorView()
        {
            base.Clear();
            Object.DestroyImmediate(_editor);
            _editor = null;
        }


        public void UpdateSelection(NodeView view)
        {
            base.Clear();

            Object.DestroyImmediate(_editor);
            _editor = Editor.CreateEditor(view.node);
            base.Add(new IMGUIContainer(DrawInspectorGUI));
        }


        private void DrawInspectorGUI()
        {
            if (_editor.target == null)
            {
                return;
            }

            _editor?.OnInspectorGUI();
        }
    }
}