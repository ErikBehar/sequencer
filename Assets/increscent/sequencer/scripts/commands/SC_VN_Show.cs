using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;
using DG.Tweening;

/// <summary>
/// 2D : Visual Novel: Show : Sequencer Command
// Tweens a target in the style of a visual novel
// show who?  (pick a target)
// starting from ? ( target or null)
// goint to ( target or null )
// time to get there ( 0 or x seconds) (zero just makes it appear in the to pos)
/// </summary>

[Serializable]
public class SC_VN_Show : SequencerCommandBase
{
    public override string commandId{ get { return "show"; } }
    public override string commandType{ get { return "base"; } }

    public bool useFrom = false;
    public bool useTo = true;
    public string lastSelectedWho = "";
    public string lastSelectedFrom = "";
    public string lastSelectedTo = "";
    public float time = 0;
    public bool waitForEndOfTween = false;
    private bool wasActiveAtStart = false;
    private Vector3 previousPosition;
    public bool useLocal = true;

    Tweener tween;

    override public void initChild()
    {
        
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Show newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Show)) as SC_VN_Show;
        newCmd.useFrom = useFrom;
        newCmd.useTo = useTo;
        newCmd.lastSelectedWho = lastSelectedWho;
        newCmd.lastSelectedTo = lastSelectedTo;
        newCmd.lastSelectedFrom = lastSelectedFrom;
        newCmd.time = time;
        newCmd.waitForEndOfTween = waitForEndOfTween;
        newCmd.useLocal = useLocal;
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
                if ( target != null)
                    from = target.transform;

                if (useLocal)
                    previousPosition = from.localPosition;
                else
                    previousPosition = from.position;
            }


            Transform to = null;
            if (target != null)
                to = target.transform; 
            
            if (useTo)
                to = sequencerData.getTargetModel(lastSelectedTo).target.transform;
            
            wasActiveAtStart = target.gameObject.activeInHierarchy;
            target.gameObject.SetActive(true);  

            if (useLocal)
                target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            else
                target.transform.position = new Vector3(from.position.x, from.position.y, from.position.z);

            Vector3 finalPos;
            if (useLocal)
                finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            else
                finalPos = new Vector3(to.position.x, to.position.y, to.position.z);

            if ( time == 0)
            {        
                target.localPosition = finalPos;
                if (waitForEndOfTween)
                {
                    myPlayer.callBackFromCommand();
                }
            }
            else
            {
                if ( useLocal)
                    tween = DOTween.To(() => target.localPosition, x => target.localPosition = x, finalPos, time).OnComplete( onTweenComplete );
                else
                    tween = DOTween.To(() => target.position, x => target.position = x, finalPos, time).OnComplete( onTweenComplete );
            }
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

        if ( useLocal)
            target.transform.localPosition = new Vector3(from.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
        else
            target.transform.position = new Vector3(from.position.x, from.position.y, from.position.z);

        Vector3 finalPos;
        if (useFrom)
        {
            if ( useLocal)
                finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            else
                finalPos = new Vector3(to.position.x, to.position.y, to.position.z);
        }
        else
        {
            if (useLocal)
                finalPos = new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
            else
                finalPos = new Vector3(previousPosition.x, previousPosition.y, previousPosition.z);
        }

        if (time == 0)
        {       
            if (useLocal)
                target.localPosition = finalPos;
            else
                target.position = finalPos;
            
            target.gameObject.SetActive(wasActiveAtStart);
            if (waitForEndOfTween)
            {
                myPlayer.callBackFromCommand();
            }
        }
        else
        {
            if ( useLocal)
                tween = DOTween.To(() => target.localPosition, x => target.localPosition = x, finalPos, time).OnComplete( onTweenComplete );
            else
                tween = DOTween.To(() => target.position, x => target.position = x, finalPos, time).OnComplete( onTweenComplete );
        }
    }

    override public void forward(SequencePlayer player)
    {
        if (waitForEndOfTween && tween != null && !tween.IsComplete())
        {
            tween.Kill();
            
            Transform target = sequencerData.getTargetModel(lastSelectedWho).target.transform;
            Transform to = sequencerData.getTargetModel(lastSelectedTo).target.transform;

            if( useLocal )
                target.localPosition = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z); 
            else
                target.position = new Vector3(to.position.x, to.position.y, to.position.z);
        }
    }
    
    override public void backward(SequencePlayer player)
    {
        if (tween != null && !tween.IsComplete())
        {
            tween.Kill();

            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;
            Vector3 finalPos;
            if (useFrom)
            {
                Transform to = sequencerData.getTargetModel(lastSelectedFrom).target.transform;  
                if (useLocal)
                    finalPos = new Vector3(to.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
                else
                    finalPos = new Vector3(to.position.x, to.position.y, to.position.z);
            }
            else
            {
                if (useLocal)
                    finalPos = new Vector3(previousPosition.x, target.transform.localPosition.y, target.transform.localPosition.z);
                else
                    finalPos = new Vector3(previousPosition.x, previousPosition.y, previousPosition.z);
            }

            if (useLocal)
                target.localPosition = finalPos;
            else
                target.position = finalPos;
        }
    }
    
    public void onTweenComplete()
    {  
        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }

    public void onUndoComplete()
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

        if ( nickChars != null)
        lastSelectedWho = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, lastSelectedWho), nickChars, GUILayout.Width(100))];
        
        useFrom = GUILayout.Toggle(useFrom, "use from?");

        if (useFrom)
        {
            GUILayout.Label("start from:"); 
            if ( nickPos != null)
                lastSelectedFrom = nickPos [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, lastSelectedFrom), nickPos, GUILayout.Width(100))];
        } else
        {
            lastSelectedFrom = "";
        }

        useTo = GUILayout.Toggle(useTo, "use to?");

        if (useTo)
        {
            GUILayout.Label("going to:"); 
            if ( nickPos != null)
                lastSelectedTo = nickPos[EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickPos, lastSelectedTo), nickPos, GUILayout.Width(100))];
        }
        else
            lastSelectedTo = "";

        GUILayout.Label("transition Time:");
        time = EditorGUILayout.FloatField(time);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);

        useLocal = GUILayout.Toggle(useLocal, "Use Local Space");
    }
    #endif

    override public string toRenpy()
    {
        //target output: show ami normal at center with dissolve
        return "show " + lastSelectedWho + " normal at " + lastSelectedTo + " with dissolve\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + useFrom + "╫" + useTo + "╫"
        + lastSelectedWho + "╫" + lastSelectedFrom + "╫" + lastSelectedTo + "╫"
        + time + "╫" + waitForEndOfTween + "╫" + useLocal + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        useFrom = bool.Parse(splitString [1]);
        useTo = bool.Parse(splitString[2]);
        lastSelectedWho = splitString [3];
        lastSelectedFrom = splitString [4];
        lastSelectedTo = splitString [5];
        time = float.Parse(splitString [6]);
        waitForEndOfTween = bool.Parse(splitString [7]);
        useLocal = bool.Parse(splitString[8]);
    }

    override public bool updateTargetReference(string oldNickname, string newNickName)
    {
        bool didChange = false;
        
        if (lastSelectedWho == oldNickname)
        {
            lastSelectedWho = newNickName;
            didChange = true;
        }

        if (lastSelectedFrom == oldNickname)
        {
            lastSelectedFrom = newNickName;
            didChange = true;
        }
        
        if (lastSelectedTo == oldNickname)
        {
            lastSelectedTo = newNickName;
            didChange = true;
        }
        
        if (didChange)
            return true;
        
        return false;
    }
}