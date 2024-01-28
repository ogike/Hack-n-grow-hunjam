using System;
using Enemy;
using Enemy.States;
using UnityEditor;
using UnityEngine;

namespace EditorScripts
{
    [CustomEditor(typeof(EnemyAI))]
    [CanEditMultipleObjects]
    public class AIStateDrawer : Editor
    {
        private SerializedObject so;
        
        private SerializedProperty stateMoveProp; 
        
        private SerializedProperty stateAttackWindupProp;
        private SerializedProperty stateAttackMainProp;
        private SerializedProperty stateAttackWinddownProp;
        
        private SerializedProperty stateKnockBackProp;
        
        private SerializedProperty animatorProp;

        private void OnEnable()
        {
            so = serializedObject;
            stateMoveProp = so.FindProperty( "stateMove" );
            stateAttackWindupProp = so.FindProperty( "stateAttackWindup" );
            stateAttackMainProp = so.FindProperty( "stateAttackMain" );
            stateAttackWinddownProp = so.FindProperty( "stateAttackWinddown" );
            stateKnockBackProp = so.FindProperty( "stateKnockBack" );
            animatorProp = so.FindProperty( "animator" );
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            // using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            // {
            //     EditorGUI.indentLevel++;
            //     EditorGUILayout.PropertyField(stateMoveProp);
            //     EditorGUI.indentLevel--;
            // }
            //
            // Utility.DrawHorizontalGUILine();
            //
            // using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            // {
            //     EditorGUI.indentLevel++;
            //     
            //     EditorGUILayout.PropertyField( stateAttackWindupProp );
            //     Utility.DrawHorizontalGUILine();
            //
            //     EditorGUILayout.PropertyField( stateAttackMainProp );
            //     Utility.DrawHorizontalGUILine();
            //
            //     EditorGUILayout.PropertyField( stateAttackWinddownProp );
            //
            //     EditorGUI.indentLevel--;
            // }
            //
            // Utility.DrawHorizontalGUILine();
            //
            // using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            // {
            //     EditorGUI.indentLevel++;
            //     EditorGUILayout.PropertyField(stateKnockBackProp);
            //     EditorGUI.indentLevel--;
            // }
            //
            // Utility.DrawHorizontalGUILine();
            //
            // EditorGUILayout.PropertyField( animatorProp );

            so.ApplyModifiedProperties();
        }
    }
}
