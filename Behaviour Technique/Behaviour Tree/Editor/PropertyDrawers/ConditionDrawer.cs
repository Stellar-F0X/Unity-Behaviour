using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private static readonly string[] BoolConditionTypes = new[] { nameof(EConditionType.Equal), nameof(EConditionType.NotEqual) };

        private static readonly string[] NumbericConditionTypes = Enum.GetNames(typeof(EConditionType));

        private readonly List<IBlackboardProperty> _cachedPropertyList = new List<IBlackboardProperty>();

        private bool _canDraw = false;

        private const int _propertyFieldWidth = 50;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviourTreeEditorWindow.Instance is null || BehaviourTreeEditorWindow.Instance.CanEditTree == false)
            {
                return;
            }

            BlackboardData data = BehaviourTreeEditorWindow.Instance.Tree.blackboardData;
            SerializedProperty blackboardProp = property.FindPropertyRelative("property");
            //EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                this.DrawBlackboardProperty(data, blackboardProp);

                if (_canDraw)
                {
                    this.DrawCompareValueField(property, blackboardProp);
                }
            }
        }


        private void DrawBlackboardProperty(BlackboardData data, SerializedProperty blackboardProp)
        {
            if (data.Count == 0)
            {
                EditorGUILayout.LabelField("No blackboard properties found.");
                _canDraw = false;
                return;
            }

            IBlackboardProperty[] properties = this.GetUsableBlackboardProperties(data);

            if (properties.Length == 0)
            {
                EditorGUILayout.LabelField("Cannot find a blackboard property with the given key.");
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
            selected                  = EditorGUILayout.Popup(selected, dropdownOptions);
            blackboardProp.boxedValue = properties[selected];
            _canDraw                  = true;
        }


        private void DrawCompareValueField(SerializedProperty property, SerializedProperty blackboardProp)
        {
            SerializedProperty sourceType = blackboardProp.FindPropertyRelative("_propertyType");
            SerializedProperty targetValue = property.FindPropertyRelative("comparableValue");

            if (targetValue.boxedValue is null)
            {
                //source의 Type은 블랙보드의 프로퍼티 생성때 정해지는데 None 선택칸이 없으므로 None일 수 없다.
                //더불어, DrawBlackboardProperty 함수에서 Object 타입을 필터링 하므로 Object 타입도 Condition의 타입으로 설정될 수 없음.
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

            this.DrawCompareCondition(property, sourceType);

            this.DrawComparablePropertyField(sourceType, targetValue.FindPropertyRelative("_value"));
        }


        private void DrawCompareCondition(SerializedProperty property, SerializedProperty sourceType)
        {
            EditorGUILayout.Space(3);

            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
            int selected = conditionType.enumValueIndex;

            if ((EBlackboardPropertyType)sourceType.enumValueIndex == EBlackboardPropertyType.Bool)
            {
                selected                     = Mathf.Clamp(selected, 0, BoolConditionTypes.Length - 1);
                conditionType.enumValueIndex = EditorGUILayout.Popup(selected, BoolConditionTypes);
            }
            else
            {
                selected                     = Mathf.Clamp(selected, 0, NumbericConditionTypes.Length - 1);
                conditionType.enumValueIndex = EditorGUILayout.Popup(selected, NumbericConditionTypes);
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


        private void DrawComparablePropertyField(SerializedProperty sourceType, SerializedProperty prop)
        {
            EditorGUILayout.Space(3);

            switch ((EBlackboardPropertyType)sourceType.enumValueIndex)
            {
                case EBlackboardPropertyType.Int: prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(_propertyFieldWidth)); break;

                case EBlackboardPropertyType.Bool: prop.boolValue = EditorGUILayout.Toggle(prop.boolValue, GUILayout.Width(_propertyFieldWidth)); break;

                case EBlackboardPropertyType.Float: prop.floatValue = EditorGUILayout.FloatField(prop.floatValue, GUILayout.Width(_propertyFieldWidth)); break;
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