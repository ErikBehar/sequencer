using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Set Variable : Sequencer Command
// Creates or sets an already existing variable from user input 
/// </summary>

[Serializable]
public class SC_InputVariable : SequencerCommandBase
{
    public override string commandId{ get { return "varSet"; } }

    public override string commandType{ get { return "base"; } }

    public string variableName = "";
    public string variableValue = "";
    private string previousValue;
    private int varIndex = 0;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {
        SC_InputVariable newCmd = ScriptableObject.CreateInstance(typeof(SC_InputVariable)) as SC_InputVariable;
        newCmd.variableName = variableName;
        newCmd.variableValue = variableValue;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        
        if (myPlayer.inRewindMode)
        {
            undo();
        } else
        {
            if (myPlayer.runningTimeVariablesDictionary.ContainsKey(variableName))
                previousValue = myPlayer.runningTimeVariablesDictionary [variableName];
            else
                myPlayer.runningTimeVariablesDictionary.Add(variableName, "");
            
            myPlayer.inputController.showInputFor(variableName, myPlayer);
        }

        myPlayer.callBackFromCommand();
    }
    
    override public void undo()
    {
        if (myPlayer.runningTimeVariablesDictionary.ContainsKey(variableName))
        {
            myPlayer.runningTimeVariablesDictionary [variableName] = previousValue;
        } else
        {
            myPlayer.runningTimeVariablesDictionary.Add(variableName, previousValue);
        }

        myPlayer.inputController.hideInput();
    }

    override public void forward(SequencePlayer player)
    {
        //nothing, it blocks
        myPlayer.blockForward = true;
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    }

    #if UNITY_EDITOR
    override public void drawCustomUi()
    {
        if (sequencerData.variables.Count > 0)
        {
            string[] existingVariables = sequencerData.getNamesOfVariables();
            
            varIndex = EditorGUILayout.Popup(varIndex, existingVariables); 
    
            if (GUILayout.Button("set from popup"))
            {    
                variableName = existingVariables [varIndex];
                variableValue = sequencerData.variables [varIndex].value; //not sure we need to do this but ok
            }
        }

        GUILayout.Label("Variable Name to use for input:");
        variableName = EditorGUILayout.TextField(variableName);
    }
    #endif
}