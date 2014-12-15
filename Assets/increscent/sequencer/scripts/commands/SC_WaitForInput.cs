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
public class SC_WaitForInput : SequencerCommandBase
{
    public override string commandId{ get { return "waitForInput"; } }

    public override string commandType{ get { return "base"; } }
	

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_WaitForInput newCmd = ScriptableObject.CreateInstance(typeof(SC_WaitForInput)) as SC_WaitForInput;
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;

        myPlayer.inRewindMode = false;
        myPlayer.callBackFromCommand(true); 
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
    override public void drawCustomUi()
    { 
        GUILayout.Label("Wait For Input");
    }
#endif

    override public string toRenpy()
    {
        //target output: ?
        return "";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫\n";
    }
}