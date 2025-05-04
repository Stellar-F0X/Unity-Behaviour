using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private static readonly string[] BoolConditionTypes = new[] { nameof(EConditionType.Equal), nameof(EConditionType.NotEqual) };

        private static readonly string[] NumbericConditionTypes = Enum.GetNames(typeof(EConditionType));

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
            SerializedProperty sourceType = blackboardProp.FindPropertyRelative("_propertyType");
            SerializedProperty targetValue = property.FindPropertyRelative("comparableValue");

            if (targetValue.boxedValue is null)
            {
                this.AllocateBlackboardProperty(sourceType, targetValue);
            }
            else
            {
                SerializedProperty targetValueType = targetValue.FindPropertyRelative("_propertyType");

                if (targetValueType.enumValueIndex != sourceType.enumValueIndex)
                {
                    this.AllocateBlackboardProperty(sourceType, targetValue);
                }
            }

            this.DrawCompareCondition(property, sourceType, compareRect);

            this.DrawComparablePropertyField(sourceType, targetValue.FindPropertyRelative("_value"), valueRect);
        }

        private void DrawCompareCondition(SerializedProperty property, SerializedProperty sourceType, Rect compareRect)
        {
            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
            int selected = conditionType.enumValueIndex;

            if ((EBlackboardPropertyType)sourceType.enumValueIndex == EBlackboardPropertyType.Bool)
            {
                selected                     = Mathf.Clamp(selected, 0, BoolConditionTypes.Length - 1);
                conditionType.enumValueIndex = EditorGUI.Popup(compareRect, selected, BoolConditionTypes);
            }
            else
            {
                selected                     = Mathf.Clamp(selected, 0, NumbericConditionTypes.Length - 1);
                conditionType.enumValueIndex = EditorGUI.Popup(compareRect, selected, NumbericConditionTypes);
            }
        }

        private IBlackboardProperty[] GetUsableBlackboardProperties(BlackboardData data)
        {
            _cachedPropertyList.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                IBlackboardProperty prop = data.GetProperty(i);

                if (prop.propertyType is EBlackboardPropertyType.None or EBlackboardPropertyType.Object)
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

        private void DrawComparablePropertyField(SerializedProperty sourceType, SerializedProperty prop, Rect valueRect)
        {
            switch ((EBlackboardPropertyType)sourceType.enumValueIndex)
            {
                case EBlackboardPropertyType.Int: prop.intValue = EditorGUI.IntField(valueRect, prop.intValue); break;

                case EBlackboardPropertyType.Bool: prop.boolValue = EditorGUI.Toggle(valueRect, prop.boolValue); break;

                case EBlackboardPropertyType.Float: prop.floatValue = EditorGUI.FloatField(valueRect, prop.floatValue); break;
            }
        }

        private void AllocateBlackboardProperty(SerializedProperty sourceType, SerializedProperty targetValue)
        {
            switch ((EBlackboardPropertyType)sourceType.enumValueIndex)
            {
                case EBlackboardPropertyType.Int: targetValue.boxedValue = new BlackboardProperty<int>("", 0, EBlackboardPropertyType.Int); break;

                case EBlackboardPropertyType.Bool: targetValue.boxedValue = new BlackboardProperty<bool>("", false, EBlackboardPropertyType.Bool); break;

                case EBlackboardPropertyType.Float: targetValue.boxedValue = new BlackboardProperty<float>("", 0f, EBlackboardPropertyType.Float); break;
            }
        }
    }
}