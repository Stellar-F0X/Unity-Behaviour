using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    [CreateAssetMenu]
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        public bool debugMode;
    }
}
