using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSequence : MonoBehaviour
{
    public string sequenceDescription;
    public bool waitingForCallback = false;
    public int currIndex = 0;
    public bool useRealtime = false;

    [SerializeReference, Subclass(IsList = true)]
    public List<SimpleSequenceStepBase> steps;

    public void addStepWithType( Type stepType)
    {
        SimpleSequenceStepBase step = (SimpleSequenceStepBase)Activator.CreateInstance(stepType);
        steps.Add(step);
    }

    private void OnEnable()
    {
        executeStep();
    }

    private void OnDestroy()
    {
        //cleanup 
        for (int i = steps.Count-1; i > -1; i--)
        {
            SimpleSequenceStepBase step = steps[i];
            step.callBack -= OnCallback;
        }
    }

    private void executeStep()
    {
        if (!enabled)
        {
            StopAllCoroutines();
            return;
        }
        
        if (steps.Count > currIndex)
        {
            Debug.Log("Executing step: " + (currIndex+1) + " of " + steps.Count + " in sequence: " + sequenceDescription);
            SimpleSequenceStepBase currStep = steps[currIndex];
            currStep.onExecuteStep();
            if (currStep.deliverCallback.GetPersistentEventCount() > 0)
            {
                waitingForCallback = true;
                currStep.callBack += OnCallback;
                currStep.deliverCallback.Invoke(currStep.callBack);
            }
            else
            {
                //Note in case of no timer it just does the next action immediately
                StartCoroutine(WaitForNextAction(currStep.timerToNextAction, currStep.skipToIndex));
            }
        }
        else
        {
            Debug.Log("REACHED END OF SEQUENCE, stopping coroutines for" + sequenceDescription);
            StopAllCoroutines();
        }
    }

    IEnumerator WaitForNextAction( float time, int customIndex)
    {
        if (useRealtime)
            yield return new WaitForSecondsRealtime(time);
        else
            yield return new WaitForSeconds(time);

        if (customIndex != -1)
            currIndex = customIndex;
        else
            currIndex += 1;

        executeStep();
    }

    //Note that on callback we ignore the skipToIndex only use the callback index
    private void OnCallback(int indexToJump)
    {
        waitingForCallback = false;

        SimpleSequenceStepBase currStep = steps[currIndex];
        currStep.callBack -= OnCallback;

        StartCoroutine(WaitForNextAction(currStep.timerToNextAction, indexToJump));
    }
}
