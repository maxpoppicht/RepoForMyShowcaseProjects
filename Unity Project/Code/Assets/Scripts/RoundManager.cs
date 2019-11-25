using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Tobias Stroedicke

public class RoundManager : MonoBehaviour
{

    public int minimumPlayers;

    /// <summary>Round count variable (USE <see cref="RoundCount"/>)</summary>
    private int roundCount = 0;
    /// <summary>Round count Property</summary>
    public int RoundCount
    {
        get { return roundCount; }
        set
        {
            roundCount = value;
        }
    }

    /// <summary>Time left of current round</summary>
    public float currentRoundTime = 0;
    /// <summary>Current Time property. only server can set time</summary>
    public float CurrentRoundTime
    {
        get { return currentRoundTime; }
        set
        {
            // if value is lower than 0 set variable to 0, else to value
            if (value <= 0)
                currentRoundTime = 0;
            else
                currentRoundTime = value;
        }
    }

    /// <summary>Wait time for next round to start after round is finished</summary>
    private float tillNextRound = 7;
    /// <summary>Wait time for next round to start after round is finished</summary>
    public float TillNextRound
    {
        get { return tillNextRound; }
        set
        {
            tillNextRound = value;
        }
    }
    /// <summary>When round starts, this value if the default time to start (0 players)</summary>
    private float StartTime
    {
        get
        {
#if UNITY_EDITOR
            return 10f;
#endif
#pragma warning disable 0162
            return 60f;
#pragma warning restore
        }
    }
    /// <summary>for each player add this to the round time (0 player -> 60 + (this * playercount) seconds)</summary>
    private float PlayerSeconds
    {
        get
        {
#if UNITY_EDITOR
            return 5f;
#endif
#pragma warning disable 0162
            return 20f;
#pragma warning restore
        }
    }
    /// <summary>calculates the time for the next round</summary>
    public float NextRoundTime
    {
        get
        {
            float time = StartTime;
            time += MyNetworkManager.AllPlayers.Count * PlayerSeconds;
            return time;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // check if player playing list is more than minimum count
        if (MyNetworkManager.AllPlayersPlaying.Count < minimumPlayers && MyNetworkManager.gameRunning)
        {
            // if no round has started yet return
            if (RoundCount == 0) return;
            if (RoundCount > 0)
            {
                // if player left in round
                foreach (PlayerEntity player in MyNetworkManager.AllPlayersPlaying)
                {
                    // reset players position to a lobby position
                    player.RpcChangeStartButtonTextToStart();
                    player.RpcTeleport(SpawnpointHandler.NextLobbypoint(), ETP.LOBBYTP);
                    player.RpcSetGOActiveState(Chaser.CurrentChaser, false);
                    player.wannaPlay = false;
                    MyNetworkManager.gameRunning = false;
                    MyNetworkManager.ReloadPlayers();
                }

                // reset stats
                Reset();
                // reset player positions and lists
                MyNetworkManager.ResetGame();


                return;
            }
        }

        // get through playing list and check if more than 2 players are ready
        int particitians = 0;
        if (MyNetworkManager.gameRunning)
        {
            foreach (PlayerEntity player in MyNetworkManager.AllPlayersPlaying)
            {
                particitians++;
                if (particitians >= minimumPlayers) break;
            }
        }
        else
        {
            foreach (PlayerEntity player in MyNetworkManager.AllPlayersWannaPlay)
            {
                particitians++;
                if (particitians >= minimumPlayers) break;
            }
        }

        // if not enough particitians are there return
        if (particitians < minimumPlayers) return;

        if (MyNetworkManager.AllPlayersPlaying.Count > 1)
            CurrentRoundTime -= Time.deltaTime;

        // reset current round time if chaser left
        if (Chaser.CurrentChaser == null && CurrentRoundTime > 0)
            CurrentRoundTime = 0;

        // When round is over, afterround starts
        if (CurrentRoundTime == 0)
        {
            // reduce time till next round
            TillNextRound -= Time.deltaTime;

            if (RoundCount == 1 && minimumPlayers == 1) TillNextRound += Time.deltaTime;

            if (TillNextRound <= 0)
            {
                // if time is up transfer lists and start new round
                MyNetworkManager.NewRound();
                NextRound();
                MyNetworkManager.gameRunning = true;
            }
        }

        // check if all players or chaser are dead, but only when round has begun
        // check if chaser is dead
        if (RoundCount != 0)
        {
            if (Chaser.CurrentChaser != null && Chaser.CurrentChaser.gameObject.GetComponent<PlayerEntity>().CurrentHP <= 0)
                RoundOver();

            // check if all players are dead
            bool alldead = true;
            foreach (PlayerEntity pe in MyNetworkManager.AllPlayersPlaying)
            {
                // continue if player is chaser
                if (pe.IsChaser) continue;

                // check if player has more than 0 hp
                if (pe.CurrentHP > 0) alldead = false;
            }

            if (alldead)
                RoundOver();
        }
    }

    /// <summary>
    /// Function is called when round is over earlier
    /// </summary>
    public void RoundOver()
    {
        CurrentRoundTime = 0;
        foreach (PlayerEntity pe in MyNetworkManager.AllPlayersPlaying)
            pe.RpcSetRoundTime(0);
    }

    /// <summary>
    /// New Round
    /// </summary>
    public void NextRound()
    {
        // rotate new chaser
        Chaser.ChooseChaser();

        TillNextRound = 5f;
        CurrentRoundTime = NextRoundTime;
        RoundCount++;

        // reset hp and sp values for new round and teleport to new position.
        foreach (PlayerEntity player in MyNetworkManager.AllPlayersPlaying)
        {
            // reset stats for player
            player.NewRoundReset();

            // teleport player to new position
            if (player.IsChaser)
            {
                player.RpcTeleport(SpawnpointHandler.NextChaserpoint(), ETP.CHASERTP);
            }
            else
            {
                player.RpcTeleport(SpawnpointHandler.NextSpawnpointPlayer(), ETP.HUNTEDTP);
            }

            // set current round time for player to save it local
            player.RpcSetRoundTime(currentRoundTime);
        }
    }

    /// <summary>
    /// Reset Timers
    /// </summary>
    private void Reset()
    {
        RoundCount = 0;
        CurrentRoundTime = 0;
        TillNextRound = 7;
    }
}
