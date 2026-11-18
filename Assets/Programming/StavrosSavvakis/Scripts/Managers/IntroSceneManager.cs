using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// plays the intro video and loads the next scene when it ends
/// </summary>
public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer.loopPointReached += LoadNextScene;
    }

    /// <summary>
    /// loads the next scene when the video ends
    /// </summary>
    /// <param name="videoPlayer">the video player</param>
    private void LoadNextScene(VideoPlayer videoPlayer)
    { 
        SceneManager.LoadScene("PostMainMenuScene");
    }
}