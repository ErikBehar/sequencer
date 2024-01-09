using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using DG.Tweening;

/// <summary>
/// 3D : Matches character to transform : Sequencer Command
/// </summary>

[Serializable]
public class SC_MatchTransform : SequencerCommandBase
{
    public override string commandId{ get { return "SC_MatchTransform"; } }
    public override string commandType{ get { return "base"; } }

    public string lastSelectedWho = "";
    public Transform transformToMatch;
    public bool usePosition = true;
    public bool useRotation = true;
    public bool useScale = false;
    public float timeToMatch = 0f;
    public bool waitForEndOfTween = false;

    Vector3 prevPosition;
    Quaternion prevRotation;
    Vector3 prevScale;

    Sequence tweenSequence;

    override public SequencerCommandBase clone()
    {       
        SC_MatchTransform newCmd = ScriptableObject.CreateInstance(typeof(SC_MatchTransform)) as SC_MatchTransform;
        newCmd.transformToMatch = transformToMatch;
        newCmd.usePosition = usePosition;
        newCmd.useRotation = useRotation;
        newCmd.useScale = useScale;
        newCmd.timeToMatch = timeToMatch;

        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;

        if (player.inRewindMode)
        {
            undo();
        } 
        else
        {
            Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;

            prevPosition = target.position ;
            prevRotation = target.rotation;
            prevScale = target.localScale;
           
            if ( timeToMatch == 0)
            {     
                if (usePosition)
                    target.position = transformToMatch.position;

                if (useRotation)
                    target.rotation = transformToMatch.rotation;

                if (useScale)
                    target.localScale = transformToMatch.localScale;
                
                if ( waitForEndOfTween)
                    myPlayer.callBackFromCommand();
            }
            else
            {
                tweenSequence = DOTween.Sequence();

                if( usePosition)
                    tweenSequence.Join( DOTween.To(() => target.position, x => target.position = x, transformToMatch.position, timeToMatch) );

                if (useRotation)
                    tweenSequence.Join( DOTween.To(() => target.rotation, x => target.rotation = x, target.rotation.eulerAngles, timeToMatch));
                
                if ( useScale)
                    tweenSequence.Join( DOTween.To(() => target.localScale, x => target.localScale = x, transformToMatch.localScale, timeToMatch));

                tweenSequence.OnComplete(onTweenComplete);
            }
        }
        
        if (!waitForEndOfTween)
            myPlayer.callBackFromCommand();
    }
    
    override public void undo()
    {
        Transform target = sequencerData.targets [sequencerData.getIndexOfTarget(lastSelectedWho)].target.transform;

        if ( timeToMatch == 0)
        {     
            if (usePosition)
               target.position = prevPosition;

            if (useRotation)
                target.rotation = prevRotation;

            if (useScale)
                target.localScale = prevScale;

            if ( waitForEndOfTween)
                myPlayer.callBackFromCommand();
        }
        else
        {
            tweenSequence = DOTween.Sequence();

            if( usePosition)
                tweenSequence.Join( DOTween.To(() => target.position, x => target.position = x, prevPosition, timeToMatch) );

            if (useRotation)
                tweenSequence.Join( DOTween.To(() => target.rotation, x => target.rotation = x, prevRotation.eulerAngles, timeToMatch));

            if ( useScale)
                tweenSequence.Join( DOTween.To(() => target.localScale, x => target.localScale = x, prevScale, timeToMatch));

            tweenSequence.OnComplete(onTweenComplete);
        }

        if (!waitForEndOfTween)
            myPlayer.callBackFromCommand();
    }

    override public void forward(SequencePlayer player)
    {
        if (waitForEndOfTween && tweenSequence != null && !tweenSequence.IsComplete())
        {
            tweenSequence.Kill(true);
        }
    }
    
    override public void backward(SequencePlayer player)
    {
        if (tweenSequence != null && !tweenSequence.IsComplete())
        {
            tweenSequence.Kill(true);
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

        if (waitForEndOfTween)
        {
            myPlayer.callBackFromCommand();
        }
    }
    
    #if UNITY_EDITOR

    override public void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("matchTrans"), GUILayout.Width(32));
    }

    override public void drawCustomUi()
    {
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);
        //string[] nickPos = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.positional);

        GUILayout.Label("match who?:");

        if ( nickChars != null)
            lastSelectedWho = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, lastSelectedWho), nickChars, GUILayout.Width(100))];

        transformToMatch = EditorGUILayout.ObjectField("target transform:", transformToMatch, typeof(Transform), true) as Transform;

        usePosition = GUILayout.Toggle(usePosition, "use position?");
        useRotation = GUILayout.Toggle(useRotation, "use Rotation?");
        useScale = GUILayout.Toggle(useScale, "use Scale?");

        GUILayout.Label("transition Time:");
        timeToMatch = EditorGUILayout.FloatField(timeToMatch);

        GUILayout.Label("Wait for transition to end before continue?:");
        waitForEndOfTween = EditorGUILayout.Toggle(waitForEndOfTween);
    }
    #endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + transformToMatch.gameObject.GetInstanceID() + "╫" + usePosition + "╫"
            + useRotation + "╫" + useScale + "╫" + timeToMatch + "╫" + waitForEndOfTween + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        #if UNITY_EDITOR
            transformToMatch = (EditorUtility.InstanceIDToObject( int.Parse( splitString[1] )) as GameObject).transform;
        #endif

        usePosition = bool.Parse(splitString[2]);
        useRotation = bool.Parse(splitString[3]);
        useScale = bool.Parse(splitString[4]);
        timeToMatch = float.Parse(splitString[5]);
        waitForEndOfTween = bool.Parse(splitString[6]);
    }

    override public bool updateTargetReference(string oldNickname, string newNickName)
    {
        bool didChange = false;
        
        if (lastSelectedWho == oldNickname)
        {
            lastSelectedWho = newNickName;
            didChange = true;
        }
       
        if (didChange)
            return true;
        
        return false;
    }
}