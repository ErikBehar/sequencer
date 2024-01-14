using System;
using UnityEngine.Events;


[Serializable]
public class MyCallbackEvent : UnityEvent<Action<int>>
{
}

[Serializable]
public abstract class SimpleSequenceStepBase
{
    public string StepDescription;
    public MyCallbackEvent deliverCallback;
    public Action<int> callBack;
    //time to wait before going to next step (0 = no wait)
    public float timerToNextAction = 0;
    //index to skip to at end of step ( -1 = just go to next one)
    public int skipToIndex = -1;

    abstract public void onExecuteStep();
}