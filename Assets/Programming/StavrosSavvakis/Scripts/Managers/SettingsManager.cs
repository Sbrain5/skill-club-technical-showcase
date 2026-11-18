using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// saves settings applies audio and screen values and plays ui sounds
/// </summary>
public class SettingsManager : MonoBehaviour
{
    #region variables
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private string audioSettingsPath;
    private string screenSettingsPath;
    private float musicVolume;
    private float soundVolume;
    private bool fullscreen;
    private int resolution;
    #endregion

    #region unity methods
    private void Start()
    {
        audioSettingsPath = Application.persistentDataPath + "/audio_settings.json";
        screenSettingsPath = Application.persistentDataPath + "/screen_settings.json";

        if (!File.Exists(audioSettingsPath))
        {
            AudioSettingsData audioSettingsData = new AudioSettingsData();
            audioSettingsData.musicVolume = 1f;
            audioSettingsData.soundVolume = 1f;
            File.WriteAllText(audioSettingsPath, JsonUtility.ToJson(audioSettingsData));
        }

        if (!File.Exists(screenSettingsPath))
        {
            ScreenSettingsData screenSettingsData = new ScreenSettingsData();
            screenSettingsData.fullscreen = true;
            screenSettingsData.resolution = 2;
            File.WriteAllText(screenSettingsPath, JsonUtility.ToJson(screenSettingsData));
        }

        AudioSettingsData loadedAudioSettingsData = JsonUtility.FromJson<AudioSettingsData>(File.ReadAllText(audioSettingsPath));
        ScreenSettingsData loadedScreenSettingsData = JsonUtility.FromJson<ScreenSettingsData>(File.ReadAllText(screenSettingsPath));
        musicVolume = loadedAudioSettingsData.musicVolume;
        soundVolume = loadedAudioSettingsData.soundVolume;
        fullscreen = loadedScreenSettingsData.fullscreen;
        resolution = loadedScreenSettingsData.resolution;
        audioMixer.SetFloat("MusicVolume", Mathf.Lerp(-80f, 0f, Mathf.Sqrt(musicVolume)));
        audioMixer.SetFloat("SoundVolume", Mathf.Lerp(-80f, 0f, Mathf.Sqrt(soundVolume)));

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
        }

        if (soundSlider != null)
        {
            soundSlider.SetValueWithoutNotify(soundVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(resolution);
        }

        ApplyResolution();
    }
    #endregion

    #region methods
    /// <summary>
    /// changes music volume
    /// </summary>
    /// <param name="value">slider value</param>
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        audioMixer.SetFloat("MusicVolume", Mathf.Lerp(-80f, 0f, Mathf.Sqrt(musicVolume)));
        AudioSettingsData audioSettingsData = new AudioSettingsData();
        audioSettingsData.musicVolume = musicVolume;
        audioSettingsData.soundVolume = soundVolume;
        File.WriteAllText(audioSettingsPath, JsonUtility.ToJson(audioSettingsData));
    }

    /// <summary>
    /// changes sound volume
    /// </summary>
    /// <param name="value">slider value</param>
    public void SetSoundVolume(float value)
    {
        soundVolume = value;
        audioMixer.SetFloat("SoundVolume", Mathf.Lerp(-80f, 0f, Mathf.Sqrt(soundVolume)));
        AudioSettingsData audioSettingsData = new AudioSettingsData();
        audioSettingsData.musicVolume = musicVolume;
        audioSettingsData.soundVolume = soundVolume;
        File.WriteAllText(audioSettingsPath, JsonUtility.ToJson(audioSettingsData));
    }

    /// <summary>
    /// changes fullscreen state
    /// </summary>
    /// <param name="value">toggle value</param>
    public void SetFullscreen(bool value)
    {
        fullscreen = value;
        ApplyResolution();
        ScreenSettingsData screenSettingsData = new ScreenSettingsData();
        screenSettingsData.fullscreen = fullscreen;
        screenSettingsData.resolution = resolution;
        File.WriteAllText(screenSettingsPath, JsonUtility.ToJson(screenSettingsData));
    }

    /// <summary>
    /// changes resolution
    /// </summary>
    /// <param name="value">dropdown value</param>
    public void SetResolution(int value)
    {
        resolution = value;
        ApplyResolution();
        ScreenSettingsData screenSettingsData = new ScreenSettingsData();
        screenSettingsData.fullscreen = fullscreen;
        screenSettingsData.resolution = resolution;
        File.WriteAllText(screenSettingsPath, JsonUtility.ToJson(screenSettingsData));
    }

    /// <summary>
    /// plays hover sound
    /// </summary>
    public void PlayHoverSound()
    {
        soundSource.PlayOneShot(hoverClip);
    }

    /// <summary>
    /// plays confirm sound
    /// </summary>
    public void PlayConfirmSound()
    {
        soundSource.PlayOneShot(confirmClip);
    }

    /// <summary>
    /// closes the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// applies the selected resolution
    /// </summary>
    private void ApplyResolution()
    {
        int width = 1920;
        int height = 1080;

        if (resolution == 0)
        {
            width = 1280;
            height = 720;
        }
        else if (resolution == 1)
        {
            width = 1600;
            height = 900;
        }
        else if (resolution == 2)
        {
            width = 1920;
            height = 1080;
        }
        else if (resolution == 3)
        {
            width = 2560;
            height = 1440;
        }

        Screen.SetResolution(width, height, fullscreen);
    }
    #endregion

    #region nested classes
    [System.Serializable]
    private class AudioSettingsData
    {
        #region variables
        public float musicVolume;
        public float soundVolume;
        #endregion
    }

    [System.Serializable]
    private class ScreenSettingsData
    {
        #region variables
        public bool fullscreen;
        public int resolution;
        #endregion
    }
    #endregion
}