#pragma strict
#pragma downcast

public function evalBool( expression:String):boolean
{
	var result:boolean = eval( expression);
	gameObject.SendMessage( "setEvalResultBool", result);

	Debug.Log( "Evaluated Expression: " + expression + " with result: " + result );
}

public function evalInt ( expression:String):int
{
	var result:int = eval( expression);
	gameObject.SendMessage( "setEvalResultInt", result);
	
	Debug.Log( "Evaluated Expression: " + expression + " with result: " + result );
}

public function evalFloat( expression:String):float
{
	var result:float = eval( expression);
	gameObject.SendMessage( "setEvalResultFloat", result);
	
	Debug.Log( "Evaluated Expression: " + expression + " with result: " + result );
}