using UnityEngine;
using UnityEditor;
using System.Collections;

public class RenameTargetWindow : EditorWindow
{
    public SequencerTargetModel toRenameTargetModel;
    public SequencerData data;

    private string newName;

    public static void hide()
    {
        SequencerWindow.renameTargetWindowLive.Close();
        SequencerWindow.renameTargetWindowLive = null;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Original nickname: " + toRenameTargetModel.nickname);
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
        if (newName == null && newName.Length == 0)
        {
            Debug.Log("Name must be at least 1 character !");
            return;
        }

        //check if name is unique first
        foreach (SequencerTargetModel targetModel in data.targets)
        {
            if (targetModel.nickname == newName)
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
                bool wasUpdated = command.updateTargetReference(toRenameTargetModel.nickname, newName);

                if (wasUpdated)
                    numberOfRefrences += 1;
            }
        }

        toRenameTargetModel.nickname = newName;

        //print out rename data
        Debug.Log("Rename complete, there are " + numberOfRefrences + " commands that reference this target");
    }
}