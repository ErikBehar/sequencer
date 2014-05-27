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
   
    override public void initChild()
    {
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
            model.sceneCommandIndexToJump = 0;
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

                        GUILayout.Label("Jump to:");
                        sectionIndexList [i] = EditorGUILayout.Popup(sectionIndexList [i], nicks, GUILayout.Width(100));
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

        if (sectionIndexList.Count < size)
            sectionIndexList.AddRange(Enumerable.Repeat(0, size - sectionIndexList.Count).ToList());
        
        if (optionTextList.Count < size)
            optionTextList.AddRange(Enumerable.Repeat("option", size - optionTextList.Count).ToList());

        sectionIndexList = sectionIndexList.GetRange(0, size);
        optionTextList = optionTextList.GetRange(0, size);
    }
}