using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// 2D : Visual Novel: Flip Targets  : Sequencer Command
/// Sends a message to target to flip -1 scale or 180 for 3d chars
/// this works in conjunction with the object having a "VN_Character" component on it
/// </summary>

[Serializable]
public class SC_VN_Flip : SequencerCommandBase
{
    public override string commandId{ get { return "flip"; } }
    public override string commandType{ get { return "base"; } }

    public string targetName = "";

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Flip newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Flip)) as SC_VN_Flip;
        newCmd.targetName = targetName;
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
            GameObject target = sequencerData.getTargetModel(targetName).target;
            VN_CharBase charcomp = target.GetComponent<VN_CharBase>();
            charcomp.flip();      
        }

        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        GameObject target = sequencerData.getTargetModel(targetName).target;
        VN_CharBase charcomp = target.GetComponent<VN_CharBase>();
        charcomp.flip(); 
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
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);
        
        GUILayout.Label("modify who?:");
        targetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, targetName), nickChars, GUILayout.Width(100))];
    }
#endif
}