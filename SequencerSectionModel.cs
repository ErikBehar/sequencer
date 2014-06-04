using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SequencerSectionModel
{
    public string name;
    public List<SequencerCommandBase> commandList;

    public SequencerSectionModel clone()
    {
        SequencerSectionModel newSection = new SequencerSectionModel();
        newSection.name = name;
        newSection.commandList = new List<SequencerCommandBase>();
        
        
        foreach (SequencerCommandBase command in commandList)
        {
            newSection.commandList.Add(command.clone());
        }        

        return newSection;
    }
}
