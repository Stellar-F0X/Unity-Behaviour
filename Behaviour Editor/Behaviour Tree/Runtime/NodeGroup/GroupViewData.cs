using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem
{
    [Serializable]
    public class GroupViewData
    {
        public GroupViewData(string title, Vector2 position)
        {
            this.groupTitle = title;
            this.position = position;
            this.nodeGuids = new List<string>();
        }
        
        public string groupTitle;
        public Vector2 position;
        public List<string> nodeGuids;
    }
}