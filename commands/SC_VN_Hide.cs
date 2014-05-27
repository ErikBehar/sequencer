using UnityEngine;

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
    public int lastSelectedWho = 0;
    public int lastSelectedTo = -1;
    public float time = 0;
    public bool waitForEndOfTween = false;
    private Vector3 previousPosition;

    Tweener tween;

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
            Transform target = sequencerData.targets [lastSelectedWho].target.transform;
            previousPosition = target.transform.localPosition;

            if (useTo)
            {
                Transform to = sequencerData.targets [lastSelectedTo].target.transform;
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
        
        sequencerData.targets [lastSelectedWho].target.SetActive(true);
        Transform target = sequencerData.targets [lastSelectedWho].target.transform;

        if (useTo)
        {
            Transform from = null;
            from = sequencerData.targets [lastSelectedTo].target.transform;
            target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onUndoCompleteEvt));
        } else
        {
            target.localPosition = previousPosition;
            onUndoComplete();
        }
    }

    override public void forward(SequencePlayer player)
    {
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    }

    public void onExecuteCompleteEvt(TweenEvent evt)
    {
        onExecuteComplete();
    }
    
    public void onExecuteComplete()
    {
        if (tween != null && !tween.isComplete)
        {
            tween.Kill();
        }

        sequencerData.targets [lastSelectedWho].target.SetActive(false);  
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
        string[] nicks = sequencerData.getTargetNickNames();
        
        GUILayout.Label("hide who?:");
        lastSelectedWho = EditorGUILayout.Popup(lastSelectedWho, nicks, GUILayout.Width(100));
        
        useTo = GUILayout.Toggle(useTo, "use to?");

        if (useTo)
        {
            GUILayout.Label("going to:"); 
            lastSelectedTo = EditorGUILayout.Popup(lastSelectedTo, nicks, GUILayout.Width(100));
        } else
        {
            lastSelectedTo = -1;
        }

        GUILayout.Label("transition Time:");
        time = EditorGUILayout.FloatField(time);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);
    }
    #endif
}