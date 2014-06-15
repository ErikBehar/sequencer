using System.Collections.Generic;
using UnityEngine;

public abstract class ChoiceControllerBase : MonoBehaviour
{
    public abstract void generateButtons(List<ChoiceModel> choices, SequencePlayer player);
    public abstract void cleanup();
}
