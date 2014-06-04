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
    public List<int> sectionIndexList;
    public List<string> optionTextList;
    public List<int> commandIndexList;
   
    override public void initChild()
    {
    }

    override public SequencerCommandBase clone()
    {       
        SC_VN_Choice newCmd = ScriptableObject.CreateInstance(typeof(SC_VN_Choice)) as SC_VN_Choice;
        newCmd.size = size;
        newCmd.sectionIndexList = new List<int>(sectionIndexList.ToArray());
        newCmd.optionTextList = new List<string>(optionTextList.ToArray());
        newCmd.commandIndexList = new List<int>(commandIndexList.ToArray());
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
        string[] nicks = sequencerData.getSectionNames();   
        List<ChoiceModel> choices = new List<ChoiceModel>();
        for (int i = 0; i < sectionIndexList.Count; i++)
        {
            ChoiceModel model = new ChoiceModel();
            model.text = optionTextList [i];
            model.sceneNameToJump = nicks [sectionIndexList [i]];
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
                        sectionIndexList [i] = EditorGUILayout.Popup(sectionIndexList [i], nicks, GUILayout.Width(100));

                        int[] commands = Enumerable.Repeat(0, sequencerData.sections [sectionIndexList [i]].commandList.Count).ToArray();
                        string[] commandStr = new string[commands.Length];
                        for (int c = 0; c < commands.Length; c++)
                        {
                            commandStr [c] = c.ToString();
                        }   
                        GUILayout.Label("Jump to command Index:");
                        commandIndexList [i] = EditorGUILayout.Popup(commandIndexList [i], commandStr);
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
        if (sectionIndexList == null)
            sectionIndexList = new List<int>(size); 

        if (optionTextList == null)
            optionTextList = new List<string>(size);

        if (commandIndexList == null)
            commandIndexList = new List<int>(size);

        if (sectionIndexList.Count < size)
            sectionIndexList.AddRange(Enumerable.Repeat(0, size - sectionIndexList.Count).ToList());

        if (commandIndexList.Count < size)
            commandIndexList.AddRange(Enumerable.Repeat(0, size - commandIndexList.Count).ToList());
        
        if (optionTextList.Count < size)
            optionTextList.AddRange(Enumerable.Repeat("option", size - optionTextList.Count).ToList());

        commandIndexList = commandIndexList.GetRange(0, size);
        sectionIndexList = sectionIndexList.GetRange(0, size);
        optionTextList = optionTextList.GetRange(0, size);
    }
}