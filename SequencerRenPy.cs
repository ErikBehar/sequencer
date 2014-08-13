using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SequencerRenPy 
{
    SequencerSectionModel currentSection;

    List<SC_VN_Choice> choiceCommandList = new List<SC_VN_Choice>();
    Dictionary<SC_VN_Choice, List<SC_Jump>> choiceToJumpsDictionary = new Dictionary<SC_VN_Choice, List<SC_Jump>>();
    Dictionary<SC_VN_Choice, int> choiceToCurrentResolvedIndex = new Dictionary<SC_VN_Choice, int>();
    List<int> choiceEndLineList = new List<int>();

    //this is for snippets, may not have a scene target, so append to current selected scene!
    public void renpyToSequencer(string text, SequencerData sequencerData, int lastSelectedSection)
    {
        currentSection = sequencerData.sections[ lastSelectedSection ];
        renpyToSequencer(text, sequencerData);
    }

    public void renpyToSequencer(string text, SequencerData sequencerData)
    {
        string[] splitFile = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        bool commandWasValid = true;
        for (int i=0; i < splitFile.Length; i++)
        {
            string myString = splitFile [i].Replace('\t', ' '); 
            string[] splitString = myString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string firstWord = (splitString.Length > 0) ? splitString [0] : "EMPTY";

            //for use below
            SequencerTargetModel targetModel;
    
            commandWasValid = true;
            switch (firstWord)
            {
                case ("label"):
                    currentSection = checkSectionExistsOrCreate(splitString [1], sequencerData);
                    break;
                case ("scene"):
                    //also add a clearAll automatically ( cause I think ren'py does this on scene switch )
                    SC_ClearAll clear = ScriptableObject.CreateInstance(typeof(SC_ClearAll)) as SC_ClearAll;
                    initCommandAndAddToSection(currentSection, clear, sequencerData);

                    //bg's are treated like characters cause you can manipulate them and move em around
                    //scene bg park with wipeleft
                    SC_VN_Show show = ScriptableObject.CreateInstance(typeof(SC_VN_Show)) as SC_VN_Show;
                    initCommandAndAddToSection(currentSection, show, sequencerData);
                    SC_VN_Modify modify = ScriptableObject.CreateInstance(typeof(SC_VN_Modify)) as SC_VN_Modify;
                    initCommandAndAddToSection(currentSection, modify, sequencerData);
    
                    if (splitString [1] == " " || splitString.Length < 1)
                        break;

                    targetModel = checkTargetExistsOrCreate(splitString [1], sequencerData);                    
                    targetModel.type = SequencerTargetTypes.character; 
    
//                    TODO: use the transition type
//                    string transitionType;
//                    if (splitString.Length > 2)
//                        transitionType = splitString [3];
//                    else
//                        transitionType = "instant";
        
                    show.time = 0;
                    show.useFrom = false;
                    modify.targetName = show.lastSelectedWho = targetModel.nickname;

                    show.lastSelectedTo = checkTargetExistsOrCreate("centerPos", sequencerData).nickname;

                    modify.expressionName = splitString [2];
                    break;
                case ("hide"):
                    SC_VN_Hide hide = ScriptableObject.CreateInstance(typeof(SC_VN_Hide)) as SC_VN_Hide;
                    initCommandAndAddToSection(currentSection, hide, sequencerData);
                    
                    //TODO: this will only hide in place, but probably inteded to hide in some direction with a transition type

                    if (splitString [1] == " " || splitString [1].Length < 1)
                        break;

                    targetModel = checkTargetExistsOrCreate(splitString [1], sequencerData);
                    hide.lastSelectedWho = targetModel.nickname;
                    hide.useTo = false; 
                    break;
                case ("jump"):
                    SC_Jump jump = ScriptableObject.CreateInstance(typeof(SC_Jump)) as SC_Jump;
                    initCommandAndAddToSection(currentSection, jump, sequencerData);

                    SequencerSectionModel jumpSection = checkSectionExistsOrCreate(splitString [1], sequencerData);
                    jump.targetSectionName = jumpSection.name;
                    jump.commandIndex = 0;
                    jump.targetCommand = (jumpSection.commandList.Count > 0) ? jumpSection.commandList [0] : null;
                    break;
                case ("menu:"):
                    SC_VN_Choice choice = ScriptableObject.CreateInstance(typeof(SC_VN_Choice)) as SC_VN_Choice;
                    initCommandAndAddToSection(currentSection, choice, sequencerData);
                    
                    choice.sectionNameList = new List<string>();
                    choice.commandIndexList = new List<int>();
                    choice.optionTextList = new List<string>();
                    choice.targetCommandList = new List<SequencerCommandBase>();

                    //we will treat options as part of same scene ( more akin to ren'py style ) 
                    int choiceTabs = splitFile [i].Split('\t').Length - 1;
                    int startLineIndex = i;

                    while (true)
                    {
                        i += 1;

                        //check for last line
                        if ( i >= splitFile.Length)
                            break;

                        string currString = splitFile [i]; 
                        string currStringCheck = splitFile [i].Replace('\t', ' '); 
                        string[] splitStringCheck = currStringCheck.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splitStringCheck.Length > 0 && splitFile [i].Split('\t').Length - 1 <= choiceTabs)
                        {
                            choiceEndLineList.Add(i);
                            break;
                        }

                        //look for choice lines
                        string[] splitCurrString = currString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splitCurrString [splitCurrString.Length - 1].Contains("\":"))
                        {
                            choice.sectionNameList.Add(currentSection.name);
                            choice.commandIndexList.Add(i);
                            choice.targetCommandList.Add(null);
                            int indexOfQuotes = currString.IndexOf("\"");
                            choice.optionTextList.Add(currString.Substring(indexOfQuotes + 1, currString.LastIndexOf(":") - 2 - indexOfQuotes)); // cut out the  :
                        } 
                    }

                    choice.size = choice.commandIndexList.Count;

                    choiceCommandList.Add(choice);
                    choiceToJumpsDictionary.Add(choice, new List<SC_Jump>());
                    choiceToCurrentResolvedIndex.Add(choice, 0);

                    //reset back to the line we were on    
                    i = startLineIndex;
                    break;
                case ("pause"):
                    SC_Pause pause = ScriptableObject.CreateInstance(typeof(SC_Pause)) as SC_Pause;
                    initCommandAndAddToSection(currentSection, pause, sequencerData);
                    
                    pause.time = .25f; //? some default time
                    if (splitString.Length > 1)
                        pause.time = float.Parse(splitString [1]);                   
                    break;
                case ("play"):    
                    //TODO deal with fadein time
                    if (splitString [1] == "music")
                    {
                        SC_PlayMusic playMusic = ScriptableObject.CreateInstance(typeof(SC_PlayMusic)) as SC_PlayMusic;
                        initCommandAndAddToSection(currentSection, playMusic, sequencerData); 
                        int startIndex = myString.IndexOf("\"") + 1;
                        int endIndex = myString.IndexOf(".");
                        playMusic.audioClipName = myString.Substring(startIndex, endIndex - startIndex);
                        
                        if (playMusic.audioClipName.Contains("/"))
                        {
                            int endSlashIndex = playMusic.audioClipName.LastIndexOf("/") + 1;
                            playMusic.audioClipName = playMusic.audioClipName.Substring(endSlashIndex, playMusic.audioClipName.Length - endSlashIndex);
                        }
                    } else if (splitString [1] == "sound")
                    {
                        SC_PlaySfx playAudio = ScriptableObject.CreateInstance(typeof(SC_PlaySfx)) as SC_PlaySfx;
                        initCommandAndAddToSection(currentSection, playAudio, sequencerData); 
                        int startIndex = myString.IndexOf("\"") + 1;
                        int endIndex = myString.IndexOf(".");
                        playAudio.audioClipName = myString.Substring(startIndex, endIndex - startIndex);

                        if (playAudio.audioClipName.Contains("/"))
                        {
                            int endSlashIndex = playAudio.audioClipName.LastIndexOf("/") + 1;
                            playAudio.audioClipName = playAudio.audioClipName.Substring(endSlashIndex, playAudio.audioClipName.Length - endSlashIndex);
                        }
                    }      
                    break;
                case ("show"):
                    show = ScriptableObject.CreateInstance(typeof(SC_VN_Show)) as SC_VN_Show;
                    initCommandAndAddToSection(currentSection, show, sequencerData);
                    
                    modify = ScriptableObject.CreateInstance(typeof(SC_VN_Modify)) as SC_VN_Modify;
                    initCommandAndAddToSection(currentSection, modify, sequencerData);
  
                    //examples:
                    //show character attire expression at position with transitiontype
                    //show laura happy at center with dissolve  7
                    //show blu normal at right                  5
                    //show ahri A1 comedyAngry at lr          6
                    //show ryuu B1 neutral at rt              6
                    //show ryuu B1 sexy                      4
                    //show kichihiro angry                    3
                    //show kichiro drop at rt                  5  

                    if (splitString [1] == " " || splitString [1].Length < 1)
                        break;

                    targetModel = checkTargetExistsOrCreate(splitString [1], sequencerData);
                    targetModel.type = SequencerTargetTypes.character;
                    show.lastSelectedWho = modify.targetName = targetModel.nickname;
                            
                    List<string> parts = new List<string>();
                    int hadAt = -1;
                    int hadWith = -1;
 
                    for (int c=2; c < splitString.Length; c++)
                    {
                        if (splitString [c] == "at")
                            hadAt = c;
                        else if (splitString [c] == "with")
                        {
                            hadWith = c;
                            break;
                        } else if (hadAt == -1)
                            parts.Add(splitString [c]);
                    }
            
                    string positionTargetName = "";
                    if (hadAt > -1)
                        positionTargetName = splitString [hadAt + 1];    

                    //TODO: do something with transition type
                    string transitionType = "";
                    if (hadWith > -1)     
                        transitionType = splitString [hadWith + 1];    
                    transitionType += ""; //remove me later, just to get rid of warning                                  
        
                    string attire = "";
                    string expression = "";
                        
                    if (parts.Count > 1)
                    {
                        attire = parts [0];
                        expression = parts [1];
                    } else
                        expression = parts [0];
                    
                    show.useFrom = false;

                    //this should just use previous position of this char, so go back and look for one
                    if (positionTargetName == " " || positionTargetName.Length < 1)
                    {  
                        if (currentSection.commandList.Count > 3)
                        {
                            for (int g = currentSection.commandList.Count-3; g > -1; g--)
                            {
                                if (currentSection.commandList [g] is SC_VN_Show)
                                {
                                    SC_VN_Show temp = (SC_VN_Show)currentSection.commandList [g];
                                    if (temp.lastSelectedWho == show.lastSelectedWho)
                                    {
                                        positionTargetName = temp.lastSelectedTo;
                                        break;
                                    }
                                }
                            }
                        }
                        
                        if (positionTargetName == "")
                            positionTargetName = "centerPos";
                    }

                    show.lastSelectedTo = checkTargetExistsOrCreate(positionTargetName, sequencerData).nickname;
                    modify.attireName = attire;
                    modify.expressionName = expression;
                    break;   
                case ("stop"):        
                    SC_StopMusic stopMusic = ScriptableObject.CreateInstance(typeof(SC_StopMusic)) as SC_StopMusic;
                    initCommandAndAddToSection(currentSection, stopMusic, sequencerData);  
                    if (splitString.Length > 2)
                        stopMusic.fadeTime = float.Parse(splitString [3]);         
                    break;
                case ("voice"):           
                    SC_PlaySfx playSfx = ScriptableObject.CreateInstance(typeof(SC_PlaySfx)) as SC_PlaySfx;
                    initCommandAndAddToSection(currentSection, playSfx, sequencerData); 
                    playSfx.audioClipName = splitString [1];           
                    break;
                case ("$"):
                    if (myString.Contains("renpy."))
                    {   
                        if (myString.Contains("renpy.pause"))
                        {
                            string[] furtherSplit = splitString [1].Replace("renpy.", "").Split(new char[]{'('}, StringSplitOptions.None);
                            
                            pause = ScriptableObject.CreateInstance(typeof(SC_Pause)) as SC_Pause;
                            initCommandAndAddToSection(currentSection, pause, sequencerData);
                            string[] againSplit = furtherSplit [1].Split(new char[]{')'}, StringSplitOptions.None);
    
                            pause.time = float.Parse(againSplit [0]);
                            break;
                        } else if (myString.Contains("renpy.input"))
                        {
                            SC_InputVariable input = ScriptableObject.CreateInstance(typeof(SC_InputVariable)) as SC_InputVariable; 
                            initCommandAndAddToSection(currentSection, input, sequencerData);
                            
                            input.variableName = splitString [1];
                            break;
                        }

                        commandWasValid = false;
                        string failLine = string.Join(" ", splitString);
                        
                        //only print out lines that are not empty
                        if (failLine.Length > 0 && failLine != " ")
                            Debug.Log("Line discarded: " + failLine);
                    } else
                    {
                        //check for variables 
                        SC_SetVariable variable = ScriptableObject.CreateInstance(typeof(SC_SetVariable)) as SC_SetVariable; 
                        initCommandAndAddToSection(currentSection, variable, sequencerData);
                        
                        variable.variableName = splitString [1];
                        variable.variableValue = string.Join(" ", splitString, 2, splitString.Length - 2);
                        //TODO: does this make sense ? if > or < or ==  bool, if / then float, else + )  
                        if (!variable.variableValue.Contains(".") && (variable.variableValue.Contains(">") || variable.variableValue.Contains("<") || variable.variableValue.Contains("==")
                            || variable.variableValue.Contains("+") || variable.variableValue.Contains("-") || variable.variableValue.Contains("*")))
                        {
                            variable.typeIndex = 1;
                        } else if (variable.variableValue.Contains(".") || variable.variableValue.Contains("/"))
                        {
                            variable.typeIndex = 2;
                        } else
                            variable.typeIndex = 0;

                        //attempt to fix assignments 
                        if ( variable.variableValue.Contains("+=") || variable.variableValue.Contains("-=") || variable.variableValue.Contains("*=") || variable.variableValue.Contains("/=") || (variable.variableValue.Contains("=") && !variable.variableValue.Contains("==") ))
                        {
                            if( variable.variableValue[0] == '+' || variable.variableValue[0] == '-' || variable.variableValue[0] == '*' || variable.variableValue[0] == '/' || variable.variableValue[0] == '=')
                            {
                                variable.variableValue = "[" + variable.variableName + "] " + variable.variableValue.Replace( "=" , " ") ;
                            }
                        }
                    }   
                    break;
                case ( "if"):
                    //TODO deal with "if" / "else" variable case 
                    //just going to drop a expression jump here... 
                    SC_VN_ExpressionJump expJump = ScriptableObject.CreateInstance(typeof(SC_VN_ExpressionJump)) as SC_VN_ExpressionJump; 
                    initCommandAndAddToSection(currentSection, expJump, sequencerData);

                    expJump.size = 1;
                    expJump.expressionList = new List<string>();
                    expJump.expressionList.Add(string.Join(" ", splitString, 1, splitString.Length - 1));
                    break;
                default:                    
                    //Add jumps at end of choices  
                    if (splitString.Length > 0 && splitString [splitString.Length - 1].Contains("\":"))
                    {
                        // 1 because we skip the first choice
                        for (int f=1; f < choiceCommandList [choiceCommandList.Count - 1].commandIndexList.Count; f++)
                        {
                            if (i == choiceCommandList [choiceCommandList.Count - 1].commandIndexList [f])
                            {
                                jump = ScriptableObject.CreateInstance(typeof(SC_Jump)) as SC_Jump;
                                initCommandAndAddToSection(currentSection, jump, sequencerData);
                                
                                jump.targetSectionName = currentSection.name;
                                
                                choiceToJumpsDictionary [choiceCommandList [choiceCommandList.Count - 1]].Add(jump);
                            }
                        }   

                        commandWasValid = false; // this is valid, but exception so the bottom thing works out
                        break;  
                    }
                    //end add jump at end of choices

                    //deal with characters dialogs
                    //target "dialog"
                    //"dialog"
                    if (splitString.Length == 1 && firstWord [0] == '"')
                    {
                        SC_VN_Dialog dialogNar = ScriptableObject.CreateInstance(typeof(SC_VN_Dialog)) as SC_VN_Dialog;
                        initCommandAndAddToSection(currentSection, dialogNar, sequencerData);

                        SequencerTargetModel narModel = checkTargetExistsOrCreate("narrator", sequencerData);
                        narModel.type = SequencerTargetTypes.character;
                        dialogNar.speakerTargetName = narModel.nickname;
                        dialogNar.text = myString.Substring(1, myString.Length - 2);
                        commandWasValid = true;
                        break;
                    } else if (splitString.Length > 1 && splitString [1] [0] == '"')
                    {
                        SC_VN_Dialog dialog = ScriptableObject.CreateInstance(typeof(SC_VN_Dialog)) as SC_VN_Dialog;
                        initCommandAndAddToSection(currentSection, dialog, sequencerData);

                        if (firstWord == " " || firstWord.Length < 1)
                            break;

                        SequencerTargetModel diagModel = checkTargetExistsOrCreate(firstWord, sequencerData);
                        diagModel.type = SequencerTargetTypes.character;
                        dialog.speakerTargetName = diagModel.nickname;
                        int startIndex = myString.IndexOf("\"") + 1;
                        int endIndex = myString.LastIndexOf("\"");
                        dialog.text = myString.Substring(startIndex, endIndex - startIndex);
                        commandWasValid = true;
                        break;
                    }
                    //end dialog
                    
                    //just skip the line discarded output for these, cause they are actually used!
                    if (splitString.Length > 0 && splitString [splitString.Length - 1].Contains("\":"))
                        break;
                    
                    //not found 
                    commandWasValid = false;

                    string failLine = string.Join(" ", splitString);

                    //only print out lines that are not empty
                    if (failLine.Length > 0 && failLine != " ")
                        Debug.Log("Line discarded: " + failLine);
                    break;
            }

            //every time we have made a valid command after a choice line, update the jump index for that choice 
            if (commandWasValid && choiceCommandList.Count > 0)
            {
                int currChoiceIndexToResolve = choiceToCurrentResolvedIndex [choiceCommandList [choiceCommandList.Count - 1]];

                //next valid command 
                if (choiceCommandList [choiceCommandList.Count - 1].commandIndexList.Count > currChoiceIndexToResolve && i > choiceCommandList [choiceCommandList.Count - 1].commandIndexList [currChoiceIndexToResolve])
                {
                    choiceCommandList [choiceCommandList.Count - 1].commandIndexList [currChoiceIndexToResolve] = currentSection.commandList.Count - 1;
                    choiceCommandList [choiceCommandList.Count - 1].targetCommandList [currChoiceIndexToResolve] = currentSection.commandList [currentSection.commandList.Count - 1];
                    choiceToCurrentResolvedIndex [choiceCommandList [choiceCommandList.Count - 1]] += 1;
                }    
            }

            //if we are at the end line for a choice, then complete the jump for the end of that choice
            if (choiceCommandList.Count > 0 && i == choiceEndLineList [choiceEndLineList.Count - 1])
            {
                foreach (SC_Jump jumpCmd in choiceToJumpsDictionary[choiceCommandList [choiceCommandList.Count - 1]])
                {
                    jumpCmd.commandIndex = currentSection.commandList.Count - 1;
                    jumpCmd.targetCommand = currentSection.commandList [currentSection.commandList.Count - 1];
                }
                choiceEndLineList.RemoveAt(choiceEndLineList.Count - 1);
                choiceCommandList.RemoveAt(choiceCommandList.Count - 1);
            }
        }
    }

    SequencerSectionModel checkSectionExistsOrCreate(string name, SequencerData data)
    {
        SequencerSectionModel sectionModel;  
        if (data.sections.Count == 0)
        {
            sectionModel = new SequencerSectionModel();
            sectionModel.name = name;
            sectionModel.commandList = new List<SequencerCommandBase>();
            data.sections.Add(sectionModel);  
        } else
        {
            sectionModel = data.getSectionModel(name);
            if (sectionModel.name != name)
            {
                sectionModel = new SequencerSectionModel();
                sectionModel.name = name;
                sectionModel.commandList = new List<SequencerCommandBase>();
                data.sections.Add(sectionModel);  
            }
        }          
        return sectionModel;
    }
    
    SequencerTargetModel checkTargetExistsOrCreate(string name, SequencerData data)
    {
        SequencerTargetModel targetModel;
        if (data.targets.Count == 0)
        {
            targetModel = new SequencerTargetModel();
            targetModel.nickname = name;
            data.targets.Add(targetModel); 
        } else
        {
            targetModel = data.getTargetModel(name);
            if (targetModel.nickname != name)
            {
                targetModel = new SequencerTargetModel();
                targetModel.nickname = name;    
                data.targets.Add(targetModel); 
            }
        }
        return targetModel;
    }

    void initCommandAndAddToSection(SequencerSectionModel sectionModel, SequencerCommandBase command, SequencerData data)
    {
        if ( sectionModel == null)
        {
            Debug.LogWarning( "No Section to add command to ! ");
            return;
        }

        command.init(sectionModel.name, data);
        sectionModel.commandList.Add(command);
        command.updateAllIndex(); 
    }
}
