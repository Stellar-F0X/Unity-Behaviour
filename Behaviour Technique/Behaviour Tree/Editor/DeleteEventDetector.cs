using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public sealed class DeleteEventDetector
    {
        private Action<DeleteEventDetector> _cachedDeleteEvent;

        public void RegisterCallback(INodeViewDeletable deletable)
        {
            (deletable as NodeView)?.RegisterCallback<DetachFromPanelEvent>(OnElementDetached);

            _cachedDeleteEvent -= deletable.OnDeletedElementEvent;
            _cachedDeleteEvent += deletable.OnDeletedElementEvent;
        }
        
        public void UnregisterCallback(INodeViewDeletable deletable)
        {
            (deletable as NodeView)?.UnregisterCallback<DetachFromPanelEvent>(OnElementDetached);
            _cachedDeleteEvent = null;
        }
        
        private void OnElementDetached(DetachFromPanelEvent evt)
        {
            _cachedDeleteEvent?.Invoke(this);
        }
    }
}
