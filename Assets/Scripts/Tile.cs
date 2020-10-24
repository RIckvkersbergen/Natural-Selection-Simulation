using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject placedObject;
    public bool hasTree = false;
    public bool hasFood = false;

    void Update()
    {
        Available();
    }

    void Available()
    {
        if(placedObject == null && hasFood == true)
        {
            hasFood = false;
        }
    }
}
