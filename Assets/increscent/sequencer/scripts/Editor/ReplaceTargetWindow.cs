using UnityEngine;
using UnityEditor;
using System.Collections;
    
public class ReplaceTargetWindow : EditorWindow
{
    public SequencerTargetModel toReplaceTargetModel;
    public SequencerData data;
        
    private SequencerTargetModel newTargetModel;

    private int popupSelection = 0;

    public static void hide()
    {
        SequencerWindow.replaceTargetWindowLive.Close();
        SequencerWindow.replaceTargetWindowLive = null;
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Original target nickname: " + toReplaceTargetModel.nickname);

                popupSelection = EditorGUILayout.Popup(popupSelection, data.getTargetNickNames());
                newTargetModel = data.targets [popupSelection]; 

                if (GUILayout.Button("Delete original & replace with: " + newTargetModel.nickname))
                {
                    doReplace();
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

    void doReplace()
    {
        int numberOfRefrences = 0;

        //rename all command target references
        foreach (SequencerSectionModel sectionModel in data.sections)
        {
            foreach (SequencerCommandBase command in sectionModel.commandList)
            {
                bool wasUpdated = command.updateTargetReference(toReplaceTargetModel.nickname, newTargetModel.nickname);
                
                if (wasUpdated)
                    numberOfRefrences += 1;
            }
        }
        
        data.targets.Remove(toReplaceTargetModel);
        
        //print out rename data
        Debug.Log("Deleted and replace complete, there are " + numberOfRefrences + " more commands that reference the new target");

        hide();
    }
}