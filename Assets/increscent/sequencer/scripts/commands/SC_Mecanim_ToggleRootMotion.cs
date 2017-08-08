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
public class SC_Mecanim_ToggleRootMotion : SequencerCommandBase 
{
    public override string commandId{ get { return "SC_Mecanim_ToggleRootMotion"; } }
    public override string commandType{ get { return "base"; } }

    public string targetName = "";
    public bool rootMotionOn = true;

    override public SequencerCommandBase clone()
    {       
        SC_Mecanim_ToggleRootMotion newCmd = ScriptableObject.CreateInstance(typeof(SC_Mecanim_ToggleRootMotion)) as SC_Mecanim_ToggleRootMotion;
        newCmd.targetName = targetName;
        newCmd.rootMotionOn = rootMotionOn;
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
            GameObject target = sequencerData.getTargetModel(targetName).target;
            Animator anim = target.GetComponent<Animator>();

            if ( anim != null)
                anim.applyRootMotion = rootMotionOn;   
        }

        myPlayer.callBackFromCommand(); 
    }

    #if UNITY_EDITOR

    override public void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("switchRoot"));
    }

    override public void drawCustomUi()
    { 
        string[] nickChars = sequencerData.getTargetNickNamesByType(SequencerTargetTypes.character);

        GUILayout.Label("Mecanim Character Set Root Motion, who?:");
        if (nickChars != null && nickChars.Length > 0)
            targetName = nickChars [EditorGUILayout.Popup(sequencerData.getIndexFromArraySafe(nickChars, targetName), nickChars, GUILayout.Width(100))];

        SequencerTargetModel model = sequencerData.getTargetModel(targetName);
        if (model != null)
        {
            if (model.target == null)
                return;

            rootMotionOn = GUILayout.Toggle(rootMotionOn, "Root Motion On/Off:");
        }
    }
    #endif

    override public string toSequncerSerializedString()
    {
        return GetType().Name + "╫" + targetName + "╫" + rootMotionOn + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        targetName = splitString[1];
        rootMotionOn = bool.Parse(splitString[2]);
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