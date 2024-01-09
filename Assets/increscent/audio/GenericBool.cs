using UnityEngine;

[CreateAssetMenu(fileName = "GenericBool", menuName = "ScriptableObjects/GenericBool")]
public class GenericBool : ScriptableObject
{
    public System.Action<bool> onChange = delegate { };

    [SerializeField] private bool _value;
    public bool Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            onChange(_value);
        }
    }
}
