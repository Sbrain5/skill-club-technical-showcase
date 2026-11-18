using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

/// <summary>
/// starts the menu transition when the player presses any button
/// </summary>
public class PressAnyButton : MonoBehaviour
{
    #region variables
    [SerializeField] private string nextSceneName = "MainMenuScene";
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject pressText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pressClip;
    [SerializeField] private float inputDelay = 0.35f;
    private bool started;
    #endregion

    #region unity methods
    private void OnEnable()
    {
        Invoke(nameof(StartListeningForInput), inputDelay);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(StartListeningForInput));
    }
    #endregion

    #region methods
    /// <summary>
    /// starts listening for any button press
    /// </summary>
    private void StartListeningForInput()
    {
        InputSystem.onAnyButtonPress.CallOnce(OnButtonPressed);
    }

    /// <summary>
    /// starts the transition
    /// </summary>
    /// <param name="button">button that was pressed</param>
    private void OnButtonPressed(InputControl button)
    {
        if (started)
        {
            return;
        }

        started = true;

        if (pressText != null)
        {
            pressText.SetActive(false);
        }

        if (audioSource != null && pressClip != null)
        {
            audioSource.PlayOneShot(pressClip);
        }

        if (menuAnimator != null)
        {
            menuAnimator.SetTrigger("start");
            return;
        }

        LoadNextScene();
    }

    /// <summary>
    /// loads the next scene
    /// </summary>
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    #endregion
}