using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System;
using System.Reflection;

[Serializable]
public class SequencerCommandBase : ScriptableObject
{
    public int sectionIndex = -1;
    private int listIndex = -1;
    private int currIndex = -1;
    private int targetIndex = -1;
    protected SequencePlayer myPlayer;

    public void init(int in_sectionIndex)
    {
        sectionIndex = in_sectionIndex;
        name = this.GetType().Name;

        initChild();
    }

    public virtual void initChild()
    {
    }

    public virtual void execute(SequencePlayer player)
    {
    }

    public virtual void undo()
    {
    }

    public virtual void forward(SequencePlayer player)
    {
    }

    public virtual void backward(SequencePlayer player)
    {
    }

    #if UNITY_EDITOR
    public virtual void drawCustomUi()
    {
    }

    public void drawFrontUi()
    {    
        listIndex = findIndexOf(this);
        if (currIndex == -1)
        {
            currIndex = listIndex;
        }
    
        GUILayout.Label(listIndex.ToString(), GUILayout.Width(40));

        if (GUILayout.Button("Change Position ->", GUILayout.Width(100)))
            currIndex = targetIndex;

        targetIndex = EditorGUILayout.IntField(targetIndex, GUILayout.Width(40));
        
        if (currIndex != listIndex)
        {
            changeIndex(currIndex, listIndex);
        }

        GUILayout.Label(name, GUILayout.Width(125));
            
        EditorGUILayout.BeginVertical();
        {
            if (GUILayout.Button("^", GUILayout.Width(30)))
                doReorder(-1);

            if (GUILayout.Button("v", GUILayout.Width(30)))
                doReorder(1);
        }
        EditorGUILayout.EndVertical();
    }

    public void drawBackUi()
    {        
        if (GUILayout.Button("Delete this Command", GUILayout.Width(150)))
            deleteThisCommand();
    }
#endif

    public void changeIndex(int newPos, int oldPos)
    {
        if (newPos > -1)
        {
            newPos = Mathf.Clamp(newPos, 0, ((SequencerData)((SequencerData)SequencerData.get())).sections [sectionIndex].commandList.Count - 1);
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Remove(this);
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Insert(newPos, this);
        
            updateAllIndex();
        }
    }

    public void doReorder(int direction)
    {
        int oldIndex = findIndexOf(this);
        int newIndex = Mathf.Clamp(oldIndex + direction, 0, ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Count - 1);
        
        if (newIndex != oldIndex)
        {
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Remove(this);
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Insert(newIndex, this);
        
            updateAllIndex();
        }
    }

    public void deleteThisCommand()
    {
        foreach (SequencerCommandBase command in ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList)
        {
            if (command.GetInstanceID() == this.GetInstanceID())
            {
                ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Remove(command);
                DestroyImmediate(command);

                updateAllIndex();
                break;         
            }
        }
    }

    public int findIndexOf(SequencerCommandBase command)
    {
        for (int i = 0; i < ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Count; i++)
        {
            if (((SequencerData)SequencerData.get()).sections [sectionIndex].commandList [i].GetInstanceID() == command.GetInstanceID())
            {
                return i;
            }
        }
        return -1;
    }

    public void updateAllIndex()
    {
        for (int i = 0; i < ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList.Count; i++)
        {
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList [i].listIndex = i;
            ((SequencerData)SequencerData.get()).sections [sectionIndex].commandList [i].currIndex = i;
        } 
    }
}
