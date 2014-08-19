using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System;

/// <summary>
/// Example : Sequencer Command
/// This is here just for reference and helps make your own command
/// </summary>

[Serializable]
public class SC_Example : SequencerCommandBase
{
    //this id is used by the editor window, the name here should be a unique id, this way if the user adds or changes the order of commands
    //we wont loose data or have things pointing at the wrong index, usually pretty close to class name, but we could change class name
    // and hopefully the id stays same, and so we dont loose data, hopefully.
    public override string commandId{ get { return "example"; } }

    //this type is used by the editor window for sub-filtering, which is still TODO     
    public override string commandType{ get { return "base"; } }
    
    //mostly unused now, but initChild can be used to init this command when its created in the editor window
    override public void initChild()
    {
    }

    //make a copy of this thats not a reference ( make sure you change the indexes after you clone, lest you run into duplicates)
    override public SequencerCommandBase clone()
    {       
        SC_Example newCmd = ScriptableObject.CreateInstance(typeof(SC_Example)) as SC_Example;
        return base.clone(newCmd);        
    }
    
    //Execute runs when the player.play() method is called, basically when we've switched into this command
    override public void execute(SequencePlayer player)
    {
        //cache the player, pretty important
        myPlayer = player;
        
        //if executed this command while rewinding then do something 
        if (myPlayer.inRewindMode)
        {
            //usually undo whatever the command did
            undo();
        } else
        {
            //do whatever the command does here 
        }

        //we are done with the command, let the player know, if we dont do this then we are stuck waiting here
        myPlayer.callBackFromCommand(); 
    }
    
    //usually called to undo whatever the command did
    override public void undo()
    {
    }
    
    //called right before doing the next command
    override public void forward(SequencePlayer player)
    {
    }
    
    //gets executed when the player recieves a input back command, usually calls undo.
    override public void backward(SequencePlayer player)
    {
    }
    
    #if UNITY_EDITOR
    //this is an example custom ui for this command, this gets drawn when the editor wants to draw one of these commands
    //allows you to specify details of this command
    override public void drawCustomUi()
    { 
        GUILayout.Label("Example:");
    }
    #endif

    //turn your command to one or more similar lines of ren'py code, return "" or dont even include this function if there is no ren'py similar command
    override public string toRenpy()
    {
        return "";
    }

    //turn your command to something we can save to a txt file to restore later, see examples in other commands 
    override public string toSequncerSerializedString()
    {
        return "";
    }

    //use the split string you save out above to try to restore this command back
    override public void initFromSequncerSerializedString(string[] splitString)
    {
    }

    //if this command is referencing a specific defined target, and that target changes name or is swapped, this will be called
    //you must implement this to update the command accordingly if it needs it.
    override public bool updateTargetReference(string oldNickname, string newNickName)
    {
        return false;
    }

    //if this commmand references a specific section, and that section changes, then you must implement this to swap out with the new section
    override public bool updateSectionReference(string oldSection, string newSection)
    {
        return false;
    }

    //if this command uses defined variables, and that variable changes, then you must implement this to swap out with the new variable 
    override public bool updateVariableReference(string oldVariable, string newVariable)
    {
        return false;
    }
}