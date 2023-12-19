using UnityEditor;
using UnityEngine;

namespace EditorScripts
{
    public class Utility
    {
        /// <summary>
        /// Code from Unity Forums user ModLunar
        /// https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
        /// </summary>
        public static void DrawHorizontalGUILine(int height = 1) {
            GUILayout.Space(4);
     
            Rect rect = GUILayoutUtility.GetRect(10, height, GUILayout.ExpandWidth(true));
            rect.height = height;
            rect.xMin = EditorGUI.indentLevel * 15;
            rect.xMax = EditorGUIUtility.currentViewWidth - (EditorGUI.indentLevel * 15);
     
            Color lineColor = new Color(0.10196f, 0.10196f, 0.10196f, 1);
            EditorGUI.DrawRect(rect, lineColor);
            GUILayout.Space(4);
        }
    }
}
