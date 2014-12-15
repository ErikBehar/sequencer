using UnityEngine;
using System.Collections;

public class SequencePlayerInput : MonoBehaviour
{
    public SequencePlayer player;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            player.input(-1);
        } else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            player.input(1);
        }
    }
}
