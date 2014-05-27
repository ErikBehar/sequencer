using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NguiChoiceButtons : MonoBehaviour
{
    public GameObject exampleButton;

    private List<ChoiceModel> currChoices;

    private List<GameObject> buttons = new List<GameObject>();

    private SequencePlayer currentPlayer;

    void Start()
    {
        exampleButton.SetActive(false);
    }

    public void generateButtons(List<ChoiceModel> choices, SequencePlayer player)
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
            button.transform.FindChild("Label").GetComponent<UILabel>().text = choices [i].text;
            EventDelegate.Add(button.GetComponent<UIButton>().onClick, onButtonClick);  
            button.transform.parent = exampleButton.transform.parent;
            button.transform.localPosition = new Vector3(0, (Screen.height / 2) + (-100 * (i + 1)), 0);
            button.transform.localScale = Vector3.one;
            button.SetActive(true);
            buttons.Add(button);
        } 
    }

    void onButtonClick()
    {
        foreach (ChoiceModel model in currChoices)
        {
            if (model.text == UIButton.current.gameObject.transform.FindChild("Label").GetComponent<UILabel>().text)
            {
                cleanup();
                currentPlayer.jumpToScene(model.sceneNameToJump, model.sceneCommandIndexToJump);
                break;
            }
        }
    }

    public void cleanup()
    {
        foreach (GameObject button in buttons)
        {
            EventDelegate.Remove(button.GetComponent<UIButton>().onClick, onButtonClick);
            Destroy(button);
        }

        buttons = new List<GameObject>(); 
    }
}