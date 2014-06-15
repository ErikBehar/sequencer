using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SequencerData : MonoBehaviour
{
    public List<SequencerTargetModel> targets = new List<SequencerTargetModel>();
    public List<SequencerSectionModel> sections = new List<SequencerSectionModel>();

    public List<SequencerVariableModel> variables = new List<SequencerVariableModel>();

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
        
        return targets [0];// this a good idea?
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