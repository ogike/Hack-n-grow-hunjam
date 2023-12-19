using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Enemy.States;

namespace EditorScripts
{
    
    [CustomPropertyDrawer(typeof(AIStateMecanimInfo))]
    public class AIStateAnimationInfo : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Animation data for state");

                using (new GUILayout.HorizontalScope())
                {
                    var exitsByDefault = property.FindPropertyRelative("exitsByDefault");
                    EditorGUILayout.PropertyField(
                        exitsByDefault,
                        new GUIContent("Exits state after time"));
                    
                    if (exitsByDefault.boolValue)
                    {
                        EditorGUILayout.PropertyField(
                            property.FindPropertyRelative("stateTime"),
                            GUIContent.none);
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    var entryEventType = property.FindPropertyRelative("animatorEntryEventType");
                    EditorGUILayout.PropertyField(
                            entryEventType,
                        new GUIContent("Event on state entry"));

                    if (entryEventType.enumValueIndex != (int)AnimatorEntryEventType.None)
                    {
                        // EditorGUILayout.Space();
                        EditorGUIUtility.labelWidth = 5; // so small it wont appear

                        EditorGUILayout.PropertyField(
                            property.FindPropertyRelative("animatorEntryTrigger"),
                            new GUIContent("Event name"));
                        
                        EditorGUIUtility.labelWidth = 0; //resetting to default value

                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;
    }

}
