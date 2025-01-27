using UnityEngine;

public class Collectible : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerMain"))
        {
            PlayPickupSound();
            HandlePickup();
        }
    }

    private void PlayPickupSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            // Create a new GameObject for the sound
            GameObject soundPlayer = new GameObject("PickupSound");
            AudioSource newAudioSource = soundPlayer.AddComponent<AudioSource>();

            // Set the clip and properties
            newAudioSource.clip = audioSource.clip;
            newAudioSource.volume = audioSource.volume;
            newAudioSource.pitch = audioSource.pitch;
            newAudioSource.spatialBlend = audioSource.spatialBlend; // For 3D sound
            newAudioSource.Play();

            // Destroy the sound object after the clip finishes
            Destroy(soundPlayer, audioSource.clip.length);
        }
    }

    private void HandlePickup()
    {
        // Add your collectible logic here (e.g., increase score, notify game manager)
        Debug.Log("Collectible picked up!");

        // Optionally delay destruction until after handling logic
        Destroy(gameObject);
    }
}