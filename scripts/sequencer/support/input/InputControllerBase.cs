using UnityEngine;

public abstract class InputControllerBase : MonoBehaviour
{
    public abstract void showInputFor(string variableName, SequencePlayer player);
    public abstract void hideInput();
}