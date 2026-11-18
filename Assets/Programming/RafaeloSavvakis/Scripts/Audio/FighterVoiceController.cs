using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// holds one character voice set
/// </summary>
[System.Serializable]
public class FighterVoiceSet
{
    #region variables
    public string characterName;
    public AudioClip[] roundStartClips;
    public AudioClip[] attackLightClips;
    public AudioClip[] attackHeavyClips;
    public AudioClip[] getHitClips;
    public AudioClip[] lowHealthClips;
    public AudioClip[] victoryClips;
    public AudioClip[] defeatClips;
    public AudioClip[] gruntClips;
    #endregion
}

/// <summary>
/// plays local fighter voice lines from gameplay state changes
/// </summary>
public sealed class FighterVoiceController : MonoBehaviour
{
    #region variables
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup soundMixerGroup;

    [Header("Voice Sets")]
    [SerializeField] private FighterVoiceSet[] characterVoices;

    [Header("Chances")]
    [SerializeField] private float roundStartChance = 1.0f;
    [SerializeField] private float lightAttackChance = 0.35f;
    [SerializeField] private float heavyAttackChance = 0.55f;
    [SerializeField] private float getHitChance = 0.45f;
    [SerializeField] private float lowHealthChance = 1.0f;
    [SerializeField] private float victoryChance = 1.0f;
    [SerializeField] private float defeatChance = 1.0f;
    [SerializeField] private float gruntChance = 0.25f;

    [Header("Cooldowns")]
    [SerializeField] private float globalCooldown = 1.0f;
    [SerializeField] private float attackCooldown = 1.4f;
    [SerializeField] private float hitCooldown = 1.8f;
    [SerializeField] private float lowHealthCooldown = 4.0f;
    [SerializeField] private float resultCooldown = 2.0f;
    [SerializeField] private float gruntCooldown = 1.2f;

    [Header("Round Limits")]
    [SerializeField] private int maxAttackVoicesPerRound = 4;
    [SerializeField] private int maxGetHitVoicesPerRound = 3;
    [SerializeField] private int maxGruntsPerRound = 4;
    [SerializeField] private float lowHealthPercent = 0.3f;

    private FighterVoiceSet currentVoiceSet;
    private SimAttackState lastAttackState;
    private int characterIndex;
    private int currentRoundKey = -1;
    private int lastHitWindowId = -1;
    private int lastHitStartFrame = -1;
    private int attackVoicesThisRound;
    private int getHitVoicesThisRound;
    private int gruntsThisRound;
    private bool lowHealthPlayedThisRound;
    private bool matchResultPlayed;
    private bool voiceEnabled = true;
    private float nextAnyVoiceTime;
    private float nextAttackVoiceTime;
    private float nextHitVoiceTime;
    private float nextLowHealthVoiceTime;
    private float nextResultVoiceTime;
    private float nextGruntVoiceTime;
    #endregion

    #region unity methods
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (soundMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = soundMixerGroup;
        }

        SetCharacterIndex(characterIndex);
    }
    #endregion

    #region public methods
    /// <summary>
    /// sets the active character voice set
    /// </summary>
    /// <param name="index">character index</param>
    public void SetCharacterIndex(int index)
    {
        characterIndex = index;

        if (characterVoices == null || characterVoices.Length == 0)
        {
            currentVoiceSet = null;
            return;
        }

        if (characterIndex < 0 || characterIndex >= characterVoices.Length)
        {
            characterIndex = 0;
        }

        currentVoiceSet = characterVoices[characterIndex];
        ResetVoiceState();
    }

    /// <summary>
    /// enables or disables this fighter voice locally
    /// </summary>
    /// <param name="value">true if voice can play</param>
    public void SetVoiceEnabled(bool value)
    {
        voiceEnabled = value;

        if (!voiceEnabled && audioSource != null)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// reads player state and plays local voice lines when needed
    /// </summary>
    /// <param name="state">authoritative player state</param>
    public void ApplyPlayerState(SimPlayerState state)
    {
        if (state == null || currentVoiceSet == null)
        {
            return;
        }

        CheckAttackVoice(state);
        CheckHitVoice(state);
        CheckLowHealthVoice(state);

        lastAttackState = state.AttackState;
    }

    /// <summary>
    /// reads match state and plays round start victory and defeat voices
    /// </summary>
    /// <param name="state">authoritative game state</param>
    /// <param name="playerSlot">this player slot</param>
    public void ApplyMatchState(GameState state, PlayerController.PlayerSlot playerSlot)
    {
        int roundKey;
        int ownSlotNumber;
        roundKey = state.Player01RoundWins * 10 + state.Player02RoundWins;

        if (state.MatchState == SimMatchState.Playing)
        {
            if (roundKey != currentRoundKey)
            {
                currentRoundKey = roundKey;
                ResetRoundVoiceLimits();
                TryPlay(currentVoiceSet.roundStartClips, roundStartChance, resultCooldown, ref nextResultVoiceTime, ref gruntsThisRound, 1);
            }

            return;
        }

        if (state.MatchState != SimMatchState.MatchOver || matchResultPlayed)
        {
            return;
        }

        matchResultPlayed = true;
        ownSlotNumber = GetOwnSlotNumber(playerSlot);

        if (state.MatchWinnerSlot == ownSlotNumber)
        {
            TryPlay(currentVoiceSet.victoryClips, victoryChance, resultCooldown, ref nextResultVoiceTime, ref gruntsThisRound, 1);
            return;
        }

        TryPlay(currentVoiceSet.defeatClips, defeatChance, resultCooldown, ref nextResultVoiceTime, ref gruntsThisRound, 1);
    }
    #endregion

    #region check methods
    /// <summary>
    /// checks attack voice
    /// </summary>
    /// <param name="state">player state</param>
    private void CheckAttackVoice(SimPlayerState state)
    {
        bool playedVoice;

        if (state.AttackState == SimAttackState.None)
        {
            return;
        }

        if (lastAttackState != SimAttackState.None)
        {
            return;
        }

        if (state.AttackState == SimAttackState.HeavyAttack)
        {
            playedVoice = TryPlay(currentVoiceSet.attackHeavyClips, heavyAttackChance, attackCooldown, ref nextAttackVoiceTime, ref attackVoicesThisRound, maxAttackVoicesPerRound);
        }
        else
        {
            playedVoice = TryPlay(currentVoiceSet.attackLightClips, lightAttackChance, attackCooldown, ref nextAttackVoiceTime, ref attackVoicesThisRound, maxAttackVoicesPerRound);
        }

        if (!playedVoice)
        {
            TryPlay(currentVoiceSet.gruntClips, gruntChance, gruntCooldown, ref nextGruntVoiceTime, ref gruntsThisRound, maxGruntsPerRound);
        }
    }

    /// <summary>
    /// checks get hit voice
    /// </summary>
    /// <param name="state">player state</param>
    private void CheckHitVoice(SimPlayerState state)
    {
        if (!state.IsInHitstun)
        {
            return;
        }

        if (state.LastReceivedHitWindowId < 0 || state.LastReceivedAttackStartFrame < 0)
        {
            return;
        }

        if (state.LastReceivedHitWindowId == lastHitWindowId && state.LastReceivedAttackStartFrame == lastHitStartFrame)
        {
            return;
        }

        lastHitWindowId = state.LastReceivedHitWindowId;
        lastHitStartFrame = state.LastReceivedAttackStartFrame;

        TryPlay(currentVoiceSet.getHitClips, getHitChance, hitCooldown, ref nextHitVoiceTime, ref getHitVoicesThisRound, maxGetHitVoicesPerRound);
    }

    /// <summary>
    /// checks low health voice
    /// </summary>
    /// <param name="state">player state</param>
    private void CheckLowHealthVoice(SimPlayerState state)
    {
        float healthPercent;

        if (lowHealthPlayedThisRound)
        {
            return;
        }

        if (state.MaxHealth <= 0 || state.CurrentHealth <= 0)
        {
            return;
        }

        healthPercent = (float)state.CurrentHealth / state.MaxHealth;

        if (healthPercent > lowHealthPercent)
        {
            return;
        }

        lowHealthPlayedThisRound = true;
        TryPlay(currentVoiceSet.lowHealthClips, lowHealthChance, lowHealthCooldown, ref nextLowHealthVoiceTime, ref gruntsThisRound, 1);
    }
    #endregion

    #region play methods
    /// <summary>
    /// tries to play one random clip
    /// </summary>
    /// <param name="clips">clip list</param>
    /// <param name="chance">play chance</param>
    /// <param name="cooldown">cooldown time</param>
    /// <param name="nextCategoryTime">next category time</param>
    /// <param name="categoryCount">category count</param>
    /// <param name="categoryLimit">category limit</param>
    /// <returns>true if a clip played</returns>
    private bool TryPlay(AudioClip[] clips, float chance, float cooldown, ref float nextCategoryTime, ref int categoryCount, int categoryLimit)
    {
        AudioClip clip;

        if (!voiceEnabled)
        {
            return false;
        }

        if (audioSource == null)
        {
            return false;
        }

        if (clips == null || clips.Length == 0)
        {
            return false;
        }

        if (categoryLimit > 0 && categoryCount >= categoryLimit)
        {
            return false;
        }

        if (Time.unscaledTime < nextAnyVoiceTime || Time.unscaledTime < nextCategoryTime)
        {
            return false;
        }

        if (Random.value > chance)
        {
            return false;
        }

        clip = GetRandomClip(clips);

        if (clip == null)
        {
            return false;
        }

        audioSource.PlayOneShot(clip);
        nextAnyVoiceTime = Time.unscaledTime + globalCooldown;
        nextCategoryTime = Time.unscaledTime + cooldown;
        categoryCount++;
        return true;
    }

    /// <summary>
    /// returns one random clip
    /// </summary>
    /// <param name="clips">clip list</param>
    /// <returns>random clip</returns>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        int randomIndex;
        randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }
    #endregion

    #region reset methods
    /// <summary>
    /// resets all voice state
    /// </summary>
    private void ResetVoiceState()
    {
        currentRoundKey = -1;
        matchResultPlayed = false;
        ResetRoundVoiceLimits();
    }

    /// <summary>
    /// resets per round voice limits
    /// </summary>
    private void ResetRoundVoiceLimits()
    {
        attackVoicesThisRound = 0;
        getHitVoicesThisRound = 0;
        gruntsThisRound = 0;
        lowHealthPlayedThisRound = false;
        lastAttackState = SimAttackState.None;
        lastHitWindowId = -1;
        lastHitStartFrame = -1;
    }
    #endregion

    #region helper methods
    /// <summary>
    /// returns this player slot as number
    /// </summary>
    /// <param name="playerSlot">player slot</param>
    /// <returns>player slot number</returns>
    private int GetOwnSlotNumber(PlayerController.PlayerSlot playerSlot)
    {
        if (playerSlot == PlayerController.PlayerSlot.Player01)
        {
            return 1;
        }

        return 2;
    }
    #endregion
}