using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor.SceneManagement;
    using UnityEditor;
#endif
using System;

[Serializable]
public class SequencerCommandBase : ScriptableObject
{
    public virtual string commandId { get { return "base"; } }
    public virtual string commandType { get { return "base-abstract"; } }

    public string sectionName = "";
    public int listIndex = -1;
    public int currIndex = -1;
    public int targetIndex = -1;

    public bool isEnabled = true;

    public SequencerData sequencerData;

    //this is temporary 
    public SequencePlayer myPlayer;

    protected Texture icon;

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
        cmd.isEnabled = isEnabled;
        return cmd;
    }

#if UNITY_EDITOR
    public virtual void drawCustomUi()
    {
    }

    public virtual void drawMinimizedUi()
    {
        GUILayout.Button( sequencerData.getIconTexture("default"), GUILayout.Width(32));
    }

    public void drawFrontUi()
    {    
        listIndex = findIndexOf(this);
        if (currIndex == -1)
        {
            currIndex = listIndex;
        }
    
        GUILayout.Label(listIndex.ToString(), GUILayout.Width(40));

        drawMinimizedUi();

        Color defaultColor = GUI.contentColor;
        GUI.contentColor = Color.black;
        GUILayout.Label(name, GUILayout.Width(125));
        GUI.contentColor = defaultColor;

        isEnabled = GUILayout.Toggle(isEnabled, "Enabled");
            
        if (GUILayout.Button("Change Position ->", GUILayout.Width(100)))
            currIndex = targetIndex;

        targetIndex = EditorGUILayout.IntField(targetIndex, GUILayout.Width(40));
        
        if (currIndex != listIndex)
        {
            changeIndex(currIndex);
        }

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

    void makeSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(sequencerData.gameObject.scene);
    }
        
    public int changeIndex(int newPos)
    {
        if (newPos > -1)
        {
            SequencerSectionModel sectionModel = sequencerData.getSectionModel(sectionName);

            newPos = Mathf.Clamp(newPos, 0, sectionModel.commandList.Count - 1);

            //if you are removing below index of current target then value should stay the same (target moved one back, we occupy the index of previous target)
            //if you are removing above index of current target then value should be +1  (target in same spot, we occupy the index in front of it )
            int offset = 0;
            if (listIndex > newPos)
                offset = 1; 
            
            sectionModel.commandList.Remove(this);
            sectionModel.commandList.Insert( Mathf.Max( 0, newPos + offset), this);

            updateAllIndex();

            makeSceneDirty();

            return this.currIndex;
        }

        return -1;
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

            makeSceneDirty();
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

                makeSceneDirty();
                break;         
            }
        }
    }
#endif

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
