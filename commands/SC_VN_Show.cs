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
    public string lastSelectedWho = "";
    public string lastSelectedFrom = "";
    public string lastSelectedTo = "";
    public float time = 0;
    public bool waitForEndOfTween = false;
    private bool wasActiveAtStart = false;
    private Vector3 previousPosition;

    Tweener tween;

    override public void initChild()
    {
        
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Show newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Show)) as SC_VN_Show;
        newCmd.useFrom = useFrom;
        newCmd.lastSelectedWho = lastSelectedWho;
        newCmd.lastSelectedTo = lastSelectedTo;
        newCmd.lastSelectedFrom = lastSelectedFrom;
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
            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            Transform from = null;
            if (useFrom)
                from = sequencerData.getTargetModel(lastSelectedFrom).target.transform;
            else
            {
                from = target.transform;
                previousPosition = from.localPosition;
            }
            Transform to = sequencerData.getTargetModel(lastSelectedTo).target.transform;
         
            wasActiveAtStart = target.gameObject.activeInHierarchy;
            target.gameObject.SetActive(true);  

            target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

            Vector3 finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

            if (time == 0)
            {        
                target.localPosition = finalPos;
                if (waitForEndOfTween)
                {
                    myPlayer.callBackFromCommand();
                }
            } else
                tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", finalPos).OnComplete(onTweenComplete));
        }
        
        if (!waitForEndOfTween)
            myPlayer.callBackFromCommand();
    }
    
    override public void undo()
    {
        Transform target = sequencerData.getTargetModel(lastSelectedWho).target.transform;
        Transform from = sequencerData.getTargetModel(lastSelectedTo).target.transform;
        Transform to = null;
        if (useFrom)
            to = sequencerData.getTargetModel(lastSelectedFrom).target.transform;

        target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

        Vector3 finalPos;
        if (useFrom)            
            finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
        else
            finalPos = new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);

        if (time == 0)
        {        
            target.localPosition = finalPos;
            if (waitForEndOfTween)
            {
                myPlayer.callBackFromCommand();
            }
        } else
            tween = HOTween.To(target, time, new TweenParms().NewProp("localPosition", finalPos).OnComplete(onUndoComplete));
    }

    override public void forward(SequencePlayer player)
    {
        if (waitForEndOfTween && tween != null && !tween.isComplete)
        {
            tween.Kill();
            
            Transform target = sequencerData.getTargetModel(lastSelectedWho).target.transform;
            Transform to = sequencerData.getTargetModel(lastSelectedTo).target.transform;
            target.localPosition = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);            
        }
    }
    
    override public void backward(SequencePlayer player)
    {
        if (tween != null && !tween.isComplete)
        {
            tween.Kill();

            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            Vector3 finalPos;
            if (useFrom)
            {
                Transform to = sequencerData.getTargetModel(lastSelectedFrom).target.transform;          
                finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            } else
                finalPos = new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            
            target.localPosition = finalPos;    
        }
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
        Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
        target.gameObject.SetActive(wasActiveAtStart);
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

        GUILayout.Label("show who?:");
        
        lastSelectedWho = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, lastSelectedWho), nickChars, GUILayout.Width(100))];
        
        useFrom = GUILayout.Toggle(useFrom, "use from?");

        if (useFrom)
        {
            GUILayout.Label("start from:"); 
            lastSelectedFrom = nickPos [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, lastSelectedFrom), nickPos, GUILayout.Width(100))];
        } else
        {
            lastSelectedFrom = "";
        }

        GUILayout.Label("going to:"); 
        lastSelectedTo = nickPos [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, lastSelectedTo), nickPos, GUILayout.Width(100))];

        GUILayout.Label("transition Time:");
        time = EditorGUILayout.FloatField(time);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);
    }
    #endif

    override public string toRenpy()
    {
        //target output: show ami normal at center with dissolve
        return "show " + lastSelectedWho + " normal at " + lastSelectedTo + " with dissolve\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + useFrom.ToString() + "╫"
            + lastSelectedWho + "╫" + lastSelectedFrom + "╫" + lastSelectedTo + "╫"
            + time.ToString() + "╫" + waitForEndOfTween.ToString() + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        useFrom = bool.Parse(splitString [1]);
        lastSelectedWho = splitString [2];
        lastSelectedFrom = splitString [3];
        lastSelectedTo = splitString [4];
        time = float.Parse(splitString [5]);
        waitForEndOfTween = bool.Parse(splitString [6]);
    }
}