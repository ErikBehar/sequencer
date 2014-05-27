using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// 2D : Visual Novel: Show : Sequencer Command
// Tweens a target in the style of a visual novel
// show who?  (pick a target)
// starting from ? ( target or null)
// goint to ( target or null )
// time to get there ( 0 or x seconds) (zero just makes it appear in the to pos)
/// </summary>
using Holoville.HOTween;

[Serializable]
public class SC_VN_Show : SequencerCommandBase
{
    public override string commandId{ get { return "show"; } }
    public override string commandType{ get { return "base"; } }

    public bool useFrom = false;
    public int lastSelectedWho = 0;
    public int lastSelectedFrom = -1;
    public int lastSelectedTo = 0;
    public float time = 0;
    public bool waitForEndOfTween = false;
    private bool wasActiveAtStart = false;
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
            Transform from = null;
            if (useFrom)
                from = sequencerData.targets [lastSelectedFrom].target.transform;
            else
            {
                from = target.transform;
                previousPosition = from.localPosition;
            }
            Transform to = sequencerData.targets [lastSelectedTo].target.transform;
         
            wasActiveAtStart = target.gameObject.activeInHierarchy;
            target.gameObject.SetActive(true);  

            target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onTweenComplete));
        }
        
        if (!waitForEndOfTween)
            myPlayer.callBackFromCommand();
    }
    
    override public void undo()
    {
        Transform target = sequencerData.targets [lastSelectedWho].target.transform;
        Transform from = sequencerData.targets [lastSelectedTo].target.transform;
        Transform to = null;
        if (useFrom)
            to = sequencerData.targets [lastSelectedFrom].target.transform;

        target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

        if (useFrom)            
            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onUndoComplete));
        else
            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z)).OnComplete(onUndoComplete));
    }

    override public void forward(SequencePlayer player)
    {

    }
    
    override public void backward(SequencePlayer player)
    {
        if (tween != null && !tween.isComplete)
        {
            tween.Kill();
        }

        undo();
    }
    
    public void onTweenComplete(TweenEvent evt)
    {  
        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }

    public void onUndoComplete(TweenEvent evt)
    {
        Transform target = sequencerData.targets [lastSelectedWho].target.transform;
        target.gameObject.SetActive(wasActiveAtStart);
        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }
    
    #if UNITY_EDITOR
    override public void drawCustomUi()
    {
        string[] nicks = sequencerData.getTargetNickNames();
        
        GUILayout.Label("show who?:");
        lastSelectedWho = EditorGUILayout.Popup(lastSelectedWho, nicks, GUILayout.Width(100));
        
        useFrom = GUILayout.Toggle(useFrom, "use from?");

        if (useFrom)
        {
            GUILayout.Label("start from:"); 
            lastSelectedFrom = EditorGUILayout.Popup(lastSelectedFrom, nicks, GUILayout.Width(100));
        } else
        {
            lastSelectedFrom = -1;
        }

        GUILayout.Label("going to:"); 
        lastSelectedTo = EditorGUILayout.Popup(lastSelectedTo, nicks, GUILayout.Width(100));

        GUILayout.Label("transition Time:");
        time = EditorGUILayout.FloatField(time);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);
    }
#endif
}