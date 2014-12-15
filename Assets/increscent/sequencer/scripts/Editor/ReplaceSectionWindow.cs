using UnityEngine;
using UnityEditor;
using System.Collections;
    
public class ReplaceSectionWindow : EditorWindow
{
    public string toReplaceSectionName;
    public SequencerData data;
        
    private string newSectionName;

    private int popupSelection = 0;

    private bool dontDrawAnymore = false;

    public void hide()
    {
        if (SequencerWindow.replaceSectionWindowLive != null)
        {
            SequencerWindow.replaceSectionWindowLive.Close();
            SequencerWindow.replaceSectionWindowLive = null;
        } else
        {
            this.Close();
        }
    }

    void OnEnable()
    {
        dontDrawAnymore = false;
    }
    
    void OnGUI()
    {
        if (dontDrawAnymore)
            return;

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Original section name: " + toReplaceSectionName);

                popupSelection = EditorGUILayout.Popup(popupSelection, data.getSectionNames());
                newSectionName = data.getSectionNames() [popupSelection]; 

                if (GUILayout.Button("Delete original & replace with: " + newSectionName))
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
        dontDrawAnymore = true;

        int numberOfRefrences = 0;

        //rename all command section references
        foreach (SequencerSectionModel sectionModel in data.sections)
        {
            foreach (SequencerCommandBase command in sectionModel.commandList)
            {
                bool wasUpdated = command.updateSectionReference(toReplaceSectionName, newSectionName);
                
                if (wasUpdated)
                    numberOfRefrences += 1;
            }
        }
        
        data.sections.Remove(data.getSectionModel(toReplaceSectionName));
        
        //print out replace data
        Debug.Log("Delete and replace complete, there are " + numberOfRefrences + " commands that reference the new section");

        hide();
    }
}