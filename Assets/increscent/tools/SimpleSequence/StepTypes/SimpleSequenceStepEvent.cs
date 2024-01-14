using System;
using UnityEngine.Events;

[Serializable]
public class SimpleSequenceStepEvent : SimpleSequenceStepBase
{
    public UnityEvent thingToDo;
    new public void onExecuteStep()
    {
        thingToDo.Invoke();
    }
}
