using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Set Variable : Sequencer Command
// Creates or sets an already existing temporary variable  
// to the value given 
/// </summary>

[Serializable]
public class SC_SetVariable : SequencerCommandBase
{
    public override string commandId{ get { return "varSet"; } }

    public override string commandType{ get { return "base"; } }

    public string variableName = "";
    public string variableValue = "";
    public int typeIndex = 0; 
    private string previousValue;
    private int varIndex = 0;

    string[] expressionTypes = new string[]
    {
        "string",
        "int",
        "float",
    };

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_SetVariable newCmd = ScriptableObject.CreateInstance(typeof(SC_SetVariable)) as SC_SetVariable;
        newCmd.variableName = variableName;
        newCmd.variableValue = variableValue;
        newCmd.typeIndex = typeIndex;
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
            string result = "0";
            if (typeIndex == 0)
                result = variableValue;
            
            if (myPlayer.runningTimeVariablesDictionary.ContainsKey(variableName))
            {
                previousValue = myPlayer.runningTimeVariablesDictionary [variableName];
            } else
            {
                myPlayer.runningTimeVariablesDictionary.Add(variableName, result);
                previousValue = result;
            }

            if (typeIndex == 1)
            {
                myPlayer.gameObject.GetComponent("EvalExpression").SendMessage("evalInt", parseTextForVars(variableValue));
                result = myPlayer.lastEvalResultInt.ToString();
            } else if (typeIndex == 2)
            {
                myPlayer.gameObject.GetComponent("EvalExpression").SendMessage("evalFloat", parseTextForVars(variableValue));
                result = myPlayer.lastEvalResultFloat.ToString();
            }

            myPlayer.runningTimeVariablesDictionary [variableName] = result;
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
    }

    override public void forward(SequencePlayer player)
    {
    }
    
    override public void backward(SequencePlayer player)
    {
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
                variableValue = sequencerData.variables [varIndex].value;
            }
        }

        GUILayout.Label("Variable Name:");
        variableName = EditorGUILayout.TextField(variableName);

        GUILayout.Label("Value/Expression:");
        variableValue = EditorGUILayout.TextField(variableValue);  

        typeIndex = EditorGUILayout.Popup(typeIndex, expressionTypes);

        if (GUILayout.Button("Add to list if doesnt exist"))
        {
            int indexOfVar = sequencerData.getIndexOfVariable(variableName);
            if (indexOfVar == -1)
            {     
                string result = variableValue;
                if (typeIndex == 1)
                {
                    myPlayer.gameObject.GetComponent("EvalExpression").SendMessage("evalInt", variableValue);
                    result = myPlayer.lastEvalResultInt.ToString();
                } else if (typeIndex == 2)
                {
                    myPlayer.gameObject.GetComponent("EvalExpression").SendMessage("evalFloat", variableValue);
                    result = myPlayer.lastEvalResultFloat.ToString();
                }

                sequencerData.variables.Add(new SequencerVariableModel(variableName, result)); 
            }
        }      
    }
    #endif

    private string parseTextForVars(string text)
    {
        int count = 0;
        int max = 30; //max 30 variable replacements, so we dont crash in weird cases

        //variables
        while (text.IndexOf( "[" ) > -1 && count < max)
        {
            count ++;

            int indexOpen = text.IndexOf("[");
            if (indexOpen > -1)
            {
                int indexClose = text.IndexOf("]");
                string substring = text.Substring(indexOpen + 1, indexClose - (indexOpen + 1));
                if (myPlayer.runningTimeVariablesDictionary.ContainsKey(substring))
                {
                    text = text.Replace("[" + substring + "]", myPlayer.runningTimeVariablesDictionary [substring]);
                } else
                {
                    text = text.Substring(0, indexOpen) + "{" + substring + "}" + text.Substring(indexClose, text.Length - (indexClose + 1));
                }
            }
        }

        if (count > 30)
            Debug.LogError("Stopped Forever Loop or 30 + variable parse in SC_SetVariable.cs, make sure to fix this !");
    
        return text;
    }

    override public string toRenpy()
    {
        //target output: $ bad_points += 1
        return "$ " + variableName + " " + variableValue + "\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + variableName + "╫" + variableValue + "╫" + typeIndex + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        variableName = splitString [1];
        variableValue = splitString [2];
        typeIndex = int.Parse(splitString [3]);
    }

    override public bool updateVariableReference(string oldVariable, string newVariable)
    {
        if (variableName == oldVariable)
        {
            variableName = newVariable;

            if (variableValue.Contains("[" + oldVariable + "]"))
            {
                variableValue.Replace("[" + oldVariable + "]", "[" + newVariable + "]");
            }

            return true;
        }
        return false;
    }
}