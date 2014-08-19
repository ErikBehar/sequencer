﻿using UnityEngine;

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

    public string sectionName = "";
    public int listIndex = -1;
    public int currIndex = -1;
    public int targetIndex = -1;

    public SequencerData sequencerData;

    //this is temporary 
    public SequencePlayer myPlayer;

    public void init(string in_sectionName, SequencerData data)
    {
        sectionName = in_sectionName;
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

    public virtual SequencerCommandBase clone()
    {
        return new SequencerCommandBase();
    }

    public SequencerCommandBase clone(SequencerCommandBase cmd)
    {       
        cmd.sectionName = sectionName;
        cmd.listIndex = listIndex;
        cmd.currIndex = currIndex;
        cmd.targetIndex = targetIndex;
        cmd.sequencerData = sequencerData;
        return cmd;
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
            SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

            newPos = Mathf.Clamp(newPos, 0, sectionModel.commandList.Count - 1);
            sectionModel.commandList.Remove(this);
            sectionModel.commandList.Insert(newPos, this);
        
            updateAllIndex();
        }
    }

    public void doReorder(int direction)
    {
        SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

        int oldIndex = findIndexOf(this);
        int newIndex = Mathf.Clamp(oldIndex + direction, 0, sectionModel.commandList.Count - 1);
        
        if (newIndex != oldIndex)
        {
            sectionModel.commandList.Remove(this);
            sectionModel.commandList.Insert(newIndex, this);
        
            updateAllIndex();
        }
    }

    public void deleteThisCommand()
    {
        SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

        foreach (SequencerCommandBase command in sectionModel.commandList)
        {
            if (command.GetInstanceID() == this.GetInstanceID())
            {
                sectionModel.commandList.Remove(command);
                DestroyImmediate(command);

                updateAllIndex();
                break;         
            }
        }
    }

    public int findIndexOf(SequencerCommandBase command)
    {
        SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

        for (int i = 0; i < sectionModel.commandList.Count; i++)
        {
            if (sectionModel.commandList [i].GetInstanceID() == command.GetInstanceID())
            {
                return i;
            }
        }
        return -1;
    }

    public void updateAllIndex()
    {
        SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

        for (int i = 0; i < sectionModel.commandList.Count; i++)
        {
            sectionModel.commandList [i].listIndex = i;
            sectionModel.commandList [i].currIndex = i;
            sectionModel.commandList [i].indexWasUpdated();
        } 
    }

    public virtual void indexWasUpdated()
    {
    }

    public virtual string toRenpy()
    {
        return "";
    }

    public virtual string toSequncerSerializedString()
    {
        return "";
    }

    public virtual void initFromSequncerSerializedString(string[] splitString)
    {
    }

    public virtual bool updateTargetReference(string oldNickname, string newNickName)
    {
        return false;
    }

    public virtual bool updateSectionReference(string oldSection, string newSection)
    {
        return false;
    }

    public virtual bool updateVariableReference(string oldVariable, string newVariable)
    {
        return false;
    }
}