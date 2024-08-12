using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class DeleteEventDetector
    {
        public DeleteEventDetector(NodeView nodeView)
        {
            _currentElement = nodeView;
            
            removeCallback -= UnregistryCallback;
            removeCallback += UnregistryCallback;
        }

        
        public static Action removeCallback;

        private VisualElement _currentElement;
        private Action<DeleteEventDetector> _cachedDeleteEvent;

        public void RegisterDetectedElement(Action<DeleteEventDetector> evt)
        {
            _currentElement.RegisterCallback<DetachFromPanelEvent>(OnElementDetached);

            _cachedDeleteEvent -= evt;
            _cachedDeleteEvent += evt;
        }

        public void UnregistryCallback()
        {
            _currentElement.UnregisterCallback<DetachFromPanelEvent>(OnElementDetached);
            _cachedDeleteEvent = null;
        }
        
        private void OnElementDetached(DetachFromPanelEvent evt)
        {
            _cachedDeleteEvent?.Invoke(this);
        }
    }
}
