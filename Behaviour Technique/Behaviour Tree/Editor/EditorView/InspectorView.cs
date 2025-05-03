using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using BehaviourTechnique.BehaviourTreeEditor;
using UnityEditor.UIElements;

namespace BehaviourTechnique.UIElements
{
    public class InspectorView : InspectorElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        private Editor _editor;

        public InspectorView() { }


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