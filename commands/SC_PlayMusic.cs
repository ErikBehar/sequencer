﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Play Music Sequencer Command
// Plays Music 
// Audio Clip
// Volume
/// </summary>

[Serializable]
public class SC_PlayMusic : SequencerCommandBase
{
    public override string commandId{ get { return "playMusic"; } }
    public override string commandType{ get { return "base"; } }

    public AudioClip audioClip;
    public float volume = 1.0f;
    private string previousPlayingMusicClipName;
    private float previousVolume;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_PlayMusic newCmd = ScriptableObject.CreateInstance(typeof(SC_PlayMusic)) as SC_PlayMusic;
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
            previousPlayingMusicClipName = SoundManager.Get().getCurrentMusicClipName();
            previousVolume = SoundManager.Get().musicVolume;

            if (SoundManager.Get().getMusicByName(audioClip.name) == null)
                SoundManager.Get().musicClips.Add(audioClip);
            SoundManager.Get().playMusic(audioClip.name, volume);
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        if (previousPlayingMusicClipName == SoundManager.nullSoundName)
            SoundManager.Get().fadeOutMusic();
        else
            SoundManager.Get().playMusic(previousPlayingMusicClipName, previousVolume);
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
        GUILayout.Label("Audio Clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  

        GUILayout.Label("Volume 0-1.0:"); 
        volume = EditorGUILayout.FloatField(volume);
    }
#endif
}