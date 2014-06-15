using UnityEngine;
using System.Collections;

public class NguiInputController : InputControllerBase
{
    public GameObject ed_window;
    public UIButton ed_submit;
    public UIInput ed_inputField;

    private SequencePlayer myPlayer;
    private string variableNameToUse;
    
    void Start()
    {
        EventDelegate.Add(ed_submit.onClick, onSubmit);

        ed_window.SetActive(false);
    }

    void onSubmit()
    {
        //validate
        if (ed_inputField.label.text.Length == 0 || ed_inputField.label.text == " ")
        {
            //TODO make this better? LOL
            return;
        }

        //set variable
        if (myPlayer.runningTimeVariablesDictionary.ContainsKey(variableNameToUse))
            myPlayer.runningTimeVariablesDictionary [variableNameToUse] = ed_inputField.label.text;
        else
            myPlayer.runningTimeVariablesDictionary.Add(variableNameToUse, ed_inputField.label.text);
            
        //hide
        hideInput();

        //continue ( force is needed cause this is blocking command)
        myPlayer.forceForward();
    }

    override public void showInputFor(string variableName, SequencePlayer player)
    {
        myPlayer = player;
        variableNameToUse = variableName;
        ed_inputField.label.text = "";
        ed_window.SetActive(true);
    }
    
    override public void hideInput()
    {
        ed_inputField.label.text = "";
        ed_window.SetActive(false);   
    }
}