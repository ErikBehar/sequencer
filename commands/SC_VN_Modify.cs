using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// 2D : Visual Novel: Modify Targets  : Sequencer Command
/// Sends a message to target to change attire / pose / facial expression
/// this works in conjunction with the object having a "VN_Character" component on it
/// </summary>

[Serializable]
public class SC_VN_Modify : SequencerCommandBase
{
    public override string commandId{ get { return "modify"; } }
    public override string commandType{ get { return "base"; } }

    public int targetIndexList = 0;
    public int attireListIndex = 0;
    public int expressionListIndex = 0;
    public string expressionName;
    private string prevExpressionName;
    private int prevAttireIndex = 0;
   
    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Modify newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Modify)) as SC_VN_Modify;
        newCmd.targetIndexList = targetIndexList;
        newCmd.attireListIndex = attireListIndex;
        newCmd.expressionListIndex = expressionListIndex;
        newCmd.expressionName = expressionName;
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
            GameObject target = sequencerData.targets [targetIndexList].target;
            VN_CharBase charcomp = target.GetComponent<VN_CharBase>();

            prevAttireIndex = charcomp.getCurrentAttire();
            prevExpressionName = charcomp.getCurrentExpressionName();

            charcomp.setAttire(attireListIndex);
            charcomp.setExpression(expressionName, false);            
        }

        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        GameObject target = sequencerData.targets [targetIndexList].target;
        VN_CharBase charcomp = target.GetComponent<VN_CharBase>();

        charcomp.setAttire(prevAttireIndex);
        charcomp.setExpression(prevExpressionName, true);
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
        string[] nicks = sequencerData.getTargetNickNames();
        
        GUILayout.Label("modify who?:");
        targetIndexList = EditorGUILayout.Popup(targetIndexList, nicks, GUILayout.Width(100));

        if (sequencerData.targets [targetIndexList] != null)
        {
            VN_CharBase charcomp = sequencerData.targets [targetIndexList].target.GetComponent<VN_CharBase>();
            
            if (charcomp != null)
            {
                string[] attires = charcomp.getAttireNames();
                GUILayout.Label("attire:");
                attireListIndex = EditorGUILayout.Popup(attireListIndex, attires, GUILayout.Width(100));
    
                string[] expressions = charcomp.getExpressionNames();
                GUILayout.Label("expression:");
                expressionListIndex = EditorGUILayout.Popup(expressionListIndex, expressions, GUILayout.Width(100));
                if (expressionListIndex < expressions.Length)
                    expressionName = expressions [expressionListIndex];
                else
                {
                    Debug.LogWarning("Error here");
                    expressionName = expressions [0];
                }
            }
        }
    }
#endif
}