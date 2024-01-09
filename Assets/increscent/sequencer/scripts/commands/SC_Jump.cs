using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; 
#endif

using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Jump : Sequencer Command
// Jump or Goto anothe section
// target section  (pick a section)
// future maybe some transition thing ?
/// </summary>
using System.Linq;

[Serializable]
public class SC_Jump : SequencerCommandBase
{
    public override string commandId{ get { return "jump"; } }
    public override string commandType{ get { return "base"; } }

    public string targetSectionName = "";
    public int commandIndex = 0;
    
    [HideInInspector]
    public SequencerCommandBase
        targetCommand;

    private string previousTargetSectionName = "";
    private string[] commandIndexes;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_Jump newCmd = ScriptableObject.CreateInstance(typeof(SC_Jump)) as SC_Jump;
        newCmd.targetSectionName = targetSectionName;
        newCmd.commandIndex = commandIndex;
        newCmd.commandIndexes = new string[commandIndexes.Length ];
        newCmd.targetCommand = targetCommand;
        Array.Copy(commandIndexes, newCmd.commandIndexes, commandIndexes.Length);
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        if (player.inRewindMode)
        {
            undo();
        } else
        {
            myPlayer.jumpToScene(targetSectionName, commandIndex);
        }
    }
    
    override public void undo()
    {
        myPlayer.callBackFromCommand(); 
    }  
    
    override public void forward(SequencePlayer player)
    {
    }

    override public void backward(SequencePlayer player)
    {
    }

#if UNITY_EDITOR
    override public void drawMinimizedUi()
    {
        GUILayout.Button(sequencerData.getIconTexture("jump"), GUILayout.Width(32));
    }
    override public void drawCustomUi()
    { 
        string[] nicks = sequencerData.getSectionNames();
        
        GUILayout.Label("Jump to Section:");
        targetSectionName = nicks [EditorGUILayout.Popup(sequencerData.getIndexOfSection(targetSectionName), nicks, GUILayout.Width(100))];
        
        int commandIndexMax = sequencerData.sections [sequencerData.getIndexOfSection(targetSectionName)].commandList.Count;
        if (commandIndexMax < 46) //max show for in popup style, otherwise use input field
        {
            if (targetSectionName != previousTargetSectionName)
            {
                previousTargetSectionName = targetSectionName;
                int[] numberRange = Enumerable.Range(0, commandIndexMax).ToArray();
                commandIndexes = new string[numberRange.Length];
                for (int i = 0; i < numberRange.Length; i++)
                {
                    commandIndexes [i] = numberRange [i].ToString(); 
                }
    
                commandIndex = Mathf.Clamp(commandIndex, 0, commandIndexes.Length - 1);
            }
    
            GUILayout.Label("Command Index:");
            commandIndex = EditorGUILayout.Popup(commandIndex, commandIndexes, GUILayout.Width(100));
        } else
        {
            commandIndex = Mathf.Clamp(EditorGUILayout.IntField(commandIndex), 0, commandIndexMax - 1);
        }

        SequencerSectionModel sectionModel = sequencerData.getSectionModel(targetSectionName);
        if (sectionModel.commandList != null && sectionModel.commandList.Count > 0 && sectionModel.commandList.Count > commandIndex)
            targetCommand = sectionModel.commandList [commandIndex];
    }
    #endif

    override public void indexWasUpdated()
    {
        checkIndexCorrectness();
    }

    public void checkIndexCorrectness()
    {
        SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);
        if (commandIndex == -1 || commandIndex == sectionModel.commandList.Count || sectionModel.commandList [commandIndex] != targetCommand)
        {
            bool found = false;
            foreach (SequencerCommandBase cmd in sectionModel.commandList)
            {
                if (cmd == targetCommand)
                {
                    commandIndex = sectionModel.commandList.IndexOf(cmd);
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                Debug.LogWarning("Couldnt find jump target command after index change ! Will use whatever is at previous index! " +
                    "Was looking for index:" + commandIndex + " in section: " + sectionModel.name + " is jump command at index: " + 
                    sequencerData.getSectionModel(sectionName).commandList.IndexOf(this));
                
                if (commandIndex != -1)
                    targetCommand = sectionModel.commandList [commandIndex];
                else if (commandIndex == -1 && sectionModel.commandList.Count == 0)
                    Debug.LogWarning("The section might be empty! Empty Sections cant be jumped to correctly !");
                else if (commandIndex == -1 && sectionModel.commandList.Count > 0)
                {   
                    commandIndex = 0;
                    targetCommand = sectionModel.commandList [commandIndex];
                }
            }
        }
    }

    override public string toRenpy()
    {
        //target output: jump credits
        string sectionNameSans = sectionName;
        if (targetSectionName [targetSectionName.Length - 1] == ':')
            sectionNameSans = targetSectionName.Substring(0, sectionName.Length - 1);
        return "jump " + sectionNameSans + "\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + targetSectionName + "╫" + commandIndex + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        targetSectionName = splitString [1];
        commandIndex = int.Parse(splitString [2]);
    }

    override public bool updateSectionReference(string oldSection, string newSection)
    {
        if (targetSectionName == oldSection)
        {
            targetSectionName = newSection;
            return true;
        }

        return false;
    }
}