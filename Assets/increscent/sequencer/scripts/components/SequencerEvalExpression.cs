using UnityEngine;
using System.Collections;
using TinyExe;

public class SequencerEvalExpression : MonoBehaviour
{
    Expression exp;
	
    public void evalBool(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();

        gameObject.SendMessage("setEvalResultBool", result);
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
    
    public void evalInt(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();

        gameObject.SendMessage("setEvalResultInt", result);
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
    
    public void evalFloat(string expression)
    {
        exp = new Expression(expression);
        var result = exp.Eval();

        gameObject.SendMessage("setEvalResultFloat", result);
        
        Debug.Log("Evaluated Expression: " + expression + " with result: " + result);
    }
}
