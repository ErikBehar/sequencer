using UnityEngine;

[CreateAssetMenu(fileName = "GenericFloat", menuName = "ScriptableObjects/GenericFloat")]
public class GenericFloat : ScriptableObject
{
    public System.Action<float> onChange = delegate{ };

    [SerializeField] private float _value;
    public float Value
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