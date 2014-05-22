using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SequencerData : MonoBehaviour
{
    public List<SequencerTargetModel> targets = new List<SequencerTargetModel>();
    public List<SequencerSectionModel> sections = new List<SequencerSectionModel>();

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
}