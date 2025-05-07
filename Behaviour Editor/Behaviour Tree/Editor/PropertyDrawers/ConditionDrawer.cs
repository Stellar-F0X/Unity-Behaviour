using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(BlackboardBasedCondition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private readonly List<IBlackboardProperty> _cachedPropertyList = new List<IBlackboardProperty>();
        
        private const int _propertyFieldWidth = 50;
        
        private bool _canDraw = false;
        
        private Rect _rect;
        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }

        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviourTreeEditorWindow.Instance is null || BehaviourTreeEditorWindow.Instance.CanEditTree == false)
            {
                return;
            }

            BlackboardData data = BehaviourTreeEditorWindow.Instance.Tree.blackboardData;
            SerializedProperty blackboardProp = property.FindPropertyRelative("property");

            _rect = new Rect(position.x, position.y, position.width - 10, EditorGUIUtility.singleLineHeight);

            Rect dropdownRect = new Rect(_rect.x, _rect.y + 2, _rect.width / 3, _rect.height);
            Rect compareRect = new Rect(_rect.x + _rect.width / 3 + 3, _rect.y + 2, _rect.width / 3, _rect.height);
            Rect valueRect = new Rect(_rect.x + 2 * _rect.width / 3 + 6, _rect.y + 2, _rect.width / 3, _rect.height);

            this.DrawBlackboardProperty(data, blackboardProp, dropdownRect);

            if (_canDraw)
            {
                this.DrawCompareValueField(property, blackboardProp, compareRect, valueRect);
            }
        }

        
        private void DrawBlackboardProperty(BlackboardData data, SerializedProperty blackboardProp, Rect dropdownRect)
        {
            if (data.Count == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("No blackboard properties found.", warningIcon.image));
                _canDraw = false;
                return;
            }

            IBlackboardProperty[] properties = this.GetUsableBlackboardProperties(data);

            if (properties.Length == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("Cannot find a blackboard property with the given key ID.", warningIcon.image));
                _canDraw = false;
                return;
            }

            string[] dropdownOptions = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                dropdownOptions[i] = properties[i].key;
            }

            int selected = 0;

            if (blackboardProp.boxedValue is IBlackboardProperty prop)
            {
                selected = Array.IndexOf(dropdownOptions, prop.key);
            }

            selected                  = Mathf.Clamp(selected, 0, dropdownOptions.Length - 1);
            selected                  = EditorGUI.Popup(dropdownRect, selected, dropdownOptions);
            blackboardProp.boxedValue = properties[selected];
            _canDraw                  = true;
        }

        
        private void DrawCompareValueField(SerializedProperty property, SerializedProperty blackboardProp, Rect compareRect, Rect valueRect)
        {
            SerializedProperty targetValue         = property.FindPropertyRelative("comparableValue");
            SerializedProperty sourceValueTypeName = blackboardProp.FindPropertyRelative("_propertyTypeName");

            if (targetValue.boxedValue is null)
            {
                targetValue.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
            }
            else
            {
                SerializedProperty targetValueTypeName = targetValue.FindPropertyRelative("_propertyTypeName");

                if (string.Compare(targetValueTypeName.stringValue, sourceValueTypeName.stringValue, StringComparison.Ordinal) != 0)
                {
                    targetValue.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
                }
            }

            this.DrawCompareCondition(property, blackboardProp.boxedValue as IBlackboardProperty, compareRect);

            EditorGUI.PropertyField(valueRect, targetValue.FindPropertyRelative("_value"), GUIContent.none);
        }


        private void DrawCompareCondition(SerializedProperty property, IBlackboardProperty sourceType, Rect compareRect)
        {
            SerializedProperty conditionType  = property.FindPropertyRelative("conditionType");
            int                selected       = conditionType.enumValueIndex;
            List<string>       conditionTypes = new List<string>();
            
            for (int i = (int)EConditionType.None; i < (int)sourceType.comparableConditions; i <<= 1)
            {
                EConditionType condition = (EConditionType)i;
                
                if ((condition & sourceType.comparableConditions) == condition)
                {
                    conditionTypes.Add(condition.ToString());
                }
            }

            selected                     = Mathf.Clamp(selected, 0, conditionTypes.Count - 1);
            conditionType.enumValueIndex = EditorGUI.Popup(compareRect, selected, conditionTypes.ToArray());
        }


        private IBlackboardProperty[] GetUsableBlackboardProperties(BlackboardData data)
        {
            _cachedPropertyList.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                IBlackboardProperty prop = data.GetProperty(i);

                if ((prop.comparableConditions & EConditionType.None) == EConditionType.None)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prop.key) == false)
                {
                    _cachedPropertyList.Add(prop);
                }
            }

            return _cachedPropertyList.ToArray();
        }
    }
}