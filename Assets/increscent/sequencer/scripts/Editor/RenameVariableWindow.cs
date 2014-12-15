using UnityEngine;
using UnityEditor;
using System.Collections;

public class RenameVariableWindow : EditorWindow
{
    public string variableToRename;
    public SequencerData data;

    private string newName;

    public void hide()
    {
        if (SequencerWindow.renameVariableWindowLive != null)
        {
            SequencerWindow.renameVariableWindowLive.Close();
            SequencerWindow.renameVariableWindowLive = null;
        } else
        {
            this.Close();
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Original name: " + variableToRename);
                newName = EditorGUILayout.TextField(newName); 

                if (newName != null)
                    newName = newName.Trim();

                if (GUILayout.Button("Do Rename"))
                {
                    attemptRename();
                }

                if (GUILayout.Button("Close Window"))
                {
                    hide();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    void attemptRename()
    {
        //check if name is at least 1 character long
        if (newName == null || newName.Length == 0)
        {
            Debug.Log("Name must be at least 1 character !");
            return;
        }

        //check if name is unique first
        foreach (SequencerVariableModel variableModel in data.variables)
        {
            if (variableModel.name == newName)
            {
                Debug.LogWarning(" Nickname must be unique ! Can not rename! ");
                return;
            }
        }

        int numberOfRefrences = 0;

        //rename all command references
        foreach (SequencerSectionModel sectionModel in data.sections)
        {
            foreach (SequencerCommandBase command in sectionModel.commandList)
            {
                bool wasUpdated = command.updateVariableReference(variableToRename, newName);

                if (wasUpdated)
                    numberOfRefrences += 1;
            }
        }

        //rename the actual variable 
        foreach (SequencerVariableModel variableModel in data.variables)
        {
            if (variableModel.name == variableToRename)
            {
                variableModel.name = newName;
                variableToRename = newName;
            }
        }

        //print out rename data
        Debug.Log("Rename complete, there are " + numberOfRefrences + " commands that reference this variable");
    }
}