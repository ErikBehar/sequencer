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

    public string audioClipName = SoundManagerEB.nullSoundName;
    public AudioClip audioClip;
    public float volume = 1.0f;
    public bool waitForAudioClipEnd = false;

    private bool isWaiting = false;

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
            if (audioClipName != SoundManagerEB.nullSoundName && audioClipName.Length != 0 && audioClipName != "" && audioClipName != " ")
            {
                audioClip = SoundManagerEB.Get().getSfxByName(audioClipName);
            } else if (SoundManagerEB.Get().getSfxByName(audioClip.name) == null)
            {
                SoundManagerEB.Get().sfxClips.Add(audioClip); 
                audioClipName = audioClip.name;
            }
            
            SoundManagerEB.Get().playSfx(audioClipName, volume);
        }

        if (!waitForAudioClipEnd || player.inRewindMode)
            myPlayer.callBackFromCommand();
        else
            myPlayer.StartCoroutine(doAudioWaitFinish());
    }
    
    override public void undo()
    {
        isWaiting = false;
        if (audioClipName != SoundManagerEB.nullSoundName)
            SoundManagerEB.Get().stopPlayingSoundList(new List<string>(){audioClipName});
        else
            SoundManagerEB.Get().stopPlayingSoundList(new List<AudioClip>(){audioClip});
    }

    private IEnumerator doAudioWaitFinish()
    {
        isWaiting = true;
        yield return new WaitForSeconds(audioClip.length);

        if (isWaiting)
        {
            isWaiting = false;
            myPlayer.callBackFromCommand();
        }
    }
    
    override public void forward(SequencePlayer player)
    {
        isWaiting = false;
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

        GUILayout.Label("Wait For AudioClip to end before continue? ");
        waitForAudioClipEnd = EditorGUILayout.Toggle(waitForAudioClipEnd);
    }
    #endif

    override public string toRenpy()
    {
        //target output: play audio "blah.ogg" 1.0
        return "play audio \"" + audioClipName + "\"" + volume.ToString() + "\n";
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