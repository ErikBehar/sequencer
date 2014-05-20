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

        SequencerCommandBase[] temp = new SequencerCommandBase[ commandList.Count];
        commandList.CopyTo(temp);
        newSection.commandList = new List<SequencerCommandBase>(temp);

        return newSection;
    }
}
