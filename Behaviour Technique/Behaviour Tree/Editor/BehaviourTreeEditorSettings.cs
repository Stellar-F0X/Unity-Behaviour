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
        
        [Tooltip("Horizontal grid size nodes will snap to")]
        public int gridSnapSizeX = 15;

        [Tooltip("Vertical grid size nodes will snap to")]
        public int gridSnapSizeY = 225;
    }
}
