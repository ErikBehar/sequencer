#pragma strict

public function evalBool( expression:String):boolean
{
	var result:boolean = eval( expression);
	gameObject.SendMessage( "setEvalResultBool", result);
}

public function evalInt ( expression:String):int
{
	var result:int = eval( expression);
	gameObject.SendMessage( "setEvalResultInt", result);
}

public function evalFloat( expression:String):float
{
	var result:float = eval( expression);
	gameObject.SendMessage( "setEvalResultFloat", result);
}