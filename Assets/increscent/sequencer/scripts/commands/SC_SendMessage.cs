using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using System;

/// <summary>
/// SendMessage : Sequencer Command
/// Sends out a unity message
/// </summary>

[Serializable]
public class SC_SendMessage : SequencerCommandBase
{
    public override string commandId{ get { return "sendMessage"; } }

    public override string commandType{ get { return "base"; } }

    public string functionToCallName;
    public string stringParameter = "none";
    public GameObject target;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_SendMessage newCmd = ScriptableObject.CreateInstance(typeof(SC_SendMessage)) as SC_SendMessage;
        newCmd.functionToCallName = functionToCallName;
        newCmd.stringParameter = stringParameter;
        newCmd.target = target;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;

        myPlayer.inRewindMode = false;

        if ( target != null)
            target.SendMessageUpwards(functionToCallName, stringParameter);
        else
            myPlayer.SendMessageUpwards(functionToCallName, stringParameter);

        myPlayer.callBackFromCommand(false); 
    }

    override public void undo()
    {  

    }

    override public void forward(SequencePlayer player)
    {
        
    }

    override public void backward(SequencePlayer player)
    {
        undo();
    }

    #if UNITY_EDITOR

        override public void drawMinimizedUi()
        {
            GUILayout.Button( sequencerData.getIconTexture("sendMessage"));
        }

        override public void drawCustomUi()
        { 
            GUILayout.Label("Send Standard Unity Message:");
            functionToCallName = EditorGUILayout.TextField(functionToCallName);
            stringParameter = EditorGUILayout.TextField(stringParameter);
            target = EditorGUILayout.ObjectField("target GameObject:", target, typeof(GameObject), true) as GameObject;
        }
    #endif

    override public string toRenpy()
    {
        //target output: ?
        return "";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + functionToCallName + "╫" + stringParameter + "╫" + target.GetInstanceID() + "╫\n";
    }
}