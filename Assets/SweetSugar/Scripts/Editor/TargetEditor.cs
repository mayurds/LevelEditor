using System;
using Malee;
using Malee.Editor;
using UnityEditor;
using UnityEngine;
namespace SweetSugar.Scripts.Editor
{
public class TargetEditor : EditorWindow
{
    private static TargetEditor window;
    private SerializedObject so;
    private ReorderableList list;
    private Vector2 scrollPos;

    [MenuItem("Sweet Sugar/Target editor")]
    public static void Init()
    {

        // Get existing open window or if none, make a new one:
        window = (TargetEditor)GetWindow(typeof(TargetEditor));
        window.Show();
    }
    void OnEnable()
    {
        list = new ReorderableList( so.FindProperty("targets"));
    }

    void OnGUI()
    {
        so.Update();
        EditorGUILayout.BeginVertical();
        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
        GUILayout.Space(10);
        list.DoLayoutList();
//        GuiList.Show(targetObject.targets, () => {        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/SweetSugar/Resources/Levels/TargetEditorScriptable.asset");});

        GUILayout.Space(30);
        if (GUILayout.Button("Save"))
        {
            SaveSettings();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        so.ApplyModifiedProperties();
    }

        void SaveSettings()
        {
            AssetDatabase.SaveAssets();
        }
    }
}



