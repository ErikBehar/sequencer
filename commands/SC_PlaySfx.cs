using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Play SFX Sequencer Command
// Plays a sound Effect 
// Audio Clip
// Volume
/// </summary>

[Serializable]
public class SC_PlaySfx : SequencerCommandBase
{
    public override string commandId{ get { return "playSfx"; } }

    public override string commandType{ get { return "base"; } }

    public string audioClipName = SoundManager.nullSoundName;
    public AudioClip audioClip;
    public float volume = 1.0f;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_PlaySfx newCmd = ScriptableObject.CreateInstance(typeof(SC_PlaySfx)) as SC_PlaySfx;
        newCmd.audioClipName = audioClipName;
        newCmd.audioClip = audioClip;
        newCmd.volume = volume;
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
            if (audioClipName != SoundManager.nullSoundName)
            {
                audioClip = SoundManager.Get().getSfxByName(audioClipName);
            } else if (SoundManager.Get().getSfxByName(audioClip.name) == null)
            {
                SoundManager.Get().sfxClips.Add(audioClip); 
                audioClipName = audioClip.name;
            }
            
            SoundManager.Get().playSfx(audioClipName, volume);
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        if (audioClipName != SoundManager.nullSoundName)
            SoundManager.Get().stopPlayingSoundList(new List<string>(){audioClipName});
        else
            SoundManager.Get().stopPlayingSoundList(new List<AudioClip>(){audioClip});
    }
    
    override public void forward(SequencePlayer player)
    {
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    }

    #if UNITY_EDITOR
    override public void drawCustomUi()
    { 
        GUILayout.Label("Audio Clip Name (optional):");
        audioClipName = EditorGUILayout.TextField(audioClipName); 

        GUILayout.Label("Audio Clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  

        GUILayout.Label("Volume 0-1.0:"); 
        volume = EditorGUILayout.FloatField(volume);

    }
    #endif
}