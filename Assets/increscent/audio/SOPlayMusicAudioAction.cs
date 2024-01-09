using UnityEngine;

[CreateAssetMenu(fileName = "SOPlayMusicAudioAction", menuName = "ScriptableObjects/actions/SOPlayMusicAudioAction")]
public class SOPlayMusicAudioAction : ScriptableObject
{
    public System.Action<AudioClip> action = delegate { };
}