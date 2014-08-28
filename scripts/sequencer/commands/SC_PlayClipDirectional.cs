using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System;

/// <summary>
/// 3D : Plays Animation Clip in direction of Sequencer Play : Sequencer Command
/// ONLY works with 3d Characters !!!
/// Plays Animation Clip in direction of Sequencer Play
/// </summary>

[Serializable]
public class SC_PlayClipDirectional : SequencerCommandBase
{
    public override string commandId{ get { return "playDirectional"; } }
    public override string commandType{ get { return "base"; } }

    public string targetName = "";
    public string expressionName = "";
   
    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_PlayClipDirectional newCmd = ScriptableObject.CreateInstance(typeof(SC_PlayClipDirectional)) as SC_PlayClipDirectional;
        newCmd.targetName = targetName;
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

            charcomp.setExpression(expressionName, false);   
        }

        myPlayer.callBackFromCommand(); 
    }
    
    override public void undo()
    {
        GameObject target = sequencerData.getTargetModel(targetName).target;
        VN_CharBase charcomp = target.GetComponent<VN_CharBase>();

        charcomp.setExpression(expressionName, true);
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
        
        GUILayout.Label("Play Clip Directional who?:");
        targetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, targetName), nickChars, GUILayout.Width(100))];

        SequencerTargetModel model = sequencerData.getTargetModel(targetName);
        if (model != null)
        {
            if (model.target == null)
                return;
            VN_CharBase charcomp = model.target.GetComponent<VN_CharBase>();
            
            if (charcomp != null)
            {
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
        return "";
    }

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + targetName + "╫" + expressionName + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        targetName = splitString [1];
        expressionName = splitString [2];
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