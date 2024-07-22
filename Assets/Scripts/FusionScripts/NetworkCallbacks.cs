using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    // GAME OBJECTS
    private ClassReferences Refs;
    private NetworkRunner runner;
    private FusionManager FManager;
    private SetupHost setupHost;
    private SetupClient setupClient;

    public CallInputStruct inputStruct;


    private void Start()
    {
        inputStruct = new();
        Refs = ObjectReferences.Instance.ClassRefs;
        Refs.NetworkCallbacks = this;
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

    public void OnConnectedToServer(NetworkRunner runner)
    {
        // logged on client with client joins (after OnPlayerJoined)
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
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
       
        if (runner.IsServer)
        {

            if (player == runner.LocalPlayer) // host sets up variables the first time
            {
                // Create object that holds all the extra scripts you might need
                GameObject Scripts = runner.Spawn(
                    Resources.Load<GameObject>("Prefabs/ScriptObjects")).gameObject;

                // Initialize Setup variables
                FManager = Scripts.GetComponentInChildren<FusionManager>();
                setupHost = new(Refs);
                {
                    setupMono = Scripts.GetComponentInChildren<SetupMono>()
                };  // established for when other players join as well
            }

            // Do the rest of the setup for all clients
            FManager.H_InitializePlayer(player);
            setupHost.H_Setup(player.PlayerId);
            

            /*
            else // host deals other players
            {
                
                //DealMe.GetComponent<DealClient>().Player = player;
                //DealMe.SetActive(true);
            }
            */
        }

        setupHost.C_Setup();
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

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
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
