using UnityEngine;

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

    public string audioClipName;
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
            previousPlayingMusicClipName = SoundManager.Get().getCurrentMusicClipName();
            previousVolume = SoundManager.Get().musicVolume;

            if (audioClipName != SoundManager.nullSoundName && audioClipName.Length != 0 && audioClipName != "" && audioClipName != " ")
            {
                audioClip = SoundManager.Get().getMusicByName(audioClipName);
            } else if (SoundManager.Get().getMusicByName(audioClip.name) == null)
            {
                SoundManager.Get().musicClips.Add(audioClip);
                audioClipName = audioClip.name;
            }else if ( audioClip != null)
            {
                audioClipName = audioClip.name;
            }

            SoundManager.Get().playMusic(audioClipName, volume);
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
        GUILayout.Label("Audio Clip Name (optional):");
        audioClipName = EditorGUILayout.TextField(audioClipName); 

        GUILayout.Label("Audio Clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  

        GUILayout.Label("Volume 0-1.0:"); 
        volume = EditorGUILayout.FloatField(volume);
    }
#endif

    override public string toRenpy()
    {
        //target output: play music "music/Smooth Ambience.ogg" fadein 1.0
        return "play music \"" + audioClipName + "\" fadein 1.0\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + ((audioClip != null) ? audioClip.name : audioClipName) + "╫" + volume + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        audioClipName = splitString [1];
        volume = float.Parse(splitString [2]);
    }
}