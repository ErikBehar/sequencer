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

    public int targetSectionIndex = 0;
    public int commandIndex = 0;

    private int previousTargetSectionIndex = -1;
    private string[] commandIndexes;

    override public void initChild()
    {
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        if (player.inRewindMode)
        {
            undo();
        } else
        {
            string[] nicks = sequencerData.getSectionNames();
            myPlayer.jumpToScene(nicks [targetSectionIndex], commandIndex);
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
    override public void drawCustomUi()
    { 
        string[] nicks = sequencerData.getSectionNames();
        
        GUILayout.Label("Jump to Section:");
        targetSectionIndex = EditorGUILayout.Popup(targetSectionIndex, nicks, GUILayout.Width(100));

        if (targetSectionIndex != previousTargetSectionIndex)
        {
            previousTargetSectionIndex = targetSectionIndex;
            int[] numberRange = Enumerable.Range(0, sequencerData.sections [targetSectionIndex].commandList.Count).ToArray();
            commandIndexes = new string[numberRange.Length];
            for (int i = 0; i < numberRange.Length; i++)
            {
                commandIndexes [i] = numberRange [i].ToString(); 
            } 
        }
        GUILayout.Label("Command Index:");
        commandIndex = EditorGUILayout.Popup(commandIndex, commandIndexes, GUILayout.Width(100));
    }
#endif
}