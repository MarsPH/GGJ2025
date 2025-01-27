using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}