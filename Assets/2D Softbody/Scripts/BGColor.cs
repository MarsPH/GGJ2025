using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGColor : MonoBehaviour
{

    public Camera zCam;
    public float duration = 1.0f;
    public float cycleSeconds = 60.0f; // set to say 0.5f to test


    void Update()
    {

        float t = Mathf.PingPong(Time.time, duration) / duration;
        zCam.backgroundColor = Color.HSVToRGB(
        Mathf.Repeat(Time.time / cycleSeconds, 1f), 0.5f, 0.9f);


    }
}


