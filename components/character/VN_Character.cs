using UnityEngine;
using System.Collections;

public class VN_Character : VN_CharBase
{
    //TODO: custom editor to show both gameobject and string nickname next to each other

    public GameObject[] attires;
    public GameObject[] expressions;        

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
        string[] names = new string[expressions.Length];
        for (int i = 0; i < expressions.Length; i++)
        {
            names [i] = expressions [i].name;
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
        for (int i = 0; i < expressions.Length; i++)
        {
            if (expressions [i].name == name)
            {
                expressions [i].SetActive(true);
            } else
            {
                expressions [i].SetActive(false);
            }
        }
    }
    
    public override int getCurrentAttire()
    {
        for (int i = 0; i < attires.Length; i++)
        {
            if (attires [i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }   

    public override int getCurrentExpression()
    {
        for (int i = 0; i < expressions.Length; i++)
        {
            if (expressions [i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    public override string getCurrentExpressionName()
    {
        for (int i = 0; i < expressions.Length; i++)
        {
            if (expressions [i].activeSelf)
            {
                return expressions [i].name;
            }
        }
        return "";
    }
}