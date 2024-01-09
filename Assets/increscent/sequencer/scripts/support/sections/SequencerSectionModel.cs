using System;
using System.Collections.Generic;

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
    
    public void rename(string newName)
    {
        name = newName;
        foreach (SequencerCommandBase command in commandList)
        {
            command.sectionName = newName;
        }   
    }
}
