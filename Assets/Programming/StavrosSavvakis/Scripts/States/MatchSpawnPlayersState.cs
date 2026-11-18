using System.Collections;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

/// <summary>
/// spawns player one and player two and gives them ownership
/// </summary>
public class MatchSpawnPlayersState : StateNode
{
    #region variables
    [SerializeField] private NetworkIdentity playerPrefab;
    [SerializeField] private Transform playerOneSpawn;
    [SerializeField] private Transform playerTwoSpawn;
    private bool started;
    private bool playersSpawned;
    #endregion

    #region state methods
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        if (playersSpawned)
        {
            machine.Next();
            return;
        }

        if (started)
        {
            return;
        }

        started = true;
        StartCoroutine(SpawnPlayers());
    }
    #endregion

    #region spawn methods
    /// <summary>
    /// waits for players and spawns them
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator SpawnPlayers()
    {
        // wait until both players exist in the network manager
        while (networkManager == null || networkManager.players == null || networkManager.players.Count < 2)
        {
            yield return null;
        }

        ArbiterSessionShare share = null;
        GameSceneCheck gameSceneCheck = null;

        while (share == null || gameSceneCheck == null || !gameSceneCheck.BothPlayersLoaded())
        {
            share = FindFirstObjectByType<ArbiterSessionShare>();

            if (share != null)
            {
                gameSceneCheck = share.GetComponent<GameSceneCheck>();
            }

            yield return null;
        }

        PlayerID player1Id = networkManager.players[0];
        PlayerID player2Id = networkManager.players[1];

        // create one player object for each player
        NetworkIdentity playerOneObject = Instantiate(playerPrefab, playerOneSpawn.position, playerOneSpawn.rotation);
        NetworkIdentity playerTwoObject = Instantiate(playerPrefab, playerTwoSpawn.position, playerTwoSpawn.rotation);

        // get the player controller scripts
        PlayerController playerOneController = playerOneObject.GetComponent<PlayerController>();
        PlayerController playerTwoController = playerTwoObject.GetComponent<PlayerController>();

        // tell each player object which side it belongs to
        playerOneController.SetPlayerSlot(PlayerController.PlayerSlot.Player01);
        playerTwoController.SetPlayerSlot(PlayerController.PlayerSlot.Player02);

        // give each player object to the correct network player
        playerOneObject.GiveOwnership(player1Id);
        playerTwoObject.GiveOwnership(player2Id);
        playersSpawned = true;
        machine.Next();
    }
    #endregion
}