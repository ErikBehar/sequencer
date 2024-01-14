using System.Collections;
using UnityEngine;

public class ExampleSimpleSequenceUsage : MonoBehaviour
{
    public Color color;
    private System.Action<int> savedCallback;

    public bool useIndex;
    public int skipToIndex = 3;

    private Color originalColor;

    Material mat;

    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        originalColor = mat.color;
    }

    // the example operation with some example delay for callback
    public void changeBoxColor()
    {
        StartCoroutine(doSlowColorChange());
    }

    IEnumerator doSlowColorChange()
    {
        yield return new WaitForSeconds(Random.Range(.3f,1.2f));

        // toggle color
        Color currColor = mat.color;
        if (currColor == originalColor)
            mat.color = color;
        else
            mat.color = originalColor;

        // do callback once operation is finished
        if ( savedCallback != null)
        {
            if ( useIndex)
            {
                savedCallback(skipToIndex);
            }
            else
            {
                savedCallback(-1);
            }
        }
    }

    //save callback
    public void finishedCallback(System.Action<int> callback)
    {
        savedCallback = callback;
    }
}
