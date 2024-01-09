using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class UGuiDialogBubble : DialogControllerBase
{
    public GameObject speechBubbleLeft;
    public GameObject speechBubbleRight;
    public GameObject narratorBubble;

    public Text narratorTextLabel;
    public Text speechTextLabelLeft;
    public Text speechTextLabelRight;

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

    bool shown = false;
    //Text lastShownText;

    public bool hideDialogAutomaticallyOnForward = true;

    void Awake()
    {
        hideDialog();

        leftBubbleOriginalDifference = (leftBubbleEdge.transform.position - speechBubbleLeft.transform.position).x;

        rightBubbleOriginalDifference = (rightBubbleEdge.transform.position - speechBubbleRight.transform.position).x;
    }

    // we guess which balloon to use based on general position, but we use the 
    // actual position sent in instead of hardcoded one
    override public void showDialog(string text, GameObject charspeaker, float xOffset)
    {
        shown = true;
        text = chopTextIntoPieces(textLineSize, parseForCarriageReturns(text));

        if ( Vector3.Distance(charspeaker.transform.position, leftPos.position) > Vector3.Distance(charspeaker.transform.position, rightPos.position) )
        {            
            speechTextLabelRight.text = text;
            rightPosVec = charspeaker.transform.position + new Vector3(xOffset, 0, 0);
            speechBubbleRight.transform.position = rightPosVec;
            speechBubbleRight.SetActive(true);
            //lastShownText = speechTextLabelRight;
        } else if (Vector3.Distance(charspeaker.transform.position, leftPos.position) < Vector3.Distance(charspeaker.transform.position, rightPos.position))
        { 
            speechTextLabelLeft.text = text;
            leftPosVec = charspeaker.transform.position + new Vector3(xOffset, 0, 0);
            speechBubbleLeft.transform.position = leftPosVec;
            speechBubbleLeft.SetActive(true);
            //lastShownText = speechTextLabelLeft;
        } else
        {
            narratorTextLabel.text = text;
            centerPosVec = charspeaker.transform.position + new Vector3(xOffset, 0, 0);
            narratorBubble.transform.position = centerPosVec;
            narratorBubble.SetActive(true);
            //lastShownText = narratorTextLabel;
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

    override public void onForward()
    {
        if (hideDialogAutomaticallyOnForward)
        {
            hideDialog();
        }

        //Do nothing ?
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
            // float newdifference = leftBubbleOriginalDifference - (leftBubbleEdge.transform.position - leftPosVec).x; 
            // if (Mathf.Abs(newdifference) > .03f)
            //     speechBubbleLeft.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleLeft.gameObject.transform.position.x, speechBubbleLeft.gameObject.transform.position.x + newdifference, .25f),
            //                                                                  leftPosVec.y, leftPosVec.z);

            // newdifference = rightBubbleOriginalDifference - (rightBubbleEdge.transform.position - rightPosVec).x; 
            // if (Mathf.Abs(newdifference) > .03f)
            //     speechBubbleRight.gameObject.transform.position = new Vector3(Mathf.Lerp(speechBubbleRight.gameObject.transform.position.x, speechBubbleRight.gameObject.transform.position.x + newdifference, .25f),
            //                                                                   rightPosVec.y, rightPosVec.z);
        }
    }

    override public bool dialogIsShown()
    {
        //fix this if typewritter effect for uGUI is implemented
        return true;
    }

    override public void dialogForceComplete()
    {
        //add this if typewritter effect for uGUI is implemented
    }
}
