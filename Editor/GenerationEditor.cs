using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(SolarSystemGenerator))]
public class GenerationEditor : Editor
{

    // a custom editor for our solar system generator

    SolarSystemGenerator ssGenerator;
    Editor editorMain;
    Editor editorBackup;

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        if (ssGenerator.settings != null)
        {
            if (GUILayout.Button("Create New Solar System"))
                ssGenerator.OnCreateNewPresssed();

            if (GUILayout.Button("Update Solar System"))
                ssGenerator.Generate();

            if (GUILayout.Button("Save Solar System"))
                ssGenerator.SaveSolarSystem();

            if (GUILayout.Button("Delete Solar System"))
                ssGenerator.OnDeleteSettingsPressed();
        }
        else
        {
            if (GUILayout.Button("Create Solar System"))
                ssGenerator.OnSettingsChanged();
        }

        
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (ssGenerator.settings != null)
            {
                CreateCachedEditor(ssGenerator.settings, null, ref editorMain);
                editorMain.OnInspectorGUI();
            } else
            {
                CreateCachedEditor(ssGenerator.randomSettings, null, ref editorBackup);
                editorBackup.OnInspectorGUI();
            }

            if (check.changed && ssGenerator.settings != null)
            {
                ssGenerator.OnSettingsChanged();
            }
            
        } 
        
    }

    private void OnEnable()
    {
        ssGenerator = (SolarSystemGenerator)target;
    }

}
