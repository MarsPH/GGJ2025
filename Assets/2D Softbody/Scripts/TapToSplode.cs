using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapToSplode : MonoBehaviour
{
    public bool zCharging;
    public float splodeSize;
    public GameObject target;
    public bool canSplode;
    public GameObject splodeImage;
    public GameObject thisSplodeImage;
    public bool zClicked;
    public Slider goSlider;
    public bool sliderFill;



    void Start()
    {
        sliderFill = false;
        zClicked = false;
        zCharging = false;
        canSplode = true;
        splodeSize = 0.8f;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (canSplode)
            {
                goSlider.value = 0;
                zClicked = true;
                zCharging = true;
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                thisSplodeImage = Instantiate(splodeImage, worldPosition, Quaternion.identity);
            }

        }

        if (zCharging)
        {
            if (canSplode)
            {
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 tempVec = new Vector3(worldPosition.x, worldPosition.y, 1);
                thisSplodeImage.transform.position = tempVec;
                if (splodeSize < 1.99f)
                {
                    splodeSize += 1f * Time.deltaTime;
                    thisSplodeImage.transform.localScale = new Vector3(splodeSize, splodeSize, 1);
                }
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            if (canSplode && zClicked)
            {
                Destroy(thisSplodeImage);
                zCharging = false;
                zClicked = false;
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                GameObject thisSplode = Instantiate(target, worldPosition, Quaternion.identity);
                thisSplode.transform.localScale = new Vector3(splodeSize, splodeSize, 1);
                canSplode = false;
                StartCoroutine(SpodeTimer());
            }


        }

        if (sliderFill)
        {
            goSlider.value = Mathf.Clamp(goSlider.value + .57f * Time.deltaTime, 0, 1);
        }


    }

    IEnumerator SpodeTimer()
    {
        sliderFill = true;
        yield return new WaitForSeconds(1.75f);
        canSplode = true;
        sliderFill = false;
        splodeSize = 0.8f;
    }
}
    

