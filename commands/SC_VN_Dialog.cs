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

    public string speakerTargetName;
    public string text;
    public float time = 0f;

    public string audioClipName;
    public AudioClip audioClip;
    public float volume = 1.0f;

    public float bubbleXOffset = 0;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Dialog newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Dialog)) as SC_VN_Dialog;
        newCmd.speakerTargetName = speakerTargetName;
        newCmd.text = text;
        newCmd.time = time;
        newCmd.audioClipName = audioClipName;
        newCmd.audioClip = audioClip;
        newCmd.volume = volume;
        newCmd.bubbleXOffset = bubbleXOffset;
        return base.clone(newCmd);        
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

        GameObject target = sequencerData.targets [sequencerData.getIndexOfTarget(speakerTargetName)].target;
        myPlayer.dialogController.showDialog(parseTextForVarsAndBB(text), target, bubbleXOffset);
    
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
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);


        GUILayout.Label("Speech Target:");
        if (nickChars != null && nickChars.Length > 0)
            speakerTargetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, speakerTargetName), nickChars, GUILayout.Width(100))];
    
        GUILayout.Label("Text:"); 
        text = EditorGUILayout.TextField(text, GUILayout.Width(300));
                
        GUILayout.Label("Time:"); 
        time = EditorGUILayout.FloatField(time);
    
        GUILayout.Label("Voice Over audio clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  
            
        GUILayout.Label("Volume 0 - 1.0:"); 
        volume = EditorGUILayout.FloatField(volume);
        
        GUILayout.Label("Speech bubble X offset:"); 
        bubbleXOffset = EditorGUILayout.FloatField(bubbleXOffset);
    }
    #endif

    private string parseTextForVarsAndBB(string text)
    {
        //variables
        while (text.IndexOf( "[" ) > -1)
        {
            int indexOpen = text.IndexOf("[");
            if (indexOpen > -1)
            {
                int indexClose = text.IndexOf("]");
                string substring = text.Substring(indexOpen + 1, indexClose - (indexOpen + 1));
                if (myPlayer.runningTimeVariablesDictionary.ContainsKey(substring))
                {
                    text = text.Replace("[" + substring + "]", myPlayer.runningTimeVariablesDictionary [substring]);
                } else
                {
                    text = text.Substring(0, indexOpen) + "{" + substring + "}" + text.Substring(indexClose, text.Length - indexClose);
                }
            }
        }

        //known BB codes
        text = text.Replace("{b}", "[b]");
        text = text.Replace("{/b}", "[/b]");
        text = text.Replace("{i}", "[i]");
        text = text.Replace("{/i}", "[/i]");
        text = text.Replace("{u}", "[u]");
        text = text.Replace("{/u}", "[/u]");
        text = text.Replace("{s}", "[s]");
        text = text.Replace("{/s}", "[/s]");
        text = text.Replace("{sup}", "[sup]");
        text = text.Replace("{/sup}", "[/sup]");
        text = text.Replace("{sub}", "[sub]");
        text = text.Replace("{/sub}", "[/sub]");
               
        //fix issue with ngui typewriter failing if bb is last thing in string ?
        if (text [text.Length - 1] == ']')
            text += " ";

        return text;
    }

    override public string toRenpy()
    {
        //TODO: missing voice over sfx here
        //target output: character "some text"
        return speakerTargetName + " \"" + text + "\"\n";
    }

    override public string toSequncerSerializedString()
    {    
        return GetType().Name + "╫" + speakerTargetName + "╫"
            + text + "╫" + time.ToString() + "╫" + ((audioClip != null) ? audioClip.name : audioClipName) + "╫" 
            + volume.ToString() + "╫" + bubbleXOffset.ToString() + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        speakerTargetName = splitString [1];
        text = splitString [2];
        time = float.Parse(splitString [3]);
        audioClipName = splitString [4];
        volume = float.Parse(splitString [5]);
        bubbleXOffset = float.Parse(splitString [6]);
    }
}