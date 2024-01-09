using System;
using System.Collections.Generic;
using UnityEngine;

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

    static public string ParseTextForVars(string text, Dictionary<string, string> variableDict )
    {
        int count = 0;
        int max = 30; //max 30 variable replacements, so we dont crash in weird cases

        //variables
        while (text.IndexOf("[") > -1 && count < max)
        {
            count++;

            int indexOpen = text.IndexOf("[");
            if (indexOpen > -1)
            {
                int indexClose = text.IndexOf("]");
                string substring = text.Substring(indexOpen + 1, indexClose - (indexOpen + 1));
                if (variableDict.ContainsKey(substring))
                {
                    text = text.Replace("[" + substring + "]", variableDict[substring]);
                }
                else
                {
                    text = text.Substring(0, indexOpen) + "{" + substring + "}" + text.Substring(indexClose, text.Length - (indexClose + 1));
                }
            }
        }

        if (count > 30)
            Debug.LogError("Stopped Forever Loop or 30 + variable parse in SC_SetVariable.cs, make sure to fix this !");

        return text;
    }
}
