using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Tobias Stroedicke

public class Chaser : NetworkBehaviour {

    private static Chaser m_chaser;
#pragma warning disable 0649
    [SerializeField]
    private float ChaserSPRegenMultiplier;
#pragma warning restore
    public static float DamageMultiplier { get { return 4f; } }
    public static GameObject CurrentChaser { get; private set; }
    public static GameObject LastRoundChaser { get; private set; }

    private void Start()
    {
        m_chaser = GameObject.Find("RoundManager").GetComponent<Chaser>();
    }

    /// <summary>
    /// Chooses the chaser randomly. Eliminate players who do not have the lowest amount of Chaser players, afterwards choose randomly from pool
    /// </summary>
    [Server]
    public static void ChooseChaser()
    {
        // Copy of all players
        List<PlayerEntity> chaserPool = new List<PlayerEntity>(MyNetworkManager.AllPlayersPlaying);
        // copy of chaser pool
        List<PlayerEntity> peCopy = new List<PlayerEntity>(chaserPool);

        foreach (PlayerEntity pe in peCopy)
        {
            // if player was chaser last round delete from pool
            if (pe.IsChaser || pe.WasChaserLastRound)
            {
                pe.SetChaser(false);
                chaserPool.Remove(pe);
                if (pe.WasChaserLastRound)
                {
                    LastRoundChaser = pe.gameObject;
                }
            }
        }

        // Get lowest chasernumber
        peCopy.Clear();
        peCopy = new List<PlayerEntity>(chaserPool);

        int lowest = int.MaxValue;
        foreach (PlayerEntity pe in peCopy)
        {
            // check if variable is lower than players chaser count.
            if (pe.ChaserCount <= lowest)
                // set new count
                lowest = pe.ChaserCount;
            else
            {
                pe.SetChaser(false);
                // remove player from list
                chaserPool.Remove(pe);
            }
        }

        // remove all players which does not match chasercount with lowest variable
        peCopy.Clear();
        peCopy = new List<PlayerEntity>(chaserPool);
        foreach (PlayerEntity pe in peCopy)
        {
            if (pe.ChaserCount != lowest)
            {
                pe.SetChaser(false);
                chaserPool.Remove(pe);
            }
        }

        peCopy = null;

        // if pool bigger that 1 choose randomly
        if (chaserPool.Count == 0)
        {
            Debug.LogWarning("No Chaser found! Server will be set as chaser", MyNetworkManager.AllPlayersPlaying[0].gameObject);
            MyNetworkManager.AllPlayersPlaying[0].SetChaser(true);
            CurrentChaser = MyNetworkManager.AllPlayersPlaying[0].gameObject;
        }
        else if (chaserPool.Count == 1)
        {
            PlayerEntity p = chaserPool[0];
            p.SetChaser(true);
            CurrentChaser = p.gameObject;
            chaserPool.Remove(p);
        }
        else
        {
            // chose player randomly
            int chaserIndex = Random.Range(0, (chaserPool.Count - 1));

            // save player
            PlayerEntity p = chaserPool[chaserIndex];

            // set player to chaser
            p.SetChaser(true);
            CurrentChaser = p.gameObject;

            // remove from list
            chaserPool.Remove(p);
        }

        foreach (PlayerEntity pe in chaserPool)
        {
            pe.SetChaser(false);
        }

        // Set Color
        foreach (PlayerEntity pe in MyNetworkManager.AllPlayersPlaying)
        {
            //pe.RpcSetChaserColor(CurrentChaser, LastRoundChaser);
            pe.RpcDeActivateValkyrie(CurrentChaser, LastRoundChaser);
        }

        CurrentChaser.GetComponent<PlayerEntity>().CmdSetRegenSP(AEntity.SpRegenDefault * m_chaser.ChaserSPRegenMultiplier);
        if (LastRoundChaser != null)
            LastRoundChaser.GetComponent<PlayerEntity>().CmdSetRegenSP(AEntity.SpRegenDefault);
    }
}
