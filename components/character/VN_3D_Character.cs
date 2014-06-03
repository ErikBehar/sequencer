using UnityEngine;
using System.Collections;

public class VN_3D_Character : VN_CharBase
{
    //TODO: custom editor to show both gameobject and string nickname next to each other

    public GameObject[] attires;   

    public override string[] getAttireNames()
    {
        string[] names = new string[attires.Length];
        for (int i = 0; i < attires.Length; i++)
        {
            names [i] = attires [i].name;
        }

        return names;
    }

    public override string[] getExpressionNames()
    {
        int clipCount = animation.GetClipCount();
        string[] names = new string[clipCount];
        int i = 0;
        foreach (AnimationState state in animation)
        {
            names [i] = state.name;
            i++;
        }

        return names;
    }

    public override void setAttire(int index)
    {
        for (int i = 0; i < attires.Length; i++)
        {
            if (i == index)
            {
                attires [i].SetActive(true);
            } else
            {
                attires [i].SetActive(false);
            }
        }        
    }
    
    public override void setExpression(string name, bool isRewind)
    {
        if (animation [name] != null)
        {
            if (isRewind)
                animation.Rewind(name);
            animation.CrossFade(name);
        } else
            Debug.LogWarning("Couldnt set anim: " + name + " cause possibly not active ?");
    }
    
    public override int getCurrentAttire()
    {
        for (int i = 0; i < attires.Length; i++)
        {
            if (attires [i].activeInHierarchy)
            {
                return i;
            }
        }

        return -1;
    }   
    
    public override int getCurrentExpression()
    {
        int i = 0;
        foreach (AnimationState state in animation)
        {
            if (state.enabled && state.speed > 0 && animation.isPlaying)
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    public override string getCurrentExpressionName()
    {
        int i = 0;
        foreach (AnimationState state in animation)
        {
            if (state.enabled && state.speed > 0 && animation.isPlaying)
            {
                if (state.clip == null)
                    return "none";
                return state.clip.name;
            }
            
            i++;
        }
        
        return "";
    }
}