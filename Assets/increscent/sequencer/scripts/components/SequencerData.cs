using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

[Serializable]
public class SequencerData : MonoBehaviour
{
    public List<SequencerTargetModel> targets = new List<SequencerTargetModel>();
    public List<SequencerSectionModel> sections = new List<SequencerSectionModel>();

    public List<SequencerVariableModel> variables = new List<SequencerVariableModel>();

    public Color disabledColor = Color.grey;
    public Color normalRowColor = new Color(0f, 1f, 1f, .15f);
    public Color normalAltRowColor = new Color(1f, 0f, 1f, .15f);
    public Color stubColor = new Color(1f, 1f, 0, .15f);
    public Color dropColor = new Color(0f, 1f, 0f, .25f);
    public Color dragColor = new Color(1f, 0f, 0f, .25f);

    public int pagingSize = 100;

    public Dictionary<string,Texture> iconDictionary = new Dictionary<string,Texture>();

    private Texture defaultTexture;

    //0 is not valid, store last used uid here
    [ReadOnly]
    public uint last_uid = 0;

    public uint NewUID()
    {
        return last_uid += 1;
    }

    public Texture getIconTexture(string name)
    {
        #if UNITY_EDITOR
            if (iconDictionary.Count == 0)
            {
                iconDictionary.Add( "default", (Texture)EditorGUIUtility.Load("increscent/sequencer/generic_icon.png"));
                iconDictionary.Add( "dialog", (Texture)EditorGUIUtility.Load("increscent/sequencer/dialog_icon.png"));
                iconDictionary.Add( "hide", (Texture)EditorGUIUtility.Load("increscent/sequencer/hide_icon.png"));
                iconDictionary.Add( "show", (Texture)EditorGUIUtility.Load("increscent/sequencer/show_icon.png"));
                iconDictionary.Add( "stub", (Texture)EditorGUIUtility.Load("increscent/sequencer/note_icon.png"));
                iconDictionary.Add( "matchTrans", (Texture)EditorGUIUtility.Load("increscent/sequencer/move_icon.png"));
                iconDictionary.Add( "pause", (Texture)EditorGUIUtility.Load("increscent/sequencer/pause_icon.png"));
                iconDictionary.Add( "triggerAnim", (Texture)EditorGUIUtility.Load("increscent/sequencer/triggerAnim_icon.png"));
                iconDictionary.Add( "wait", (Texture)EditorGUIUtility.Load("increscent/sequencer/wait_icon.png"));
                iconDictionary.Add( "setWaypoint", (Texture)EditorGUIUtility.Load("increscent/sequencer/waypoint_icon.png"));
                iconDictionary.Add( "sendMessage", (Texture)EditorGUIUtility.Load("increscent/sequencer/sendMessage_icon.png"));
                iconDictionary.Add( "input", (Texture)EditorGUIUtility.Load("increscent/sequencer/input_icon.png"));
                iconDictionary.Add( "switchRoot", (Texture)EditorGUIUtility.Load("increscent/sequencer/switchRoot_icon.png"));
                iconDictionary.Add( "waitProximity", (Texture)EditorGUIUtility.Load("increscent/sequencer/proximity_icon.png"));
                iconDictionary.Add( "choice", (Texture)EditorGUIUtility.Load("increscent/sequencer/choice_icon.png"));
                iconDictionary.Add( "jump", (Texture)EditorGUIUtility.Load("increscent/sequencer/jump_icon.png"));
        }

            if ( iconDictionary.ContainsKey( name))
                return iconDictionary[name];
            else
                return iconDictionary["default"];
#else
            return defaultTexture;
#endif
    }

    public string[] getSectionNames()
    {
        string[] names = new string[sections.Count];
        for (int i = 0; i < sections.Count; i++)
        {
            names [i] = sections [i].name;   
        }
        
        return names;
    }
    
    public string[] getTargetNickNames()
    {
        string[] names = new string[targets.Count];
        for (int i = 0; i < targets.Count; i++)
        {
            names [i] = targets [i].nickname;   
        }
        
        return names;
    }

    public string[] getTargetNickNamesByType(string type)
    {
        List<string> names = new List<string>();
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets [i].type == type)
                names.Add(targets [i].nickname);   
        }

        if (names.Count == 0)
            return null;

        return names.ToArray();
    }

    public int getIndexOfTarget(string name)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets [i].nickname == name)
                return i;
        }
    
        return 0; // this a good idea?
    }

    public int getIndexOfSection(string name)
    {
        for (int i = 0; i < sections.Count; i++)
        {
            if (sections [i].name == name)
                return i;
        }
        
        return 0; // this a good idea?
    }

    public SequencerTargetModel getTargetModel(string name)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets [i].nickname == name)
                return targets [i];
        }
        if (targets.Count == 0)
            return null;

        return targets [0];// is this a good idea?
    }
    
    public SequencerSectionModel getSectionModel(string name)
    {
        for (int i = 0; i < sections.Count; i++)
        {
            if (sections [i].name == name)
                return sections [i];
        }
        
        return sections [0];// this a good idea?
    }

    public int getIndexFromArraySafe(string[] array, string name)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array [i] == name)
                return i;
        }

        return 0;
    }

    public string getNameFromArraySafe(string[] array, string name)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array [i] == name)
                return array [i];
        }
        
        if (array.Length > 0)
            return array [0];
        
        return "";
    }

    public string[] getNamesOfVariables()
    {
        string[] varNames = new string[ variables.Count];
        for (int i = 0; i < variables.Count; i++)
        {
            varNames [i] = variables [i].name;
        } 
        return varNames;   
    }

    public int getIndexOfVariable(string name)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables [i].name == name)
                return i; 
        } 
        return -1;
    }

    public Dictionary<string,string> getVariablesAsDictionary()
    {
        Dictionary<string,string> variablesDic = new Dictionary<string, string>();
        
        foreach (SequencerVariableModel vModel in variables)
        {
            variablesDic.Add(vModel.name, vModel.value);
        }

        return variablesDic;
    }
}