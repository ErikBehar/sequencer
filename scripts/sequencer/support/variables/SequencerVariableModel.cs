using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SequencerVariableModel
{
    public string name;
    public string value;

    public SequencerVariableModel()
    {
    }

    public SequencerVariableModel(string in_name, string in_value)
    {
        name = in_name;
        value = in_value;
    }

    public SequencerVariableModel clone()
    {
        SequencerVariableModel newVariable = new SequencerVariableModel();
        newVariable.name = name;
        newVariable.value = value;
        return newVariable;
    }
}
