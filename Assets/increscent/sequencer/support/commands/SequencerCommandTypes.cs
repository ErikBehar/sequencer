using UnityEngine;
using System.Collections;
using System;

public class SequencerCommandTypes
{
    //put your custom commands in here
    //watch out if removing commands that have references 
    static public Type[] commandTypes = new Type[]
    {
        typeof(SC_Jump),
        typeof(SC_MuteAllSfx),
        typeof(SC_Pause),
        typeof(SC_PlayMusic),
        typeof(SC_PlaySfx),
        typeof(SC_StopMusic),
        typeof(SC_VN_Choice),
        typeof(SC_VN_Dialog),
        typeof(SC_VN_Hide),
        typeof(SC_VN_Modify),
        typeof(SC_VN_Show),
        typeof(SC_ClearAll),
        typeof(SC_SetVariable),
        typeof(SC_VN_ExpressionJump),
        typeof(SC_InputVariable),
        typeof(SC_VN_Flip),
		typeof(SC_WaitForInput),
        typeof(SC_PlayClipDirectional),
    }; 
    
    static public string[] getAsStringArray()
    {
        string[] names = new string[commandTypes.Length];
        for (int i = 0; i < commandTypes.Length; i++)
        {
            names [i] = commandTypes [i].ToString();   
        }
        
        return names;
    }
}