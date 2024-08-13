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
            if (deletable is NodeView nodeView)
            {
                nodeView.RegisterCallback<DetachFromPanelEvent>(OnElementDetached);

                _cachedDeleteEvent -= deletable.OnDeletedElementEvent;
                _cachedDeleteEvent += deletable.OnDeletedElementEvent;
            }
        }
        
        public void UnregisterCallback(INodeViewDeletable deletable)
        {
            if (deletable is NodeView nodeView)
            {
                nodeView.UnregisterCallback<DetachFromPanelEvent>(OnElementDetached);
                _cachedDeleteEvent = null;
            }
        }
        
        private void OnElementDetached(DetachFromPanelEvent evt)
        {
            _cachedDeleteEvent?.Invoke(this);
        }
    }
}
