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

    public GameObject leftBubbleEdge;
    public float leftBubbleOriginalDifference;

    public GameObject rightBubbleEdge;
    public float rightBubbleOriginalDifference;

    private bool shown = false;

    void Awake()
    {
        hideDialog();

        leftBubbleOriginalDifference = (leftBubbleEdge.transform.position - speechBubbleLeft.transform.position).x;

        rightBubbleOriginalDifference = (rightBubbleEdge.transform.position - speechBubbleRight.transform.position).x;
    }

    public void showDialog(string text, GameObject charspeaker)
    {
        shown = true;
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
        shown = false;
        speechBubbleRight.SetActive(false);
        speechBubbleLeft.SetActive(false);
        narratorBubble.SetActive(false);
    }

    void FixedUpdate()
    {
        if (shown)
        {
            float newdifference = leftBubbleOriginalDifference - (leftBubbleEdge.transform.position - leftPos.gameObject.transform.position).x; 
            if (Mathf.Abs(newdifference) > .03f)
                speechBubbleLeft.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleLeft.gameObject.transform.position.x, speechBubbleLeft.gameObject.transform.position.x + newdifference, .25f),
                                                                          leftPos.gameObject.transform.position.y, leftPos.gameObject.transform.position.z);

            newdifference = rightBubbleOriginalDifference - (rightBubbleEdge.transform.position - rightPos.gameObject.transform.position).x; 
            if (Mathf.Abs(newdifference) > .03f)
                speechBubbleRight.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleRight.gameObject.transform.position.x, speechBubbleRight.gameObject.transform.position.x + newdifference, .25f),
                                                                             rightPos.gameObject.transform.position.y, rightPos.gameObject.transform.position.z);
        }
    }
}
