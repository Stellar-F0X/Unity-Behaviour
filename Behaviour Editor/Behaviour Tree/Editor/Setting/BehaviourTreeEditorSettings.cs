using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        [Header("Grid Options")]
        public float enlargementScale = 2.5f;

        public float reductionScale = 0.2f;

        [Header("Highlight Options")]
        public float minimumFocusingDuration = 0.5f;

        public Color nodeGroupColor = new Color(65f, 65f, 65f, 1f);

        public Color nodeAppearingColor = new Color32(54, 154, 204, 255);
        public Color nodeDisappearingColor = new Color32(24, 93, 125, 255);

        public Color edgeAppearingColor = new Color32(54, 154, 204, 255);
        public Color edgeDisappearingColor = new Color32(200, 200, 200, 255);

        public Color portAppearingColor = new Color32(54, 154, 204, 255);
        public Color portDisappearingColor = new Color32(200, 200, 200, 255);

        [Header("Runtime Options")]
        public uint maxUpdateRate = 240;

        [Header("Layout References")]
        public VisualTreeAsset behaviourTreeEditorXml;

        public StyleSheet behaviourTreeStyle;
        public VisualTreeAsset nodeViewXml;
        public StyleSheet nodeViewStyle;
        public VisualTreeAsset blackboardPropertyViewXml;
        public StyleSheet blackboardPropertyViewStyle;
    }
}
