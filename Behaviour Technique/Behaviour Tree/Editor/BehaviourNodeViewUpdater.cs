using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourNodeViewUpdater : IDisposable
    {
        public BehaviourNodeViewUpdater(List<NodeView> nodes, Action updateAction = null)
        {
            _nodeViews = nodes.ToArray();
            _updateAction = updateAction;
        }
        
        private bool _alreadyDisposed = false;

        private NodeView[] _nodeViews;
        private Action _updateAction;

        
        public void UpdateViewsState(bool skipFrame = false)
        {
            if (skipFrame)
            {
                return;
            }
            
            for (int i = 0; i < _nodeViews.Length; ++i)
            {
                _nodeViews[i].UpdateState();
                _updateAction?.Invoke();
            }
        }

        
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool isDisposing)
        {
            if (_alreadyDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                _nodeViews = null;
                _updateAction = null;
            }

            _alreadyDisposed = true;
        }
    }
}
