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

    public string targetName = "";
    public string attireName = "";
    public string expressionName = "";
    private string prevExpressionName;
    private string prevAttireName = "";
   
    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Modify newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Modify)) as SC_VN_Modify;
        newCmd.targetName = targetName;
        newCmd.attireName = attireName;
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
            GameObject target = sequencerData.getTargetModel(targetName).target;
            VN_CharBase charcomp = target.GetComponent<VN_CharBase>();

            string[] attireNames = charcomp.getAttireNames();
            string[] expressionNames = charcomp.getExpressionNames();

            if (attireNames != null && attireNames.Length > 0)
                prevAttireName = charcomp.getCurrentAttireName();
            if (expressionNames != null && expressionNames.Length > 0)
                prevExpressionName = charcomp.getCurrentExpressionName();

            charcomp.setAttire(attireName);
            charcomp.setExpression(expressionName, false);            
        }

        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        GameObject target = sequencerData.getTargetModel(targetName).target;
        VN_CharBase charcomp = target.GetComponent<VN_CharBase>();

        charcomp.setAttire(prevAttireName);
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
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);
        
        GUILayout.Label("modify who?:");
        targetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, targetName), nickChars, GUILayout.Width(100))];

        SequencerTargetModel model = sequencerData.getTargetModel(targetName);
        if (model != null)
        {
            if (model.target == null)
                return;
            VN_CharBase charcomp = model.target.GetComponent<VN_CharBase>();
            
            if (charcomp != null)
            {
                string[] attires = charcomp.getAttireNames();
                GUILayout.Label("attire:");
                if (attires != null && attires.Length > 0)
                    attireName = attires [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(attires, attireName), attires, GUILayout.Width(100))];
    
                string[] expressions = charcomp.getExpressionNames();
                GUILayout.Label("expression:");
                if (expressions != null && expressions.Length > 0)
                    expressionName = expressions [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(expressions, expressionName), expressions, GUILayout.Width(100))];
            }
        }
    }
#endif

    override public string toRenpy()
    {
        //target output: show ami normal at center with dissolve
        return "show " + targetName + " " + attireName + " " + expressionName + " at center with dissolve\n";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + targetName + "╫"
            + attireName + "╫" + expressionName + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        targetName = splitString [1];
        attireName = splitString [2];
        expressionName = splitString [3];
    }
}