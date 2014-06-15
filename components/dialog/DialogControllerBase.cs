using UnityEngine;

public abstract class DialogControllerBase : MonoBehaviour
{
    public abstract void showDialog(string text, GameObject target, float xOffset);
    public abstract void hideDialog();
}
