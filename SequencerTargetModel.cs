using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SequencerTargetModel
{
    public GameObject target;
    public string nickname; 

    public SequencerTargetModel clone()
    {
        SequencerTargetModel newTarget = new SequencerTargetModel();
        newTarget.target = target;
        newTarget.nickname = nickname;
        return newTarget;
    }
}
