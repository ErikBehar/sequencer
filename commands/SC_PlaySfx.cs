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
    public AudioClip audioClip;
    public float volume = 1.0f;

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
            if (SoundManager.Get().getSfxByName(audioClip.name) == null)
                SoundManager.Get().sfxClips.Add(audioClip); 
            SoundManager.Get().playSfx(audioClip.name, volume);
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
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
        GUILayout.Label("Audio Clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  

        GUILayout.Label("Volume 0-1.0:"); 
        volume = EditorGUILayout.FloatField(volume);

    }

#endif
}