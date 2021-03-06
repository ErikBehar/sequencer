﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Mute all Sfx playing currently Sequencer Command
/// </summary>

[Serializable]
public class SC_MuteAllSfx : SequencerCommandBase
{
    public override string commandId{ get { return "muteSfx"; } }

    public override string commandType{ get { return "base"; } }

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_MuteAllSfx newCmd = ScriptableObject.CreateInstance(typeof(SC_MuteAllSfx)) as SC_MuteAllSfx;
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
            SoundManagerEB.Get().stopAllSfx();
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
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
    }
#endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫\n";
    }
}