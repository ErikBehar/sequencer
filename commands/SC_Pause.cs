using UnityEngine;
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
    public float time = 1f;

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
            player.StartCoroutine(waitABit());
        }
    }

    IEnumerator waitABit()
    {
        yield return new WaitForSeconds(time);
        myPlayer.callBackFromCommand();   
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
        GUILayout.Label("Time to pause:");
        time = EditorGUILayout.FloatField(time);
    }
#endif
}