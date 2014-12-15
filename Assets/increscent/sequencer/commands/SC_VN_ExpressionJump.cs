using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 2D : Visual Novel: Expression Jump : Sequencer Command
/// Jumps based on evaluation of a boolean expression
/// first expression to evaluate to true is the winnes (order matters)
/// </summary>
using System.Linq;

[Serializable]
public class SC_VN_ExpressionJump : SequencerCommandBase
{
    public override string commandId{ get { return "expressionJump"; } }

    public override string commandType{ get { return "base"; } }

    public int size = 0;
    public List<string> expressionList;
    public List<string> sectionNameList;
    public List<int> commandIndexList;
    [HideInInspector]
    public List<SequencerCommandBase>
        targetCommandList;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_ExpressionJump newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_ExpressionJump)) as SC_VN_ExpressionJump;
        newCmd.size = size;
        newCmd.sectionNameList = new List<string>(sectionNameList.ToArray());
        newCmd.expressionList = new List<string>(expressionList.ToArray());
        newCmd.commandIndexList = new List<int>(commandIndexList.ToArray());
        newCmd.targetCommandList = new List<SequencerCommandBase>(targetCommandList.ToArray());
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        if (player.inRewindMode)
            undo();
        else
        {
            //evaluate expressions and jump to winning one
            if (expressionList.Count == 0)
            {
                Debug.LogWarning("Expression Jump with zero expressions, will continue to next command!");
                myPlayer.callBackFromCommand();
            } else
            {
                int foundIndex = -1;
                for (int i = 0; i < expressionList.Count; i++)
                {
                    myPlayer.gameObject.GetComponent("SequencerEvalExpression").SendMessage("evalBool", parseTextForVars(expressionList [i]));
                    if (myPlayer.lastEvalResultBool)
                    {
                        foundIndex = i;
                        break;
                    }
                }
                
                if (foundIndex > -1)
                {
                    myPlayer.jumpToScene(sectionNameList [foundIndex], commandIndexList [foundIndex]);
                } else
                {
                    Debug.LogWarning("Expression Jump none of the expressions evaluated to true, will continue to next command!");
                    myPlayer.callBackFromCommand();
                }
            }
        }
    }

    override public void undo()
    {
        myPlayer.callBackFromCommand(); 
    } 
   
    #if UNITY_EDITOR 
    override public void drawCustomUi()
    { 
        EditorGUILayout.LabelField("number of options:");
        size = EditorGUILayout.IntField(size);
            
        if (size > 0)
        {
            makeSureListsAreCorrectSize(); 
           
            string[] nicks = sequencerData.getSectionNames();           

            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < size; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Expression:");
                        expressionList [i] = EditorGUILayout.TextField(expressionList [i]);

                        GUILayout.Label("Jump to Section:");
                        sectionNameList [i] = nicks [EditorGUILayout.Popup(sequencerData.getIndexOfSection(sectionNameList [i]), nicks, GUILayout.Width(100))];

                        int commandIndexMax = sequencerData.sections [sequencerData.getIndexOfSection(sectionNameList [i])].commandList.Count;
                        if (commandIndexMax < 46) //max show for in popup style, otherwise use input field
                        {
                            int[] commands = Enumerable.Repeat(0, commandIndexMax).ToArray();
                            string[] commandStr = new string[commands.Length];
                            for (int c = 0; c < commands.Length; c++)
                            {
                                commandStr [c] = c.ToString();
                            }   
                            commandIndexList [i] = Mathf.Clamp(commandIndexList [i], 0, commands.Length - 1);

                            GUILayout.Label("Jump to command Index:");
                            commandIndexList [i] = EditorGUILayout.Popup(commandIndexList [i], commandStr);
                        } else
                        {
                            commandIndexList [i] = Mathf.Clamp(EditorGUILayout.IntField(commandIndexList [i]), 0, commandIndexMax - 1);
                        }
                           
                        if (sequencerData.getSectionModel(sectionNameList [i]).commandList.Count == 0)
                        {
                            targetCommandList [i] = null;
                        } else
                            targetCommandList [i] = sequencerData.getSectionModel(sectionNameList [i]).commandList [commandIndexList [i]];
                    }
                    EditorGUILayout.EndHorizontal();  
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
    #endif

    void makeSureListsAreCorrectSize()
    {
        if (sectionNameList == null)
            sectionNameList = new List<string>(size); 

        if (expressionList == null)
            expressionList = new List<string>(size);

        if (commandIndexList == null)
            commandIndexList = new List<int>(size);

        if (targetCommandList == null)
            targetCommandList = new List<SequencerCommandBase>(size);

        if (sectionNameList.Count < size)
            sectionNameList.AddRange(Enumerable.Repeat("", size - sectionNameList.Count).ToList());

        if (commandIndexList.Count < size)
            commandIndexList.AddRange(Enumerable.Repeat(0, size - commandIndexList.Count).ToList());

        if (targetCommandList.Count < size)
        {
            int amount = size - targetCommandList.Count;
            for (int i=0; i < amount; i++)
            {
                targetCommandList.Add(null);
            }
        }
        
        if (expressionList.Count < size)
            expressionList.AddRange(Enumerable.Repeat("1 > 2", size - expressionList.Count).ToList());

        commandIndexList = commandIndexList.GetRange(0, size);
        sectionNameList = sectionNameList.GetRange(0, size);
        expressionList = expressionList.GetRange(0, size);
        targetCommandList = targetCommandList.GetRange(0, size);
    }

    override public void indexWasUpdated()
    {
        checkIndexCorrectness();
    }
    
    public void checkIndexCorrectness()
    {
        if (sectionNameList == null)
            return;

        for (int i = 0; i < sectionNameList.Count; i++)
        {
            SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionNameList [i]);
            if (commandIndexList [i] < sectionModel.commandList.Count && sectionModel.commandList [commandIndexList [i]] != targetCommandList [i])
            {
                bool found = false;
                foreach (SequencerCommandBase cmd in sectionModel.commandList)
                {
                    if (cmd == targetCommandList [i])
                    {
                        //Debug.Log(i + " = " + commandIndexList [i]);
                        commandIndexList [i] = sectionModel.commandList.IndexOf(cmd);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogWarning("Couldnt find jump target command after index change ! Will use whatever is at previous index!");
                    targetCommandList [i] = sectionModel.commandList [commandIndexList [i]];
                }
            }
        }
    }

    private string parseTextForVars(string text)
    {
        //variables
        while (text.IndexOf( "[" ) > -1)
        {
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
        
        return text;
    }

    override public string toRenpy()
    {
        //target output: if good_points >= bad_points:
        //                      else:

        string stringedExpression = "if " + expressionList [0] + ":\n";
        for (int i=1; i < expressionList.Count; i++)
            stringedExpression += "\t\telse if: " + expressionList [i] + ":\n";

        return stringedExpression;
    }

    override public string toSequncerSerializedString()
    {
        string choices = "";
        for (int i=0; i < size; i++)
        {
            choices += expressionList [i] + "╫" + sectionNameList [i] + "╫" + commandIndexList [i] + "╫";
        }
        
        return GetType().Name + "╫" + choices + "\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        expressionList = new List<string>();
        sectionNameList = new List<string>();
        commandIndexList = new List<int>();
        
        for (int i=1; i+2 < splitString.Length; i+=3)
        {
            expressionList.Add(splitString [i]);
            sectionNameList.Add(splitString [i + 1]);
            commandIndexList.Add(int.Parse(splitString [i + 2]));
        }
    }

    override public bool updateVariableReference(string oldVariable, string newVariable)
    {
        bool wasChanged = false;

        for (int i=0; i < size; i++)
        {
            if (expressionList [i].Contains(("[" + oldVariable + "]")))
            {
                expressionList [i].Replace("[" + oldVariable + "]", "[" + newVariable + "]");
                wasChanged = true;
            }
        }

        if (wasChanged)
            return true;
      
        return false;
    }

    override public bool updateSectionReference(string oldSection, string newSection)
    {
        bool wasChanged = false;
        
        for (int i = 0; i < sectionNameList.Count; i++)
        {
            if (sectionNameList [i] == oldSection)
            {
                sectionNameList [i] = newSection;
                wasChanged = true;
            }
        }
        
        if (wasChanged)
            return true;
        
        return false;
    }
}