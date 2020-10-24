using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNA : MonoBehaviour
{
    //Initialize the speedtrait
    [HideInInspector]
    public const float minSpeed = 0.1f;
    public const float maxSpeed = 1f;

    [HideInInspector]
    public float speed;

    //Initialize the sensetrait
    [HideInInspector]
    public const float minSense = 0.1f;
    public const float maxSense = 1f;

    [HideInInspector]
    public float sense;

    //Initialize the sizetrait
    [HideInInspector]
    public const float minSize = 0.1f;
    public const float maxSize = 1f;

    [HideInInspector]
    public float size;

    //Initialize the resistancetrait
    public const float minResistance = 0.1f;
    public const float maxResistance = 1f;

    [HideInInspector]
    public float resistance;
}
