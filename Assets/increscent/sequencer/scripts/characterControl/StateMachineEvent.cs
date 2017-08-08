using UnityEngine;

public class StateMachineEvent : StateMachineBehaviour
{
    public GameObject targetToNotify;

    public string enterFunctionName = "na";
    public string enterParam = "na";

    public string exitFunctionName = "na";
    public string exitParam = "na";

    public string eventName = "na";

    public float normalizedTimeToForceExit = .97f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ( enterFunctionName != "na")
            targetToNotify.SendMessage( enterFunctionName, enterParam);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("Exit: " + eventName);
        doExit();
    }

    void doExit()
    {
        if (exitFunctionName != "na")
            targetToNotify.SendMessage( exitFunctionName, exitParam);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > normalizedTimeToForceExit)
        {
            doExit();
        }
    }

//    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//    {
//        
//    }
//    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//    {
//        
//    }
}