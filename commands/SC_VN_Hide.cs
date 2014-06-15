﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// 2D : Visual Novel: Hide : Sequencer Command
// Tweens/Hides a target in the style of a visual novel  ( TODO: ALPHA FADE thing ? another command ?)
// hide who?  (pick a target)
// goint to ( target or null )
// time to hide there ( 0 or x seconds) (zero just makes it disappear instantly)
/// </summary>
using Holoville.HOTween;

[Serializable]
public class SC_VN_Hide : SequencerCommandBase
{
    public override string commandId{ get { return "hide"; } }
    public override string commandType{ get { return "base"; } }

    public bool useTo = false;
    public string lastSelectedWho = "";
    public string lastSelectedTo = "";
    public float time = 0;
    public bool waitForEndOfTween = false;
    private Vector3 previousPosition;

    Tweener tween;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Hide newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Hide)) as SC_VN_Hide;
        newCmd.useTo = useTo;
        newCmd.lastSelectedWho = lastSelectedWho;
        newCmd.lastSelectedTo = lastSelectedTo;
        newCmd.time = time;
        newCmd.waitForEndOfTween = waitForEndOfTween;
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
            if (tween != null && !tween.isComplete)
            {
                tween.Kill();
            }

            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            previousPosition = target.transform.localPosition;

            if (useTo)
            {
                Transform to = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedTo)].target.transform;
                tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onExecuteCompleteEvt));
            } else
            {
                onExecuteComplete();
            }
        }
        
        if (!waitForEndOfTween)
            myPlayer.callBackFromCommand();
    }
    
    override public void undo()
    {
        if (tween != null && !tween.isComplete)
        {
            tween.Kill();
        }
        
        sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.SetActive(true);
        Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;

        if (useTo)
        {
            Transform from = null;
            from = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedTo)].target.transform;
            target.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onUndoCompleteEvt));
        } else
        {
            target.localPosition = previousPosition;
            onUndoComplete();
        }
    }

    override public void forward(SequencePlayer player)
    {
        if (waitForEndOfTween && tween != null && !tween.isComplete)
        {
            tween.Kill();

            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            
            if (useTo)
            {
                Transform to = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedTo)].target.transform;
                target.localPosition = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            }
            
            target.gameObject.SetActive(false);  
        }
    }
    
    override public void backward(SequencePlayer player)
    {
        if (waitForEndOfTween && tween != null && !tween.isComplete)
        {
            tween.Kill();

            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            target.localPosition = previousPosition;
            target.gameObject.SetActive(true);  
        }
    }

    public void onExecuteCompleteEvt(TweenEvent evt)
    {
        onExecuteComplete();
    }
    
    public void onExecuteComplete()
    {
        sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.SetActive(false);  
        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }

    public void onUndoCompleteEvt(TweenEvent evt)
    {
        onUndoComplete();
    }

    public void onUndoComplete()
    {
        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }
    
    #if UNITY_EDITOR
    override public void drawCustomUi()
    { 
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);
        string[] nickPos = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.positional);
        
        GUILayout.Label("hide who?:");
        lastSelectedWho = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, lastSelectedWho), nickChars, GUILayout.Width(100))];
        
        useTo = GUILayout.Toggle(useTo, "use to?");

        if (useTo)
        {
            GUILayout.Label("going to:"); 
            lastSelectedTo = nickPos [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, lastSelectedTo), nickPos, GUILayout.Width(100))];
        } else
        {
            lastSelectedTo = "";
        }

        GUILayout.Label("transition Time:");
        time = EditorGUILayout.FloatField(time);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);
    }
    #endif
}