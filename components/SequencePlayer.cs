using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SequencePlayer : MonoBehaviour
{
    public string startingScene; 
    public SequencerData sequencerData;    

    private int currSceneIndex = -1;
    private int currCommandIndex = -1;

    private List<int> previousSceneList; //TODO also save the command index, so that we can go back in weird cases  

    [HideInInspector]
    public bool
        inRewindMode = false;

    public NguiDialogBubble dialogController;

    void Start()
    {
        for (int i = 0; i < sequencerData.sections.Count; i++)
        {
            if (sequencerData.sections [i].name == startingScene)
            {
                currSceneIndex = i;
                currCommandIndex = 0;
                break;
            }           
        } 

        if (currSceneIndex != -1)
        {
            previousSceneList = new List<int>();
            play();
        } else
        {   
            Debug.Log("Could not find starting section named: " + startingScene);
        }
    }

    //Executes the current command we are on
    public void play()
    {   
        Debug.Log("Play Section :" + "( " + currSceneIndex + ") " + sequencerData.sections [currSceneIndex].name + " command: " + "(" + currCommandIndex + " ) " + sequencerData.sections [currSceneIndex].commandList [currCommandIndex].name);
        sequencerData.sections [currSceneIndex].commandList [currCommandIndex].execute(this);
    }
    
    //sets one command forward and executes
    public void forward()
    {
        inRewindMode = false;

        sequencerData.sections [currSceneIndex].commandList [currCommandIndex].forward(this);

        if (currCommandIndex + 1 == sequencerData.sections [currSceneIndex].commandList.Count)
        {
            Debug.Log("END of Sequence! or reached end of Section with out a jump.");
        } else
        {
            currCommandIndex += 1;
            play();
        }
    }

    //sets one command backward and executes
    public void backward()
    {
        inRewindMode = true;

        sequencerData.sections [currSceneIndex].commandList [currCommandIndex].backward(this);

        if (currCommandIndex - 1 == -1)
        {
            jumpToScene("null", -1); //these values dont matter since we are rewinding
        } else
        {
            currCommandIndex -= 1;
            play();
        } 
    }
    
    public void callBackFromCommand()
    {
        callBackFromCommand(false);
    }
    
    public void callBackFromCommand(bool wait)
    {
        if (!wait)
        {
            if (inRewindMode)
                backward();
            else
                forward();
        }
    }
    
    public void jumpToScene(string nameToJumpTo, int commandIndex)
    {   
        if (inRewindMode)
        {
            if (previousSceneList.Count > 0)
            {   
                currSceneIndex = previousSceneList [previousSceneList.Count - 1];
                previousSceneList.RemoveAt(previousSceneList.Count - 1);

                currCommandIndex = sequencerData.sections [currSceneIndex].commandList.Count - 1;   
                play();
            } else
            {
                Debug.Log("You are at the starting index of the sequence, can't rewind further");
                inRewindMode = false;
                play();
            }
        } else
        {
            previousSceneList.Add(currSceneIndex);
            currSceneIndex = Array.IndexOf(sequencerData.getSectionNames(), nameToJumpTo);
            currCommandIndex = commandIndex;
            play();
        }
    }

    public void input(int direction)
    {
        if (direction > 0)
            forward();
        else
            backward();      
    }
}