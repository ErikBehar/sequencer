using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System;

/// <summary>
/// 2D : Visual Novel: Dialog : Sequencer Command
/// Shows text and a heading for x time ( 0 == infinite)
/// </summary>
using System.Collections.Generic;

[Serializable]
public class SC_VN_Dialog : SequencerCommandBase
{
    public override string commandId{ get { return "dialog"; } }
    public override string commandType{ get { return "base"; } }

    public int speakerTargetIndex;
    public string text;
    public float time = 0f;
    public AudioClip audioClip;
    public float volume = 1.0f;

    override public void initChild()
    {
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        
        if (audioClip != null)
        {
            if (SoundManager.Get().getSfxByName(audioClip.name) == null)
                SoundManager.Get().sfxClips.Add(audioClip); 
            SoundManager.Get().playSfx(audioClip.name, volume);
        }

        GameObject target = sequencerData.targets [speakerTargetIndex].target;
        myPlayer.dialogController.showDialog(text, target);
    
        myPlayer.inRewindMode = false;
        myPlayer.callBackFromCommand(true); 
    }
    
    override public void undo()
    {
        if (audioClip != null)
            SoundManager.Get().stopPlayingSoundList(new List<string>(){audioClip.name});

        myPlayer.dialogController.hideDialog();
    }

    override public void forward(SequencePlayer player)
    {
        undo();
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    } 
   
    #if UNITY_EDITOR 
    override public void drawCustomUi()
    { 
        string[] nicks = sequencerData.getTargetNickNames();
        GUILayout.Label("Speech Target:");
        speakerTargetIndex = EditorGUILayout.Popup(speakerTargetIndex, nicks, GUILayout.Width(100));
    
        GUILayout.Label("Text:"); 
        text = EditorGUILayout.TextField(text);
                
        GUILayout.Label("Time:"); 
        time = EditorGUILayout.FloatField(time);
    
        GUILayout.Label("Voice Over audio clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  
            
        GUILayout.Label("Volume 0 - 1.0:"); 
        volume = EditorGUILayout.FloatField(volume);
    }
    #endif
}