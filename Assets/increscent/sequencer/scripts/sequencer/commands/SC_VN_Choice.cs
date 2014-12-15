using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
 #endif
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 2D : Visual Novel: Choice : Sequencer Command
/// Shows several options each option jumps you to another section
/// </summary>
using System.Linq;

[Serializable]
public class SC_VN_Choice : SequencerCommandBase
{
    public override string commandId{ get { return "choice"; } }

    public override string commandType{ get { return "base"; } }

    public int size = 0;
    public List<string> sectionNameList;
    public List<string> optionTextList;
    public List<int> commandIndexList;
   
    [HideInInspector]
    public List<SequencerCommandBase>
        targetCommandList;

    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Choice newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Choice)) as SC_VN_Choice;
        newCmd.size = size;
        newCmd.sectionNameList = new List<string>(sectionNameList.ToArray());
        newCmd.optionTextList = new List<string>(optionTextList.ToArray());
        newCmd.commandIndexList = new List<int>(commandIndexList.ToArray());
        newCmd.targetCommandList = new List<SequencerCommandBase>(targetCommandList.ToArray());
        return base.clone(newCmd);        
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        showChoices();
        myPlayer.inRewindMode = false;
    }

    private void showChoices()
    {
        List<ChoiceModel> choices = new List<ChoiceModel>();
        for (int i = 0; i < sectionNameList.Count; i++)
        {
            ChoiceModel model = new ChoiceModel();
            model.text = optionTextList [i];
            model.sceneNameToJump = sectionNameList [i];
            model.sceneCommandIndexToJump = commandIndexList [i];
            choices.Add(model);
        }
        
        myPlayer.choiceController.generateButtons(choices, myPlayer);
    }
    
    override public void undo()
    {
        myPlayer.choiceController.cleanup();
    }

    override public void forward(SequencePlayer player)
    {
        //nothing, it blocks
        myPlayer.blockForward = true;
    }
    
    override public void backward(SequencePlayer player)
    {
        undo();
    }
   
    #if UNITY_EDITOR 
    override public void drawCustomUi()
    { 
        EditorGUILayout.LabelField("number of options:");
        size = EditorGUILayout.IntField(size);
            
        if (size > 0)
        {
            makeSureListsAreCorrectSize(); 
           
            string[] nicks = sequencerData.getSectionNames();           

            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < size; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Option Text:");
                        optionTextList [i] = EditorGUILayout.TextField(optionTextList [i]);

                        GUILayout.Label("Jump to Section:");
                        sectionNameList [i] = nicks [EditorGUILayout.Popup(sequencerData.getIndexOfSection(sectionNameList [i]), nicks, GUILayout.Width(100))];

                        int commandIndexMax = sequencerData.sections [sequencerData.getIndexOfSection(sectionNameList [i])].commandList.Count;
                        if (commandIndexMax < 46) //max show for in popup style, otherwise use input field
                        {
                            int[] commands = Enumerable.Repeat(0, commandIndexMax).ToArray();
                            string[] commandStr = new string[commands.Length];
                            for (int c = 0; c < commands.Length; c++)
                            {
                                commandStr [c] = c.ToString();
                            }   
                            commandIndexList [i] = Mathf.Clamp(commandIndexList [i], 0, commands.Length - 1);

                            GUILayout.Label("Jump to command Index:");
                            commandIndexList [i] = EditorGUILayout.Popup(commandIndexList [i], commandStr);
                        } else
                        {
                            commandIndexList [i] = Mathf.Clamp(EditorGUILayout.IntField(commandIndexList [i]), 0, commandIndexMax - 1);
                        }
                        
                        targetCommandList [i] = sequencerData.getSectionModel(sectionNameList [i]).commandList [commandIndexList [i]];
                    }
                    EditorGUILayout.EndHorizontal();  
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
    #endif

    void makeSureListsAreCorrectSize()
    {
        if (sectionNameList == null)
            sectionNameList = new List<string>(size); 

        if (optionTextList == null)
            optionTextList = new List<string>(size);

        if (commandIndexList == null)
            commandIndexList = new List<int>(size);

        if (targetCommandList == null)
            targetCommandList = new List<SequencerCommandBase>(size);

        if (sectionNameList.Count < size)
            sectionNameList.AddRange(Enumerable.Repeat("", size - sectionNameList.Count).ToList());

        if (commandIndexList.Count < size)
            commandIndexList.AddRange(Enumerable.Repeat(0, size - commandIndexList.Count).ToList());

        if (targetCommandList.Count < size)
        {
            int amount = size - targetCommandList.Count;
            for (int i=0; i < amount; i++)
            {
                targetCommandList.Add(null);
            }
        }
        
        if (optionTextList.Count < size)
            optionTextList.AddRange(Enumerable.Repeat("option", size - optionTextList.Count).ToList());

        commandIndexList = commandIndexList.GetRange(0, size);
        sectionNameList = sectionNameList.GetRange(0, size);
        optionTextList = optionTextList.GetRange(0, size);
        targetCommandList = targetCommandList.GetRange(0, size);
    }

    override public void indexWasUpdated()
    {
        checkIndexCorrectness();
    }
    
    public void checkIndexCorrectness()
    {
        if (sectionNameList == null)
            return;

        for (int i = 0; i < sectionNameList.Count; i++)
        {
            SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionNameList [i]);
            if (commandIndexList [i] < sectionModel.commandList.Count && sectionModel.commandList [commandIndexList [i]] != targetCommandList [i])
            {
                bool found = false;
                foreach (SequencerCommandBase cmd in sectionModel.commandList)
                {
                    if (cmd == targetCommandList [i])
                    {
                        //Debug.Log(i + " = " + commandIndexList [i]);
                        commandIndexList [i] = sectionModel.commandList.IndexOf(cmd);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogWarning("Couldnt find jump target command after index change ! Will use whatever is at previous index!" + 
                        "Was looking for index:" + commandIndexList [i] + " in section: " + sectionModel.name + " is Choice command at index: " + 
                        sequencerData.getSectionModel(sectionName).commandList.IndexOf(this));
                    targetCommandList [i] = sectionModel.commandList [commandIndexList [i]];
                }
            }
        }
    }

    override public string toRenpy()
    {
        //TODO: temp code, need to have some time to implement the branching for choices =( 
        //This is clearly not putting the lines that should go inbetween 
        //target output:    menu:
        //                      "I agree...":
        string choiceString = "menu:\n";
        foreach (string choice in optionTextList)
            choiceString += "\t\t\"" + choice + "\":\n";
        return  choiceString;
    }

    override public string toSequncerSerializedString()
    {
        string choices = "";
        for (int i=0; i < size; i++)
        {
            choices += sectionNameList [i] + "╫" + optionTextList [i] + "╫" + commandIndexList [i] + "╫";
        }

        return GetType().Name + "╫" + choices + "╫\n";
    }

    override public void initFromSequncerSerializedString(string[] splitString)
    {
        sectionNameList = new List<string>();
        optionTextList = new List<string>();
        commandIndexList = new List<int>();

        for (int i=1; i+2 < splitString.Length; i+=3)
        {
            sectionNameList.Add(splitString [i]);
            optionTextList.Add(splitString [i + 1]);
            commandIndexList.Add(int.Parse(splitString [i + 2]));
        }
    }

    override public bool updateSectionReference(string oldSection, string newSection)
    {
        bool wasChanged = false;

        for (int i = 0; i < sectionNameList.Count; i++)
        {
            if (sectionNameList [i] == oldSection)
            {
                sectionNameList [i] = newSection;
                wasChanged = true;
            }
        }

        if (wasChanged)
            return true;

        return false;
    }
}