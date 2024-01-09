using UnityEngine;

[CreateAssetMenu(fileName = "SOPlay3DAudioAction", menuName = "ScriptableObjects/actions/SOPlay3DAudioAction")]
public class SOPlay3DAudioAction : ScriptableObject
{
    // Position in World, AudioClip to play, volume 
    public System.Action<Vector3, AudioClip, float> action = delegate { };
}