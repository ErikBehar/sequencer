using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SequencerTargetModel
{
    public string type = SequencerTargetTypes.positional;
    public GameObject target;
    public string nickname; 

    public SequencerTargetModel clone()
    {
        SequencerTargetModel newTarget = new SequencerTargetModel();
        newTarget.target = target;
        newTarget.nickname = nickname;
        newTarget.type = type;
        return newTarget;
    }
}
