using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [CreateAssetMenu]
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        public VisualTreeAsset behaviourTreeEditorXml;
        public StyleSheet behaviourTreeStyle;
        public VisualTreeAsset nodeViewXml;
        public StyleSheet nodeViewStyle;
        public VisualTreeAsset blackboardPropertyViewXml;
        public StyleSheet blackboardPropertyViewStyle;
        
        [Space]
        public float enlargementScale = 2.5f;
        public float reductionScale = 0.2f;
    }
}
