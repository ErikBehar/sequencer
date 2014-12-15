using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UGuiInputController : InputControllerBase
{
    public GameObject ed_window;
    public Button ed_submit;
    public InputField ed_inputField;

    private SequencePlayer myPlayer;
    private string variableNameToUse;
    
    void Start()
    {
        ed_submit.onClick.AddListener(onSubmit);

        ed_window.SetActive(false);
    }

    void onSubmit()
    {
        //validate
        if (ed_inputField.text.Length == 0 || ed_inputField.text == " ")
        {
            //TODO make this better? LOL
            return;
        }

        //set variable
        if (myPlayer.runningTimeVariablesDictionary.ContainsKey(variableNameToUse))
            myPlayer.runningTimeVariablesDictionary [variableNameToUse] = ed_inputField.text;
        else
            myPlayer.runningTimeVariablesDictionary.Add(variableNameToUse, ed_inputField.text);
            
        //hide
        hideInput();

        //continue ( force is needed cause this is blocking command)
        myPlayer.forceForward();
    }

    override public void showInputFor(string variableName, SequencePlayer player)
    {
        myPlayer = player;
        variableNameToUse = variableName;
        ed_inputField.text = "";
        ed_window.SetActive(true);
    }
    
    override public void hideInput()
    {
        ed_inputField.text = "";
        ed_window.SetActive(false);   
    }
}