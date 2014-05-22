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
    public int size = 0;
    public List<int> sectionIndexList;
    public List<string> optionTextList;
   
    override public void initChild()
    {
    }

    override public void execute(SequencePlayer player)
    {
        myPlayer = player;
        
        if (player.inRewindMode)
        {
            undo();
        } else
        {
            //TODO
        }
        
        myPlayer.inRewindMode = false;
        myPlayer.callBackFromCommand(true); 
    }
    
    override public void undo()
    {
    } 

    override public void forward(SequencePlayer player)
    {
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
           
            string[] nicks = ((SequencerData)SequencerData.get()).getSectionNames();            

            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < size; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Option Text:");
                        EditorGUILayout.TextField(optionTextList [i]);

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