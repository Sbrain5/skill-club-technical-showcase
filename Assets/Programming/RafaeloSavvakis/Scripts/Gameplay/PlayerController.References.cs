using UnityEngine;

/// <summary>
/// handles player controller references and character visuals
/// </summary>
public sealed partial class PlayerController
{
    #region reference methods
    /// <summary>
    /// enables only the selected character visual
    /// </summary>
    private void ApplyCharacterVisual()
    {
        int i;

        if (this.characterVisuals.Length == 0)
        {
            return;
        }

        i = 0;

        while (i < this.characterVisuals.Length)
        {
            this.characterVisuals[i].SetActive(i == this.characterIndex);
            i++;
        }
    }
    /// <summary>
    /// pushes the current player slot to any child combat anchor components
    /// </summary>
    private void ApplyPlayerSlotToCombatAnchors()
    {
        Hitbox[] hitboxes;
        Hurtbox[] hurtboxes;

        hitboxes = this.GetComponentsInChildren<Hitbox>(true);
        hurtboxes = this.GetComponentsInChildren<Hurtbox>(true);

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.SetOwnerSlot(this.playerSlot);
        }

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            hurtbox.SetOwnerSlot(this.playerSlot);
        }
    }
    #endregion
}