using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

/// <summary>
/// 3D : Sends a named Mecanim trigger to selected Character
/// Doesn't fire or undo when playing backwards 
/// </summary>

[Serializable]
public class SC_Mecanim_Trigger : SequencerCommandBase 
{
    public override string commandId{ get { return "mecanimTrigger"; } }
    public override string commandType{ get { return "base"; } }

    public string targetName = "";
    public string triggerName = "";

    public bool waitForPlayEndCallback = false;

    override public SequencerCommandBase clone()
    {       
        SC_Mecanim_Trigger newCmd = ScriptableObject.CreateInstance(typeof(SC_Mecanim_Trigger)) as SC_Mecanim_Trigger;
        newCmd.targetName = targetName;
        newCmd.triggerName = triggerName;
        newCmd.waitForPlayEndCallback = waitForPlayEndCallback;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;

        if (player.inRewindMode)
        {
            undo();

            myPlayer.callBackFromCommand(); 
        }
        else
        {
            GameObject target = sequencerData.getTargetModel(targetName).target;
            Animator anim = target.GetComponent<Animator>();

            if ( anim != null)
                anim.SetTrigger(triggerName);   

            if (!waitForPlayEndCallback)
                myPlayer.callBackFromCommand();
            else
                target.GetComponent<AnimationEventManager>().onAnimEnd += onAnimEnd;
        }
    }

    void onAnimEnd( string animName)
    {
        if (animName == triggerName)
        {

            GameObject target = sequencerData.getTargetModel(targetName).target;
            target.GetComponent<AnimationEventManager>().onAnimEnd -= onAnimEnd;

            myPlayer.callBackFromCommand(); 
        }
    }
     
    #if UNITY_EDITOR

    override public void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("triggerAnim"), GUILayout.Width(32));
    }

    override public void drawCustomUi()
    { 
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);

        GUILayout.Label("Mecanim Set Trigger, who?:");
        if (nickChars != null && nickChars.Length > 0)
            targetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, targetName), nickChars, GUILayout.Width(100))];

        SequencerTargetModel model = sequencerData.getTargetModel(targetName);
        if (model != null)
        {
            if (model.target == null)
                return;
                
            GUILayout.Label("Trigger Name:");
            triggerName = GUILayout.TextField(triggerName);

            waitForPlayEndCallback = GUILayout.Toggle(waitForPlayEndCallback, "Wait for End Callback?:");
        }
    }
    #endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + targetName + "╫" + triggerName +  "╫" + waitForPlayEndCallback + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        targetName = splitString[1];
        triggerName = splitString[2];
        waitForPlayEndCallback = bool.Parse(splitString[3]);
    }

    override public bool updateTargetReference(string oldNickname, string newNickName)
    {
        if (targetName == oldNickname)
        {
            targetName = newNickName;
            return true;
        }

        return false;
    }
}