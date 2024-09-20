using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    // GAME OBJECTS
    ClassReferences refs;
    NetworkRunner runner;

    GameObject Scripts;
    FusionManager fManager;
    SetupServer setupHost;
    SetupClient setupClient;

    public InputFromClient inputStruct;


    private void Start()
    {
        inputStruct = new();
        refs = ObjectReferences.Instance.ClassRefs;
        refs.NetworkCallbacks = this;
    }

    public async void StartGame(GameMode mode)
    {
        // CREATE RUNNER WITH INPUT
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        // START OR JOIN THE GAME
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Mah Jongg Room",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 4
        });

        runner.AddCallbacks(this);
    }

    public void OnDisable()
    {
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    // INetworkRunnerCallbacks

#pragma warning disable UNT0006 // Incorrect message signature
    public void OnConnectedToServer(NetworkRunner runner)
#pragma warning restore UNT0006 // Incorrect message signature
    {
        // logged on client with client joins (after OnPlayerJoined)
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
#pragma warning disable UNT0006 // Incorrect message signature
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
#pragma warning restore UNT0006 // Incorrect message signature
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(inputStruct);
        inputStruct = default;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {   // logged on host when host joins
        // logged on host when client joins
        // logged on client when client joins

        // everybody does this, host player and clients

        if (!Scripts)
        {
            Scripts = runner.Spawn(
            Resources.Load<GameObject>("Prefabs/ScriptObjects")).gameObject;
        }
        SetupMono setupMono = Scripts.GetComponentInChildren<SetupMono>();
        setupClient = new(refs, setupMono);
        setupClient.SetupDriver();

        if (runner.IsServer)
        {   // code for host
            fManager = Scripts.GetComponentInChildren<FusionManager>();
            fManager.InitializePlayer(player);

            if (player == runner.LocalPlayer) // host sets up variables the first time
            {   // code for host when host joins

                // Initialize Setup variables
                setupHost = new(refs);

                // shuffle and deal
                setupHost.SetupDriver();
            }

            // Do the rest of the setup for all clients

            refs.TileTracker.SendGameStateToAll();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

#pragma warning disable UNT0006 // Incorrect message signature
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
#pragma warning restore UNT0006 // Incorrect message signature
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }
}
