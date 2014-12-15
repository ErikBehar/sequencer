using UnityEngine;

public abstract class VN_CharBase : MonoBehaviour
{
    public abstract string[] getAttireNames();
    public abstract string[] getExpressionNames();

    public abstract void setAttire(int index);
    public abstract void setAttire(string name);
    public abstract void setExpression(string name, bool isRewind);

    public abstract int getCurrentAttire();
    public abstract string getCurrentAttireName();
    public abstract int getCurrentExpression();
    public abstract string getCurrentExpressionName();
    public abstract void flip();
}
