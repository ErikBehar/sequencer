using UnityEngine;

public class AnimationEventManager : MonoBehaviour 
{
    public System.Action<string> onAnimEnd = delegate{};

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        assignEventsToThis();
    }

    void assignEventsToThis()
    {
        StateMachineEvent[] stateMachineEvents = animator.GetBehaviours<StateMachineEvent>();
        for (int i = 0; i < stateMachineEvents.Length; i++)
        {
            stateMachineEvents[i].targetToNotify = gameObject;
        }
    }

    void onExitEvent( string param)
    {
        onAnimEnd( param );
    }
}