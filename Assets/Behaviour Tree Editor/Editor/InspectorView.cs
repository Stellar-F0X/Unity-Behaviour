using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal.VR;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

    private Editor _editor;

    public InspectorView() 
    {

    }

    //객체가 선택 되었을때 Action을 통해 호출될 함수로 인스펙터에 노드 정보를 그린다.
    public void UpdateSelection(NodeView view)
    {
        base.Clear();

        Object.DestroyImmediate(_editor);
        _editor = Editor.CreateEditor(view.node);

        IMGUIContainer container = new IMGUIContainer(() => {
            if (_editor.target == null)
            {
                return;
            }

            _editor.OnInspectorGUI();

        });
        base.Add(container);
    }
}
