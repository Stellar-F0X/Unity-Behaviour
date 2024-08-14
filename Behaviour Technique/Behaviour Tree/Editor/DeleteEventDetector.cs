using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public sealed class DeleteEventDetector
    {
        private Action<BehaviourActor> _cachedDeleteEvent;

        public void RegisterCallback(INodeViewDeletable deletable)
        {
            if (deletable is NodeView nodeView)
            {
                nodeView.RegisterCallback<DetachFromPanelEvent>(OnElementDetached);

                _cachedDeleteEvent -= deletable.OnNodeDeletedEvent;
                _cachedDeleteEvent += deletable.OnNodeDeletedEvent;
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
            var actors = Object.FindObjectsByType<BehaviourActor>(FindObjectsSortMode.None);
            _cachedDeleteEvent?.Invoke(actors.FirstOrDefault(actor => actor?.runtimeTree == BehaviourTreeEditorWindow.Editor?.Tree));
        }
    }
}
