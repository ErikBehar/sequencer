using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// WaitForInput : Sequencer Command
// Wait for player to input before continue
// does stop in rewind
/// </summary>

[Serializable]
public class SC_Stub : SequencerCommandBase
{
    public override string commandId{ get { return "SC_Stub"; } }

    public override string commandType{ get { return "base"; } }

    public string stubName = "STUB";	

    override public SequencerCommandBase clone()
    {       
        SC_Stub newCmd = ScriptableObject.CreateInstance(typeof(SC_Stub)) as SC_Stub;
        newCmd.stubName = stubName;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        myPlayer.callBackFromCommand(false); 
    }

#if UNITY_EDITOR

    override public void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("stub"), GUILayout.Width(32));
    }

    override public void drawCustomUi()
    {
        stubName = GUILayout.TextField( stubName );
    }
#endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + stubName +  "╫\n";
    }
}