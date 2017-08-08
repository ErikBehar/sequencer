using UnityEngine;
using System.Collections;

public class VN_3D_Character : VN_CharBase
{
    //TODO: custom editor to show both gameobject and string nickname next to each other

    public GameObject[] attires;  

    public Vector3 flipAngleVector;

    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation.eulerAngles;
    }

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
        Animation anim = GetComponent<Animation>();
        if (anim == null)
            return null;

        int clipCount = anim.GetClipCount();
        string[] names = new string[clipCount];
        int i = 0;
        foreach (AnimationState state in GetComponent<Animation>())
        {
            names [i] = state.name;
            i++;
        }

        return names;
    }

    public override string getCurrentAttireName()
    {
        for (int i = 0; i < attires.Length; i++)
        {
            if (attires [i].activeInHierarchy)
                return attires [i].name;
        }
        
        return "";
    }

    public override void setAttire(string name)
    {
        for (int i = 0; i < attires.Length; i++)
        {
            if (attires [i].name == name)
                attires [i].SetActive(true);
            else
                attires [i].SetActive(false);
        } 
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
        if (GetComponent<Animation>() [name] != null)
        {
            if (isRewind)
                GetComponent<Animation>().Rewind(name);
            GetComponent<Animation>().CrossFade(name);
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
        foreach (AnimationState state in GetComponent<Animation>())
        {
            if (state.enabled && state.speed > 0 && GetComponent<Animation>().isPlaying)
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
        foreach (AnimationState state in GetComponent<Animation>())
        {
            if (state.enabled && state.speed > 0 && GetComponent<Animation>().isPlaying)
            {
                if (state.clip == null)
                    return "none";
                return state.clip.name;
            }
            
            i++;
        }
        
        return "";
    }

    public override void flip()
    {
        if (Mathf.Round(transform.localRotation.eulerAngles.y) == Mathf.Round(initialRotation.y))
            transform.localRotation = Quaternion.Euler(flipAngleVector);
        else
            transform.localRotation = Quaternion.Euler(initialRotation);
    }
}