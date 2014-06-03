using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SequencePlayer : MonoBehaviour
{
    public string startingScene;
    public SequencerData sequencerData;
    private int currSectionIndex = -1;
    private int currCommandIndex = -1;
    private List<PlayerJumpModel> jumpsList = new List<PlayerJumpModel>();
    [HideInInspector]
    public bool
        inRewindMode = false;
    public NguiDialogBubble dialogController;
    public NguiChoiceButtons choiceController;
    public bool blockForward = false;

    void Start()
    {
        for (int i = 0; i < sequencerData.sections.Count; i++)
        {
            if (sequencerData.sections [i].name == startingScene)
            {
                currSectionIndex = i;
                currCommandIndex = 0;
                break;
            }           
        } 

        if (currSectionIndex != -1)
        {
            play();
        } else
        {   
            Debug.Log("Could not find starting section named: " + startingScene);
        }
    }

    //Executes the current command we are on
    public void play()
    {   
        Debug.Log("Play Section :" + "( " + currSectionIndex + ") " + sequencerData.sections [currSectionIndex].name + " command: " + "(" + currCommandIndex + " ) " + sequencerData.sections [currSectionIndex].commandList [currCommandIndex].name);
        sequencerData.sections [currSectionIndex].commandList [currCommandIndex].execute(this);
    }
    
    //sets one command forward and executes
    public void forward()
    {
        inRewindMode = false;

        if (blockForward)
        {
            blockForward = false;
                
        } else
        {
            if (currCommandIndex + 1 == sequencerData.sections [currSectionIndex].commandList.Count)
            {
                Debug.Log("END of Sequence! or reached end of Section with out a jump.");
            } else
            {
                sequencerData.sections [currSectionIndex].commandList [currCommandIndex].forward(this);

                if (!blockForward)
                {
                    currCommandIndex += 1;
                    play();
                } else
                {
                    blockForward = false;
                }
            }
        }
    }

    //sets one command backward and executes
    public void backward()
    {
        inRewindMode = true;

        sequencerData.sections [currSectionIndex].commandList [currCommandIndex].backward(this);

        int lastJumpIndex = jumpsList.Count - 1;

        if (lastJumpIndex > -1 && jumpsList [lastJumpIndex].sectionJumpedTo == currSectionIndex && jumpsList [lastJumpIndex].commandJumpedTo == currCommandIndex)
        {
            currSectionIndex = jumpsList [lastJumpIndex].sectionJumpedFrom;
            currCommandIndex = jumpsList [lastJumpIndex].commandJumpedFrom;
            jumpsList.RemoveAt(lastJumpIndex);   
            play();
        } else if (currCommandIndex - 1 == -1)
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
            int lastJumpIndex = jumpsList.Count - 1;
            if (lastJumpIndex > -1)
            {   
                currSectionIndex = jumpsList [lastJumpIndex].sectionJumpedFrom;
                currCommandIndex = jumpsList [lastJumpIndex].commandJumpedFrom;
                
                jumpsList.RemoveAt(lastJumpIndex);
                play();
            } else
            {
                Debug.Log("You are at the starting index of the sequence, can't rewind further");
                inRewindMode = false;
                play();
            }
        } else
        {
            int indexOfToSection = Array.IndexOf(sequencerData.getSectionNames(), nameToJumpTo);
            jumpsList.Add(new PlayerJumpModel(currSectionIndex, indexOfToSection, currCommandIndex, commandIndex));
            
            currSectionIndex = indexOfToSection;
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