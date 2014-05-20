using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

public class NguiDialogBubble : MonoBehaviour
{
    public GameObject speechBubbleLeft;
    public GameObject speechBubbleRight;
    public GameObject narratorBubble;

    public UILabel narratorTextLabel;
    public UILabel speechTextLabelLeft;
    public UILabel speechTextLabelRight;

    public Transform centerPos;
    public Transform leftPos;
    public Transform rightPos;

    public int textLineSize = 30;

    void Awake()
    {
        hideDialog();
    }

    public void showDialog(string text, GameObject charspeaker)
    {
        text = chopTextIntoPieces(textLineSize, text);

        if (charspeaker.transform.localPosition.x > 1)
        {
            speechTextLabelRight.text = text;
            speechBubbleRight.transform.localPosition = rightPos.localPosition;
            speechBubbleRight.SetActive(true);
        } else if (charspeaker.transform.localPosition.x < -1)
        { 
            speechTextLabelLeft.text = text;
            speechBubbleLeft.transform.localPosition = leftPos.localPosition;
            speechBubbleLeft.SetActive(true);
        } else
        {
            narratorTextLabel.text = text;
            narratorBubble.transform.localPosition = centerPos.localPosition;
            narratorBubble.SetActive(true);
        }
    }

    public string chopTextIntoPieces(int size, string originalText)
    {
        string[] temp = originalText.Split(new char[]{' '});

        int newSize = 0;
        string finalString = "";
        foreach (string piece in temp)
        {
            newSize += piece.Length;
            if (newSize > size)
            {
                finalString += Environment.NewLine + piece;
                newSize = 0;
            } else
                finalString += " " + piece;
        }

        return finalString;
    }

    public void hideDialog()
    {
        speechBubbleRight.SetActive(false);
        speechBubbleLeft.SetActive(false);
        narratorBubble.SetActive(false);
    }
}
