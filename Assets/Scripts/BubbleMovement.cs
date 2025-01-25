using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleMovement : MonoBehaviour
{
      public bool zCharging;
    public float splodeSize;
    public GameObject explosionEffect;
    public bool canSplode;
    public GameObject splodeImage;
    public GameObject thisSplodeImage;
    public bool zClicked;
    public Slider goSlider;
    public bool sliderFill;
    public float explosionForce = 10f; // Force applied to the player
    public Rigidbody2D playerRigidbody; // Reference to the player's Rigidbody2D

    void Awake()
    {
        sliderFill = false;
        zClicked = false;
        zCharging = false;
        canSplode = true;
        splodeSize = 0.2f; // Start with a small explosion size
        playerRigidbody = GetComponent<Rigidbody2D>();
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
                if (splodeSize < 2.5f) // Maximum explosion size
                {
                    splodeSize += 1.5f * Time.deltaTime;
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

                // Instantiate explosion effect (optional, for visuals)
                Instantiate(explosionEffect, worldPosition, Quaternion.identity);

                // Calculate direction and apply force to the player
                Vector2 explosionDirection = ((Vector2)transform.position - worldPosition).normalized;
                playerRigidbody.AddForce(explosionDirection * explosionForce * splodeSize, ForceMode2D.Impulse);

                canSplode = false;
                StartCoroutine(SplodeCooldown());
            }
        }

        if (sliderFill)
        {
            goSlider.value = Mathf.Clamp(goSlider.value + .57f * Time.deltaTime, 0, 1);
        }
    }

    IEnumerator SplodeCooldown()
    {
        sliderFill = true;
        yield return new WaitForSeconds(1.5f); // Cooldown before the next explosion
        canSplode = true;
        sliderFill = false;
        splodeSize = 0.2f; // Reset explosion size
    }
}
