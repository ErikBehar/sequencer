﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Pause : Sequencer Command
// Pause playback for x seconds
/// </summary>

[Serializable]
public class SC_Pause : SequencerCommandBase
{
    public override string commandId{ get { return "pause"; } }

    public override string commandType{ get { return "base"; } }

    public float time = 1f;

    private bool wasCancelled = false;

    override public void initChild()
    {
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        wasCancelled = false;
        
        if (player.inRewindMode)
        {
            undo();
        } else
        {
            player.StartCoroutine(waitABit());
        }
    }

    IEnumerator waitABit()
    {
        yield return new WaitForSeconds(time);
        if (!wasCancelled)
        {
            wasCancelled = false;
            myPlayer.callBackFromCommand();
        } else
            wasCancelled = false;   
    }
    
    override public void undo()
    {
        myPlayer.callBackFromCommand(); 
    }

    override public void forward(SequencePlayer player)
    {
        wasCancelled = true;
    }
    
    override public void backward(SequencePlayer player)
    {
        wasCancelled = true;
    }

#if UNITY_EDITOR
    override public void drawCustomUi()
    { 
        GUILayout.Label("Time to pause:");
        time = EditorGUILayout.FloatField(time);
    }
#endif
}