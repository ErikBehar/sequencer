using UnityEngine;

public abstract class DialogControllerBase : MonoBehaviour
{
    public abstract void showDialog(string text, GameObject target, float xOffset);
    public abstract void hideDialog();
    public abstract void onForward();
    public abstract bool dialogIsShown();
    public abstract void dialogForceComplete();
}
