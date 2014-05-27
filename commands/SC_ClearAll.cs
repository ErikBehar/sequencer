﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Sometimes when changing scenes you want a quick way to clear everything on the screen
/// on the other hand when you rewind you want to go back to "same" state as previously
/// this command attempts to do that ( probably not going to be perfect )
/// </summary>

[Serializable]
public class SC_ClearAll : SequencerCommandBase
{
    public override string commandId{ get { return "clearAll"; } }

    public override string commandType{ get { return "base"; } }

    private string musicClipName = "";
    private float musicClipVolume = 1;
    private List<GameObject> targets;
    private List<Vector3> positions;
    private List<bool> visibilitys;
    private List<int> attires;
    private List<string> expressions;

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
            //clear previous
            positions = new List<Vector3>();
            visibilitys = new List<bool>();
            attires = new List<int>();
            expressions = new List<string>();

            //attempt save state:
            
            //music 
            musicClipName = SoundManager.Get().getCurrentMusicClipName();
            musicClipVolume = SoundManager.Get().getCurrentMusicClipVolume(); 

            SoundManager.Get().stopMusic();
            
            //characters (visible, pos, attire, expression)
            foreach (SequencerTargetModel model in myPlayer.sequencerData.targets)
            {
                positions.Add(model.target.transform.localPosition);
                visibilitys.Add(model.target.activeInHierarchy);
                VN_CharBase hasCharComp = model.target.GetComponent<VN_CharBase>(); 

                if (hasCharComp != null)
                {
                    attires.Add(hasCharComp.getCurrentAttire());
                    expressions.Add(hasCharComp.getCurrentExpressionName()); 
                } else
                {
                    attires.Add(-1);
                    expressions.Add("");
                }

                //TODO: in future use type instead? backgrounds are characters too ?
                if (hasCharComp != null)
                    model.target.SetActive(false);
            }

            //future: vars
        }
        
        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        //music 
        if (musicClipName != SoundManager.nullSoundName)
            SoundManager.Get().playMusic(musicClipName, musicClipVolume);
        
        //characters (visible, pos, attire, expression)
        for (int i = 0; i < myPlayer.sequencerData.targets.Count; i++)
        {
            SequencerTargetModel model = myPlayer.sequencerData.targets [i];
            
            VN_CharBase hasCharComp = model.target.GetComponent<VN_CharBase>(); 
            model.target.transform.localPosition = positions [i];
           
            if (hasCharComp != null)
            {
                model.target.SetActive(visibilitys [i]);
                hasCharComp.setAttire(attires [i]);
                hasCharComp.setExpression(expressions [i]);
            }
        } 
        
        //future: vars
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
}