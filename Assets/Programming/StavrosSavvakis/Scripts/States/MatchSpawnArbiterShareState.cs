using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

/// <summary>
/// spawns the arbiter session share object
/// </summary>
public class MatchSpawnArbiterShareState : StateNode
{
    #region variables
    [SerializeField] private NetworkIdentity arbiterSessionSharePrefab;
    private bool shareSpawned;
    #endregion

    #region state methods
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        if (!shareSpawned)
        {
            Instantiate(arbiterSessionSharePrefab, Vector3.zero, Quaternion.identity);
            shareSpawned = true;
        }

        machine.Next();
    }
    #endregion
}