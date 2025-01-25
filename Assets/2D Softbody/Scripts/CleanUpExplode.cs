using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanUpExplode : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(CleanYoSelf());
    }

    IEnumerator CleanYoSelf()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

}
