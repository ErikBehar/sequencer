using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

public class NguiDialogBubble : DialogControllerBase
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

    public Vector3 centerPosVec;
    public Vector3 leftPosVec;
    public Vector3 rightPosVec;

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

    override public void showDialog(string text, GameObject charspeaker, float xOffset)
    {
        shown = true;
        text = chopTextIntoPieces(textLineSize, parseForCarriageReturns(text));

        if (charspeaker.transform.localPosition.x > 1)
        {            
            speechTextLabelRight.text = text;
            rightPosVec = rightPos.position + new Vector3(xOffset, 0, 0);
            speechBubbleRight.transform.position = rightPosVec;
            speechBubbleRight.SetActive(true);
        } else if (charspeaker.transform.localPosition.x < -1)
        { 
            speechTextLabelLeft.text = text;
            leftPosVec = leftPos.position + new Vector3(xOffset, 0, 0);
            speechBubbleLeft.transform.position = leftPosVec;
            speechBubbleLeft.SetActive(true);
        } else
        {
            narratorTextLabel.text = text;
            centerPosVec = centerPos.position + new Vector3(xOffset, 0, 0);
            narratorBubble.transform.position = centerPosVec;
            narratorBubble.SetActive(true);
        }
    }

    public string parseForCarriageReturns(string text)
    {
        return text.Replace("\\n", Environment.NewLine);
    }

    public string chopTextIntoPieces(int size, string originalText)
    {
        string[] temp = originalText.Split(new string[]
        {
            " ",
            Environment.NewLine
        }, StringSplitOptions.None);
        
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

    override public void hideDialog()
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
            float newdifference = leftBubbleOriginalDifference - (leftBubbleEdge.transform.position - leftPosVec).x; 
            if (Mathf.Abs(newdifference) > .03f)
                speechBubbleLeft.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleLeft.gameObject.transform.position.x, speechBubbleLeft.gameObject.transform.position.x + newdifference, .25f),
                                                                             leftPosVec.y, leftPosVec.z);

            newdifference = rightBubbleOriginalDifference - (rightBubbleEdge.transform.position - rightPosVec).x; 
            if (Mathf.Abs(newdifference) > .03f)
                speechBubbleRight.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleRight.gameObject.transform.position.x, speechBubbleRight.gameObject.transform.position.x + newdifference, .25f),
                                                                              rightPosVec.y, rightPosVec.z);
        }
    }
}
