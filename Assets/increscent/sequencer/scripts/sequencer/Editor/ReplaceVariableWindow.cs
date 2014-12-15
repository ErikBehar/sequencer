using UnityEngine;
using UnityEditor;
using System.Collections;
    
public class ReplaceVariableWindow : EditorWindow
{
    public string variableToReplace;
    public SequencerData data;
        
    private string newVariableName;

    private int popupSelection = 0;

    private bool shouldNotRefresh = false;

    void OnEnable()
    {
        shouldNotRefresh = false;
    }

    public void hide()
    {
        if (SequencerWindow.replaceVariableWindowLive != null)
        {
            SequencerWindow.replaceVariableWindowLive.Close();
            SequencerWindow.replaceVariableWindowLive = null;
        } else
        {
            this.Close();
        }
    }
    
    void OnGUI()
    {
        if (shouldNotRefresh)
            return;

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Original target nickname: " + variableToReplace);

                popupSelection = EditorGUILayout.Popup(popupSelection, data.getNamesOfVariables());
                newVariableName = data.getNamesOfVariables() [popupSelection]; 

                if (GUILayout.Button("Delete original & replace with: " + variableToReplace))
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
        shouldNotRefresh = true;

        int numberOfRefrences = 0;

        //rename all command variable references
        foreach (SequencerSectionModel sectionModel in data.sections)
        {
            foreach (SequencerCommandBase command in sectionModel.commandList)
            {
                bool wasUpdated = command.updateVariableReference(variableToReplace, newVariableName);
                
                if (wasUpdated)
                    numberOfRefrences += 1;
            }
        }

        //delete the variable now
        data.variables.RemoveAt(data.getIndexOfVariable(variableToReplace));

        //print out rename data
        Debug.Log("Delete and replace complete, there are " + numberOfRefrences + " commands that reference the new variable");

        hide();
    }
}