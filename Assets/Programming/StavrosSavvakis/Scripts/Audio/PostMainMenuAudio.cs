using UnityEngine;

/// <summary>
/// plays the logo sound first and starts the menu music after a short delay
/// </summary>
public class PostMainMenuAudio : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource logoSoundSource;
    [SerializeField] private float musicDelay = 0.2f;

    private void Start()
    {
        logoSoundSource.Play();
        musicSource.PlayDelayed(musicDelay);
    }
}