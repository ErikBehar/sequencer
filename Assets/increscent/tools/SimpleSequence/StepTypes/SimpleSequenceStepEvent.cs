using System;
using UnityEngine.Events;

[Serializable]
public class SimpleSequenceStepEvent : SimpleSequenceStepBase
{
    public UnityEvent thingToDo;
    override public void onExecuteStep()
    {
        thingToDo.Invoke();
    }
}
