using UnityEngine;
using System.Collections.Generic;
using System;

public class SequencePlayer : MonoBehaviour
{
    public bool usingNGUI = false;
    public string startingScene;
    public bool autoStart = true;
    public int startingCommandIndex = 0;
    public SequencerData sequencerData;
    int currSectionIndex = -1;
    int currCommandIndex = -1;
    List<PlayerJumpModel> jumpsList = new List<PlayerJumpModel>();
    [HideInInspector]
    public bool
        inRewindMode = false;

    public DialogControllerBase dialogController;
    public ChoiceControllerBase choiceController;
    public InputControllerBase inputController;

    [HideInInspector]
    public bool
        blockForward = false;

    public Dictionary<string, string> runningTimeVariablesDictionary;

    [HideInInspector]
    public bool
        lastEvalResultBool = false;
    [HideInInspector]
    public int
        lastEvalResultInt = -1;
    [HideInInspector]
    public float
        lastEvalResultFloat = 1.0f;

    public Action<string> onChangeScene = delegate{};

    public UnityEngine.Events.UnityEvent<string> onEndSequence;

    void Start()
    {
        runningTimeVariablesDictionary = sequencerData.getVariablesAsDictionary();

        if (autoStart)
        {
            for (int i = 0; i < sequencerData.sections.Count; i++)
            {
                if (sequencerData.sections[i].name == startingScene)
                {
                    currSectionIndex = i;
                    currCommandIndex = startingCommandIndex;
                    break;
                }           
            } 

            if (currSectionIndex != -1 && sequencerData.sections[currSectionIndex].commandList.Count > 0)
            {
                onChangeScene(startingScene);
                play();
            }
            else
            {   
                if (currSectionIndex != -1 && sequencerData.sections[currSectionIndex].commandList.Count == 0)
                    Debug.LogWarning("Current section has no commands !");
                else
                    Debug.LogWarning("Could not find starting section named: " + startingScene);
            }
        }
    }

    public void startScene( string sceneName )
    {
        for (int i = 0; i < sequencerData.sections.Count; i++)
        {
            if (sequencerData.sections[i].name == sceneName)
            {
                currSectionIndex = i;
                currCommandIndex = startingCommandIndex;
                break;
            }           
        } 

        if (currSectionIndex != -1 && sequencerData.sections[currSectionIndex].commandList.Count > 0)
        {
            onChangeScene(sceneName);
            play();
        }
        else
        {   
            if (currSectionIndex != -1 && sequencerData.sections[currSectionIndex].commandList.Count == 0)
                Debug.LogWarning("Current section has no commands !");
            else
                Debug.LogWarning("Could not find starting section named: " + sceneName);
        }
    }

    //Executes the current command we are on (unless its disabled then skip to next one )
    public void play()
    {
        if ( !sequencerData.sections [currSectionIndex].commandList [currCommandIndex].isEnabled )
        {
            Debug.Log("!Skipped! (Was Disabled): Section :" + "( " + currSectionIndex + ") " + 
                sequencerData.sections [currSectionIndex].name + " command: " + "(" + currCommandIndex +
                " ) " + sequencerData.sections [currSectionIndex].commandList [currCommandIndex].name);
            
            if (inRewindMode)
                backward();
            else
                forward();
        }
    
        #if UNITY_EDITOR
            Debug.Log("Play Section :" + "( " + currSectionIndex + ") " + 
            sequencerData.sections [currSectionIndex].name + " command: " + "(" + currCommandIndex +
            " ) " + sequencerData.sections [currSectionIndex].commandList [currCommandIndex].name);
        #endif

        sequencerData.sections [currSectionIndex].commandList [currCommandIndex].execute(this);
    }
    
    //sets one command forward and executes (only if the command isnt blocking you )
    public void forward()
    {
        inRewindMode = false;

        if (blockForward)
        {
            blockForward = false;    
        } 
        else
        {
            if (currCommandIndex + 1 == sequencerData.sections [currSectionIndex].commandList.Count)
            {
                //should still call this commands forward, even if its the last guy
                sequencerData.sections [currSectionIndex].commandList [currCommandIndex].forward(this);

                Debug.Log("END of Sequence! or reached end of Section with out a jump.");

                onEndSequence.Invoke("auto");
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

    public void forceForward()
    {
        if (currCommandIndex + 1 == sequencerData.sections [currSectionIndex].commandList.Count)
        {
            Debug.Log("END of Sequence! or reached end of Section with out a jump.");
        } else
        {
            jumpToScene(sequencerData.sections [currSectionIndex].name, currCommandIndex + 1);
        }
    }

    //sets one command backward and executes
    public void backward()
    {
        if (!inRewindMode)
        {
            inRewindMode = true;
            sequencerData.sections [currSectionIndex].commandList [currCommandIndex].backward(this);
        }

        int lastJumpIndex = jumpsList.Count - 1;

        if (lastJumpIndex > -1 && jumpsList [lastJumpIndex].sectionJumpedTo == currSectionIndex && jumpsList [lastJumpIndex].commandJumpedTo == currCommandIndex)
        {
            currSectionIndex = jumpsList [lastJumpIndex].sectionJumpedFrom;
            currCommandIndex = jumpsList [lastJumpIndex].commandJumpedFrom;
            onChangeScene( sequencerData.getSectionNames()[jumpsList [lastJumpIndex].sectionJumpedTo]);
            jumpsList.RemoveAt(lastJumpIndex);   
            play();
        } 
        else if (currCommandIndex - 1 == -1)
        {
            jumpToScene("null", -1); //these values dont matter since we are rewinding
        }
        else
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
                onChangeScene(nameToJumpTo);
                play();
            } else
            {
                Debug.Log("You are at the starting index of the sequence, can't rewind further");
                inRewindMode = false;
                play();
            }
        } 
        else
        {
            int indexOfToSection = Array.IndexOf(sequencerData.getSectionNames(), nameToJumpTo);
            jumpsList.Add(new PlayerJumpModel(currSectionIndex, indexOfToSection, currCommandIndex, commandIndex));
            
            currSectionIndex = indexOfToSection;
            currCommandIndex = commandIndex;
            onChangeScene(nameToJumpTo);
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

    public void setEvalResultBool(bool result)
    {
        lastEvalResultBool = result;
    }

    public void setEvalResultInt(int result)
    {
        lastEvalResultInt = result;
    }
    
    public void setEvalResultFloat(float result)
    {
        lastEvalResultFloat = result;
    }   

    public string getCurrentSceneName()
    {
       return sequencerData.sections[currSectionIndex].name;
    }
}