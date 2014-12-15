using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UGuiChoiceButtons : ChoiceControllerBase
{
    public GameObject exampleButton;

    private List<ChoiceModel> currChoices;

    private List<GameObject> buttons = new List<GameObject>();

    private SequencePlayer currentPlayer;

    void Start()
    {
        exampleButton.SetActive(false);
    }

    override public void generateButtons(List<ChoiceModel> choices, SequencePlayer player)
    {
        currentPlayer = player;

        if (buttons.Count > 0)
        {
            cleanup();
        }

        currChoices = choices;

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject button = Instantiate(exampleButton) as GameObject;
            button.transform.FindChild("Text").GetComponent<Text>().text = choices [i].text;

            //note this value must be captured outside of the addlistener below
            //for some strange esoteric reason Im still figuring out
            string tempTextVal = choices [i].text; // "captured" value

            button.GetComponent<Button>().onClick.AddListener(() => onButtonClick(tempTextVal));

            button.transform.parent = exampleButton.transform.parent;
            button.transform.localPosition = new Vector3(0, (Screen.height / 2) + (-100 * (i + 1)), 0);
            button.transform.localScale = Vector3.one;
            button.SetActive(true);
            buttons.Add(button);
        } 
    }

    void onButtonClick(string text)
    {
        foreach (ChoiceModel model in currChoices)
        {
            if (model.text == text)
            {
                cleanup();
                currentPlayer.jumpToScene(model.sceneNameToJump, model.sceneCommandIndexToJump);
                break;
            }
        }
    }

    override public void cleanup()
    {
        foreach (GameObject button in buttons)
        {
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(button);
        }

        buttons.Clear();
    }
}