using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyBehaviourController))]
public class EnemyBehaviourControllerEditor : Editor
{
    private SerializedProperty archetype;

    private SerializedProperty enemyController;
    private SerializedProperty stateController;
    private SerializedProperty detection;

    private SerializedProperty patrol;
    private SerializedProperty search;
    private SerializedProperty chase;

    private SerializedProperty red;
    private SerializedProperty blue;
    private SerializedProperty cyan;

    private SerializedProperty enableDebugLog;

    private void OnEnable()
    {
        archetype =
            serializedObject.FindProperty("archetype");

        enemyController =
            serializedObject.FindProperty("enemyController");

        stateController =
            serializedObject.FindProperty("stateController");

        detection =
            serializedObject.FindProperty("detection");

        patrol =
            serializedObject.FindProperty("patrol");

        search =
            serializedObject.FindProperty("search");

        chase =
            serializedObject.FindProperty("chase");

        red =
            serializedObject.FindProperty("red");

        blue =
            serializedObject.FindProperty("blue");

        cyan =
            serializedObject.FindProperty("cyan");

        enableDebugLog =
            serializedObject.FindProperty("enableDebugLog");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        DrawEnemyType();

        EditorGUILayout.Space();

        DrawCoreReferences();

        EditorGUILayout.Space();

        DrawCommonBehaviours();

        EditorGUILayout.Space();

        DrawArchetypeBehaviours();

        EditorGUILayout.Space();

        DrawDebug();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEnemyType()
    {
        EditorGUILayout.LabelField(
            "Enemy Type",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            archetype,
            new GUIContent("Archetype")
        );
    }

    private void DrawCoreReferences()
    {
        EditorGUILayout.LabelField(
            "Core References",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            enemyController
        );

        EditorGUILayout.PropertyField(
            stateController
        );

        EditorGUILayout.PropertyField(
            detection
        );
    }

    private void DrawCommonBehaviours()
    {
        EditorGUILayout.LabelField(
            "Common Behaviours",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            patrol
        );

        EditorGUILayout.PropertyField(
            search
        );

        EditorGUILayout.PropertyField(
            chase
        );
    }

    private void DrawArchetypeBehaviours()
    {
        EnemyArchetype selectedArchetype =
            (EnemyArchetype)archetype.enumValueIndex;

        switch (selectedArchetype)
        {
            case EnemyArchetype.Red:
                DrawRed();
                break;

            case EnemyArchetype.Blue:
                DrawBlue();
                break;

            case EnemyArchetype.Cyan:
                DrawCyan();
                break;

            case EnemyArchetype.Purple:
                DrawPurple();
                break;
        }
    }

    private void DrawRed()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Red Movement",
            EditorStyles.boldLabel
        );

        if (red != null)
        {
            EditorGUILayout.PropertyField(
                red,
                true
            );
        }
    }

    private void DrawBlue()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Blue Movement",
            EditorStyles.boldLabel
        );

        if (blue != null)
        {
            EditorGUILayout.PropertyField(
                blue,
                true
            );
        }
    }

    private void DrawCyan()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Cyan Movement",
            EditorStyles.boldLabel
        );

        if (cyan != null)
        {
            EditorGUILayout.PropertyField(
                cyan,
                true
            );
        }
    }

    private void DrawPurple()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Purple Movement",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Behavior khusus Purple belum dibuat.",
            MessageType.Info
        );
    }

    private void DrawDebug()
    {
        EditorGUILayout.LabelField(
            "Debug",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            enableDebugLog
        );
    }
}