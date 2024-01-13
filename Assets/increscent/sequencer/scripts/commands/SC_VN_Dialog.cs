using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 2D : Visual Novel: Dialog : Sequencer Command
/// Shows text and a heading for x time ( 0 == infinite)
/// </summary>

[Serializable]
public class SC_VN_Dialog : SequencerCommandBase
{
    public override string commandId{ get { return "dialog"; } }
    public override string commandType{ get { return "base"; } }

    public string speakerTargetName;
    public string text;
    public float time = 0f;
    public bool selectSpeakerPosition = false;
    public string speakerPosName;

    public string audioClipName;
    public AudioClip audioClip;
    public float volume = 1.0f;

    public float bubbleXOffset = 0;

    private Coroutine coroutineWaitTime = null;

    [ReadOnly]
    public uint uid = 0; //unique id, used to find this specific text

    override public void initChild()
    {
        if (uid == 0)
            uid = sequencerData.NewUID();
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Dialog newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Dialog)) as SC_VN_Dialog;
        newCmd.speakerTargetName = speakerTargetName;
        newCmd.selectSpeakerPosition = selectSpeakerPosition;
        newCmd.speakerPosName = speakerPosName;
        newCmd.text = text;
        newCmd.time = time;
        newCmd.audioClipName = audioClipName;
        newCmd.audioClip = audioClip;
        newCmd.volume = volume;
        newCmd.bubbleXOffset = bubbleXOffset;
        newCmd.uid = uid;
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

        GameObject target = null;
        if ( selectSpeakerPosition )
            target = sequencerData.targets [sequencerData.getIndexOfTarget(speakerPosName)].target;
        else{
            //by default we attempt to get the speech bubble target which is child of the outfit
            SequencerTargetModel model = sequencerData.getTargetModel(speakerTargetName);
            if (model != null)
            {
                if (model.target!= null)
                {
                    VN_CharBase charcomp = model.target.GetComponent<VN_CharBase>();
                    
                    if (charcomp != null)
                    {
                        GameObject attireGO = charcomp.getCurrentAttireGO();
                        if ( attireGO != null && attireGO.transform.childCount > 0){
                            target = attireGO.transform.GetChild(0).gameObject;
                            Debug.Log( "target is: " + target.name);
                        }
                    }
                }
            }

            //if we dont find it then use the characters positional target
            if ( target == null){
                Debug.Log( " did not find bubble child");
                target = sequencerData.targets [sequencerData.getIndexOfTarget(speakerTargetName)].target;
            }
        }
        
        myPlayer.dialogController.showDialog(parseTextForVarsAndBB(text), target, bubbleXOffset);
    
        myPlayer.inRewindMode = false;

        if ( time == 0)
        {
            myPlayer.callBackFromCommand(true);
        }
        else
        {
            coroutineWaitTime = myPlayer.StartCoroutine(waitTimeThenCallback());
        }
    }

    IEnumerator waitTimeThenCallback()
    {
        yield return new WaitForSeconds(time);
        myPlayer.callBackFromCommand(false);
    }

    override public void undo()
    {
        if ( coroutineWaitTime != null ){
            myPlayer.StopCoroutine(coroutineWaitTime);
            coroutineWaitTime = null;
        }

        if (audioClip != null)
            SoundManager.Get().stopPlayingSoundList(new List<string>(){audioClip.name});

        myPlayer.dialogController.hideDialog();
    }

    void notifyForward()
    {
        if (audioClip != null)
            SoundManager.Get().stopPlayingSoundList(new List<string>(){audioClip.name});
        
        myPlayer.dialogController.onForward();
    }

    override public void forward(SequencePlayer player)
    {    
        myPlayer = player;

        if ( coroutineWaitTime != null ){
            myPlayer.StopCoroutine(coroutineWaitTime);
            coroutineWaitTime = null;
        }

        if (myPlayer.dialogController.dialogIsShown())
        {
            myPlayer.blockForward = false;
            notifyForward();
        }
        else
        {
            myPlayer.blockForward = true;
            myPlayer.dialogController.dialogForceComplete();
        }
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    } 
   
    #if UNITY_EDITOR 

    override public void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("dialog"), GUILayout.Width(32));
    }

    override public void drawCustomUi()
    { 
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);

        GUILayout.Label("Speech Target:");
        if (nickChars != null && nickChars.Length > 0)
            speakerTargetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, speakerTargetName), nickChars, GUILayout.Width(100))];
    
        GUILayout.Label("Text:"); 
        text = EditorGUILayout.TextArea(text, GUILayout.Width(300));
                
        GUILayout.Label("Time:"); 
        time = EditorGUILayout.FloatField(time);
    
        GUILayout.Label("Voice Over audio clip:");
        audioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), true) as AudioClip;  
            
        GUILayout.Label("Volume 0 - 1.0:"); 
        volume = EditorGUILayout.FloatField(volume);
        
        GUILayout.Label("Speech bubble X offset:"); 
        bubbleXOffset = EditorGUILayout.FloatField(bubbleXOffset);

        selectSpeakerPosition = GUILayout.Toggle(selectSpeakerPosition, "Select Speaker Position?");

        if (selectSpeakerPosition)
        {
            string[] nickPos = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.positional);
            GUILayout.Label("Dialog Position Target:");
            if ( nickPos != null && nickPos.Length > 0)
                speakerPosName = nickPos [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, speakerPosName), nickPos, GUILayout.Width(100))];
        }
    }
    #endif

    private string parseTextForVarsAndBB(string text)
    {
        text = SequencerVariableModel.ParseTextForVars(text, myPlayer.runningTimeVariablesDictionary);

        if (myPlayer.usingNGUI)
        {
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
        } else //suppose we are using uGUI instead
        {
            //only supports bold, italic, size, color according to manual
            //http://docs.unity3d.com/Manual/StyledText.html

            text = text.Replace("{b}", "<b>");
            text = text.Replace("{/b}", "</b>");
            text = text.Replace("{i}", "<i>");
            text = text.Replace("{/i}", "</i>");
            //TODO size & color
        }

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
            + volume.ToString() + "╫" + bubbleXOffset.ToString() + "╫"  
            + selectSpeakerPosition.ToString() + "╫" + speakerPosName + "╫" 
            + uid.ToString() + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        speakerTargetName = splitString [1];
        text = splitString [2];
        time = float.Parse(splitString [3]);
        audioClipName = splitString [4];
        volume = float.Parse(splitString [5]);
        bubbleXOffset = float.Parse(splitString [6]);
        selectSpeakerPosition = bool.Parse(splitString[7]);
        speakerPosName = splitString[8];
        uid = uint.Parse(splitString[9]);
    }

    override public bool updateTargetReference(string oldNickname, string newNickName)
    {
        if (speakerTargetName == oldNickname )
        {
            speakerTargetName = newNickName;
            return true;
        }
        if (speakerPosName == oldNickname)
        {
            speakerPosName = newNickName;
            return true;
        }
        return false;
    }

    override public bool updateVariableReference(string oldVariable, string newVariable)
    {
        if (text.Contains(("[" + oldVariable + "]")))
        {
            text = text.Replace("[" + oldVariable + "]", "[" + newVariable + "]");
            return true;
        }
        return false;
    }
}