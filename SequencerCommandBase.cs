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
    public virtual string commandId { get { return "base"; } }
    public virtual string commandType { get { return "base-abstract"; } }

    public int sectionIndex = -1;
    private int listIndex = -1;
    private int currIndex = -1;
    private int targetIndex = -1;
    protected SequencePlayer myPlayer;

    public SequencerData sequencerData;

    public void init(int in_sectionIndex, SequencerData data)
    {
        sectionIndex = in_sectionIndex;
        sequencerData = data;
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
            newPos = Mathf.Clamp(newPos, 0, sequencerData.sections [sectionIndex].commandList.Count - 1);
            sequencerData.sections [sectionIndex].commandList.Remove(this);
            sequencerData.sections [sectionIndex].commandList.Insert(newPos, this);
        
            updateAllIndex();
        }
    }

    public void doReorder(int direction)
    {
        int oldIndex = findIndexOf(this);
        int newIndex = Mathf.Clamp(oldIndex + direction, 0, sequencerData.sections [sectionIndex].commandList.Count - 1);
        
        if (newIndex != oldIndex)
        {
            sequencerData.sections [sectionIndex].commandList.Remove(this);
            sequencerData.sections [sectionIndex].commandList.Insert(newIndex, this);
        
            updateAllIndex();
        }
    }

    public void deleteThisCommand()
    {
        foreach (SequencerCommandBase command in sequencerData.sections [sectionIndex].commandList)
        {
            if (command.GetInstanceID() == this.GetInstanceID())
            {
                sequencerData.sections [sectionIndex].commandList.Remove(command);
                DestroyImmediate(command);

                updateAllIndex();
                break;         
            }
        }
    }

    public int findIndexOf(SequencerCommandBase command)
    {
        for (int i = 0; i < sequencerData.sections [sectionIndex].commandList.Count; i++)
        {
            if (sequencerData.sections [sectionIndex].commandList [i].GetInstanceID() == command.GetInstanceID())
            {
                return i;
            }
        }
        return -1;
    }

    public void updateAllIndex()
    {
        for (int i = 0; i < sequencerData.sections [sectionIndex].commandList.Count; i++)
        {
            sequencerData.sections [sectionIndex].commandList [i].listIndex = i;
            sequencerData.sections [sectionIndex].commandList [i].currIndex = i;
        } 
    }
}
