using TinyExe;
using UnityEngine;

public class SequencerEvalExpression : MonoBehaviour
{
    Expression exp;

    private SequencePlayer player;
    private void Awake()
    {
        player = GetComponent<SequencePlayer>();
    }

    public void evalBool(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();
        string string_result = result.ToString();
        bool bool_result;
        bool_result = string_result == "1";
        if (bool_result == false)
        {
            bool_result = string_result == "True";
        }

        player.setEvalResultBool(bool_result);
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
    
    public void evalInt(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();

        player.setEvalResultInt(int.Parse(result.ToString()));
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
    
    public void evalFloat(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();

        player.setEvalResultFloat( float.Parse(result.ToString()));
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
}
