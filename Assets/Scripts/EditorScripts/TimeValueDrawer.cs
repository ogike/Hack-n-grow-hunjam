using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace EditorScripts
{
    [CustomPropertyDrawer(typeof(TimeValue))]
    public class TimeValueDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                
                var frames = property.FindPropertyRelative("frames");
                EditorGUILayout.PropertyField(frames);

                EditorGUILayout.LabelField(frames.intValue * FrameTime.FrameTimeSeconds + "s");
            }
            

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;

    }
}
