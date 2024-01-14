using System;

[Serializable]
public class SimpleSequenceStepOther : SimpleSequenceStepBase
{
    //example of adding your own
    public int cheese = 200;

    override public void onExecuteStep()
    {
        UnityEngine.Debug.Log("Do some other thing");
    }
}
