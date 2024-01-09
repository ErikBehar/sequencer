using UnityEngine;

[CreateAssetMenu(fileName = "SOPlaySoundAction", menuName = "ScriptableObjects/actions/SOPlaySoundAction")]
public class SOPlaySoundAction : ScriptableObject
{
    public System.Action<AudioClip, float> action = delegate { };
}