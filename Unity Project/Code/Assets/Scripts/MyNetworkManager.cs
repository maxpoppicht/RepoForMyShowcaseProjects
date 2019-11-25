using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Tobias Stroedicke

public class MyNetworkManager : NetworkManager {

    /// <summary>True when game is running</summary>
    public static bool gameRunning = false;
    /// <summary>when true, looking for players</summary>
    private static bool Reload { get; set; }
    /// <summary>Get Networkmanager Singleton</summary>
    public static NetworkManager GetSingleton { get { return singleton; } }

    /// <summary>Set while initialize, if true player has access to Update</summary>
    public static bool CurrentlyServer = false;

    /// <summary>List with all players in Scene</summary>
    private static List<PlayerEntity> allPlayers = new List<PlayerEntity>();
    /// <summary>List with all players</summary>
    public static List<PlayerEntity> AllPlayers { get { return allPlayers; } }

    /// <summary>List with all players in Scene</summary>
    private static List<GameObject> allPlayersGo = new List<GameObject>();
    /// <summary>List with all players</summary>
    public static List<GameObject> AllPlayersGo { get { return allPlayersGo; } }

    /// <summary>List with all players in Game</summary>
    private static List<PlayerEntity> allPlayersPlaying = new List<PlayerEntity>();
    /// <summary>List with all players who are playing</summary>
    public static List<PlayerEntity> AllPlayersPlaying { get { return allPlayersPlaying; } }

    /// <summary>List with all players in lobby who wanna play</summary>
    private static List<PlayerEntity> allPlayersWannaPlay = new List<PlayerEntity>();
    /// <summary>List with all players in lobby who wanna play</summary>
    public static List<PlayerEntity> AllPlayersWannaPlay { get { return allPlayersWannaPlay; } }

    /// <summary>List with all players in Lobby</summary>
    private static List<PlayerEntity> allPlayersLobby = new List<PlayerEntity>();
    /// <summary>List with all players in lobby</summary>
    public static List<PlayerEntity> AllPlayersLobby { get { return allPlayersLobby; } }
    private void Start()
    {
        if (!CurrentlyServer) return;
        Reload = true;
    }

    private void Update()
    {
        // If client is not connected (in Offline Scene) ignore Networkmanager
        if (!IsClientConnected())
        {
            // set isserver to false
            CurrentlyServer = false;
            return;
        }

        // if is not server return
        if (!CurrentlyServer) return;

        // if player joined or left reload all player
        if (Reload)
        {
            SearchPlayer(true);
        }
    }

    /// <summary>
    /// Add player to game
    /// </summary>
    /// <param name="_player">The player</param>
    /// <param name="_setWannaPlay">true: Set <see cref="AEntity.wannaPlay"/> value to true</param>
    public static void AddPlayer(GameObject _player)
    {
        PlayerEntity p = _player.GetComponent<PlayerEntity>();
        AllPlayersWannaPlay.Add(p);
        allPlayersLobby.Remove(p);
    }

    /// <summary>
    /// Remove player from game
    /// </summary>
    /// <param name="_player">The player</param>
    public static void RemovePlayer(GameObject _player)
    {
        PlayerEntity p = _player.GetComponent<PlayerEntity>();
        allPlayersLobby.Add(p);
        AllPlayersWannaPlay.Remove(p);
    }

    /// <summary>
    /// Searching player in this Scene
    /// </summary>
    /// <param name="_force">
    /// false: Only overrite list when playercount is equal with found players
    /// || 
    /// true: overrite list even that playercount is equal with found players
    /// </param>
    private static void SearchPlayer(bool _force = false)
    {
        // find objects with tag
        List<GameObject> tempGO = (GameObject.FindGameObjectsWithTag("Player")).ToList();
        if (!_force)
        {
            if (tempGO.Count == allPlayers.Count)
            {
                return;
            }
        }

        // set Reload to false to the server stop looking for new players next frame
        Reload = false;

        // reset list
        allPlayers.Clear();
        allPlayersGo.Clear();
        allPlayersPlaying.Clear();
        AllPlayersWannaPlay.Clear();
        allPlayersLobby.Clear();

        // Add player gameobject to lists
        foreach (GameObject go in tempGO)
        {
            if (go.name == "PlayerNew(Clone)")
            {
                // add to all player (go) list
                AllPlayersGo.Add(go);
                AllPlayers.Add(go.gameObject.GetComponent<PlayerEntity>());

                // add to playing or lobby list
                if (go.gameObject.transform.position.y > 400)
                {
                    // check if player wants to play
                    if (go.gameObject.GetComponent<PlayerEntity>().wannaPlay == true)
                    {
                        AllPlayersWannaPlay.Add(go.GetComponent<PlayerEntity>());
                    }
                    else
                    {
                        AllPlayersLobby.Add(go.GetComponent<PlayerEntity>());
                    }
                }
                else
                    AllPlayersPlaying.Add(go.GetComponent<PlayerEntity>());
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        // set isserver to false
        CurrentlyServer = false;
        // clear all lists
        allPlayersGo.Clear();
        allPlayers.Clear();
        allPlayersPlaying.Clear();
        allPlayersLobby.Clear();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        CurrentlyServer = true;
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Reload = true;

        conn.Disconnect();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Reload = true;

        // activate HUD
        NetworkManagerHUD hud = GameObject.Find("Network Manager").GetComponent<NetworkManagerHUD>();
        hud.showGUI = true;
    }

    /// <summary>
    /// Returns a Copy of all players
    /// </summary>
    /// <returns>copy of all players</returns>
    public static List<PlayerEntity> AllPlayerCopy()
    {
        return new List<PlayerEntity>(AllPlayers);
    }

    public void CloseConnection(PlayerEntity _player)
    {
        NetworkManagerHUD hud = GameObject.Find("Network Manager").GetComponent<NetworkManagerHUD>();
        hud.showGUI = true;

        if (_player.isServer)
            MyNetworkManager.singleton.StopHost();
        else
            MyNetworkManager.singleton.StopClient();
    }

    public static void ResetGame()
    {
        // save playing list
        List<PlayerEntity> temp = new List<PlayerEntity>(AllPlayersPlaying);

        // for each player who was playing transfer to lobby list
        foreach (PlayerEntity pe in temp)
        {
            AllPlayersLobby.Add(pe);
        }
        // clear list
        AllPlayersPlaying.Clear();
        temp = null;
        gameRunning = false;
    }

    public static void NewRound()
    {
        // copy all PEs to Playing list
        foreach (PlayerEntity pe in AllPlayersWannaPlay)
        {
            // set wannaplay to false
            pe.wannaPlay = false;
            // add PE to list
            AllPlayersPlaying.Add(pe);
        }

        // clear wanna play list
        AllPlayersWannaPlay.Clear();
    }

    /// <summary>
    /// Will reload players next frame
    /// </summary>
    public static void ReloadPlayers()
    {
        Reload = true;
    }

}
