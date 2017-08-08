using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// Stop Music Sequencer Command
// fade time 0 = no fade
/// </summary>

[Serializable]
public class SC_StopMusic : SequencerCommandBase
{
    public override string commandId{ get { return "stopMusic"; } }

    public override string commandType{ get { return "base"; } }

    public float fadeTime = 1f;
    private string previousPlayingMusicClipName;
    private float previousFadeTime;
    private float previousVolume;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_StopMusic newCmd = ScriptableObject.CreateInstance(typeof(SC_StopMusic)) as SC_StopMusic;
        newCmd.fadeTime = fadeTime;
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
            previousPlayingMusicClipName = SoundManagerEB.Get().getCurrentMusicClipName();
            previousFadeTime = SoundManagerEB.Get().fadeTime;
            previousVolume = SoundManagerEB.Get().getCurrentMusicClipVolume();
            SoundManagerEB.Get().fadeTime = fadeTime;
            SoundManagerEB.Get().stopMusic();
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        SoundManagerEB.Get().fadeTime = previousFadeTime;
        SoundManagerEB.Get().musicVolume = previousVolume;
        SoundManagerEB.Get().playMusic(previousPlayingMusicClipName, previousVolume);
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
        GUILayout.Label("Fade Time:"); 
        fadeTime = EditorGUILayout.FloatField(fadeTime);
    }
    #endif

    override public string toRenpy()
    {
        //target output: stop music fadeout 1.0
        return "stop music fadeout 1.0\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + fadeTime + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        fadeTime = float.Parse(splitString [1]);
    }
}