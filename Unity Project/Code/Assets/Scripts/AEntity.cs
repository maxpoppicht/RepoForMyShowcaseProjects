using Assets.Scripts.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Cinemachine;

// Tobias Stroedicke

/// <summary>
/// Main Class of Player
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class AEntity : NetworkBehaviour
{
    protected bool wasHit;
    protected float hitTimer;
    public float HitTimer { get { return 0.2f; } }

    ///<summary>Tag of anything where player can get damage</summary>
#if UNITY_EDITOR
    [TagSelector]
#endif
    [SerializeField]
#pragma warning disable 0649
    private string m_DamageTag;
#pragma warning restore

    ///<summary>see <see cref="m_DamageTag"/></summary>
    public string DamageTag { get { return m_DamageTag; } }

    public static float DashConsumption { get { return 10.0f; } }
    public static float WalljumpConsumption { get { return 15.0f; } }
    public static float SpRegenDefault { get { return 5f; } }

    protected Rigidbody m_rigidbody;

    [Header("Materials")]
    [Tooltip("Primary color for Chaser")]
    ///<summary>Red material</summary>
    public Material matRed;
    ///<summary>Blue material</summary>
    [Tooltip("Primary color for Player in Game")]
    public Material matBlue;
    ///<summary>White material</summary>
    [Tooltip("Primary color for Player in Lobby")]
    public Material matWhite;

    public Material ChaserMaterial { get { return matRed; } }
    public Material PlayerMaterial { get { return matBlue; } }
    public Material DefaultMaterial { get { return matWhite; } }

    [Header("Player")]
    [SerializeField]
    protected GameObject m_riggedPlayer;

    [SerializeField]
    protected GameObject m_body;

    ///<summary>Player look position</summary>
    [SerializeField]
    protected Transform m_lookAt;

    [Header("Valkyrie")]
#if UNITY_EDITOR
    [TagSelector]
#endif
    [SerializeField]
#pragma warning disable 0649
    ///<summary>Tag of Valkyrie</summary>
    private string m_ValkyrieTag;
#pragma warning restore
    public string ValkyrieTag { get { return m_ValkyrieTag; } }

    [SerializeField]
    ///<summary>Valkyrie Gameobject</summary>
    protected GameObject m_Valkyrie;

    [Header("Camera")]
    ///<summary>Player Camera</summary>
    [SerializeField]
    protected Camera m_playerCamera;
    ///<summary>Object which Camera needs for orientation</summary>
    [SerializeField]
    protected GameObject CineMachineObject;

    protected CinemachineFreeLook CineFreeLook;

    [Header("Audio")]
    public AudioSource m_Audio;


#pragma warning disable 0649
    [Header("Dash")]
    [SerializeField]
    private float m_Cooldown;
#pragma warning restore

    private float m_TimeStamp;

    [Header("Weapons")]
    ///<summary>Weapon gameobject of Player</summary>
    [SerializeField]
    protected GameObject[] m_WeaponsGO;

    ///<summary>Weapon Classes of Player</summary>
    protected AWeapon[] m_Weapons;

    [Header("Network")]
    [Tooltip("X and Z Coordinates will be multiplied with hit position. Y Coordinate will be set instead.")]
    public Vector3 m_swordHitVelocityMultiplier;


    ///<summary>index of current weapon</summary>
    private int weaponIndex = 0;
    ///<summary>Set index of current weapon</summary>
    public int WeaponIndex
    {
        get { return weaponIndex; }
        set
        {
            if (value >= m_Weapons.Length)
            {
                value = m_Weapons.Length - 1;
            }
            else if (value <= 0)
            {
                value = 0;
            }

            PreviousWeaponIndex = weaponIndex;

            weaponIndex = value;
        }
    }

    public int PreviousWeaponIndex { get; protected set; }

    ///<summary>get current weapon</summary>
    public AWeapon GetCurrentWeapon { get { return m_Weapons[weaponIndex]; } }

    #region UI Variables
    [Header("UI")]
    ///<summary>Player UI</summary>
    [SerializeField]
    protected GameObject m_UI;
    ///<summary>Player Canvas</summary>
    [SerializeField]
    protected Canvas m_Canvas;
    ///<summary>Player HP Text</summary>
    [SerializeField]
    protected Text m_HPText;
    ///<summary>Player SP Text</summary>
    [SerializeField]
    protected Text m_SPText;    
    ///<summary>Player SP Text</summary>
    [SerializeField]
    protected Text m_TimeText;
    ///<summary>Start Button</summary>
    [SerializeField]
    protected Button m_StartButton;
    ///<summary>Cross Hair Image</summary>
    [SerializeField]
    protected Image m_CrossHair;
    /// <summary>UI Text of Ammo</summary>
    [SerializeField]
    protected Text m_AmmoText;
    /// <summary>UI Text of Ammo</summary>
    public Text AmmoTextBox { get { return m_AmmoText; } }
    /// <summary>Pause Script</summary>
    [SerializeField]
    protected GameObject m_Pause;
    #endregion

    ///<summary>local time of current Round</summary>
    private float m_localRoundTime;
    ///<summary>Round manager in this scene</summary>
    private RoundManager roundManager;
    ///<summary>Round manager in this scene (Lazy loading)</summary>
    protected RoundManager RoundManager
    {
        get
        {
            if (!isServer)
            {
                Debug.LogWarning("Round Manager is only available for Server!!!", this.gameObject);
                return null;
            }
            if (roundManager == null)
            {
                // get gameobject
                GameObject go = GameObject.Find("RoundManager");
                // set roundmanager
                roundManager = go.GetComponent<RoundManager>();
            }
            return roundManager;
        }
    }


    #region Game Variables
    /// <summary>Max HP variable (DO NOT USE! USE <see cref="MaxHP"/> INSTEAD)</summary>
    [SyncVar]
    private float maxHP = 150;
    /// <summary>Max HP property (Everyone can get, only Server can set in <see cref="SetMaxHP(float)"/>)</summary>
    public float MaxHP
    {
        get { return maxHP; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Max HP to " + value, this.gameObject);
                return;
            }

            BeforeMaxHPChanged(value);
            maxHP = value;
            AfterMaxHPChanged();
            // change UI Text
            RpcChangeTextHP( currentHP, maxHP);
        }
    }

    /// <summary>Current HP variable (DO NOT USE! USE <see cref="CurrentHP"/> INSTEAD)</summary>
    [SyncVar]
    private float currentHP;
    /// <summary>Current HP property (Everyone can get, only Server can set is <see cref="SetCurrentHP(float)"/></summary>
    public float CurrentHP
    {
        get { return currentHP; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set current HP to " + value, this.gameObject);
                return;
            }

            value = BeforeCurrentHPChanged(value);
            currentHP = value;
            AfterCurrentHPChanged();
            // change UI Text
            RpcChangeTextHP((int)currentHP, maxHP);
        }
    }

    /// <summary>Max SP variable (DO NOT USE! USE <see cref="MaxSP"/> INSTEAD)</summary>
    [SyncVar]
    private float maxSP = 100;
    /// <summary>Max SP property (Everyone can get, only Server can set in <see cref="SetMaxSP(float)"/>)</summary>
    public float MaxSP
    {
        get { return maxSP; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set current SP to " + value, this.gameObject);
                return;
            }

            BeforeMaxSPChanged(value);
            maxSP = value;
            AfterMaxSPChanged();
            // change UI Text
            RpcChangeTextSP((int)currentSP, maxSP);

        }
    }

    /// <summary>Current SP variable (DO NOT USE! USE <see cref="CurrentSP"/> INSTEAD)</summary>
    [SyncVar]
    private float currentSP;
    /// <summary>Current SP property (Everyone can get, only Server can set in <see cref="SetCurrentSP(float)"/>)</summary>
    public float CurrentSP
    {
        get { return currentSP; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set current SP to " + value, this.gameObject);
                return;
            }

            value = BeforeCurrentSPChanged(value);
            currentSP = value;
            AfterCurrentSPChanged();
            // change UI Text
            RpcChangeTextSP((int)currentSP, maxSP);
        }
    }

    /// <summary>Kill count variable (DO NOT USE! USE <see cref="KillCount"/> INSTEAD)</summary>
    [SyncVar]
    private int killCount = 0;
    /// <summary>Kill count property (Everyone can get, only Server can set in <see cref="SetKillCount(int)"/>)</summary>
    public int KillCount
    {
        get { return killCount; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Kill count to " + value, this.gameObject);
                return;
            }

            BeforeKillCountPlayerChanged(value);
            killCount = value;
            AfterKillCountPlayerChanged();
        }

    }

    /// <summary>Death count variable (DO NOT USE! USE <see cref="DeathCount"/> INSTEAD)</summary>
    [SyncVar]
    private int deathCount = 0;
    /// <summary>Death count property (Everyone can get, only Server can set in <see cref="SetDeathCount(int)"/>)</summary>
    public int DeathCount
    {
        get { return deathCount; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Death count to " + value, this.gameObject);
                return;
            }

            BeforeDeathCountChanged(value);
            deathCount = value;
            AfterDeathCountChanged();
        }

    }

    /// <summary>Default variable of Player armor. 
    /// If you want to change the current armor of player use <see cref="CurrentArmor"/> instead. (DO NOT USE! USE <see cref="PlayerArmor"/> INSTEAD)</summary>
    [SyncVar]
    private float playerArmor = 1f;
    /// <summary>Player Armor property. Please change current armor in <see cref="CurrentArmor"/>. (Everyone can get, only Server can set in <see cref="SetDefaultPlayerArmor(float)"/>)</summary>
    public float PlayerArmor
    {
        get { return playerArmor; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set default value of Player Armor to " + value, this.gameObject);
                return;
            }

            BeforeDefaultPlayerArmorChanged(value);
            playerArmor = value;
            AfterDefaultPlayerArmorChanged();
        }
    }

    /// <summary>Default variable of Chaser armor. 
    /// If you want to change the current armor of chaser use <see cref="CurrentArmor"/> instead. (DO NOT USE! USE <see cref="ChaserArmor"/> INSTEAD)</summary>
    [SyncVar]
    private float chaserArmor = 60f;
    /// <summary>Chaser Armor property. Please change current armor in <see cref="CurrentArmor"/>. (Everyone can get, only Server can set in <see cref="SetDefaultChaserArmor(float)"/>)</summary>
    public float ChaserArmor
    {
        get { return chaserArmor; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set default value of chaser Armor to " + value, this.gameObject);
                return;
            }

            BeforeDefaultChaserArmorChanged(value);
            chaserArmor = value;
            AfterDefaultChaserArmorChanged();
        }

    }

    /// <summary>Current Armor variable (DO NOT USE! USE <see cref="CurrentArmor"/> INSTEAD)</summary>
    [SyncVar]
    private float currentArmor;
    /// <summary>Current Armor property (Everyone can get, only Server can set in <see cref="SetCurrentArmor(float)"/>)</summary>
    public float CurrentArmor
    {
        get
        {
            return currentArmor;
        }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set current armor to " + value, this.gameObject);
                return;
            }
            BeforeCurrentArmorChanged(value);
            currentArmor = value;
            AfterCurrentArmorChanged();
        }
    }

    /// <summary>Current sp regeneration variable (DO NOT USE! USE <see cref="RegenSP"/> INSTEAD)</summary>
    // regenSP by Max Poppicht
    [SyncVar]
    private float regenSP = SpRegenDefault;
    public float RegenSP
    {
        get
        {
            return regenSP;
        }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set current SP regen to " + value, this.gameObject);
                return;
            }

            regenSP = value;
        }
    }
    
    private float SPRegenMultiplier { get { return Time.deltaTime * 1.1f; } }

    /// <summary>How often was player Chaser</summary>
    [SyncVar]
    private int chaserCount = 0;
    /// <summary>Current Chaser count property (Everyone can get, only Server can set)</summary>
    public int ChaserCount
    {
        get { return chaserCount; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Chaser count to " + value, this.gameObject);
                return;
            }
            BeforeChaserCountChanged(value);
            chaserCount = value;
            AfterChaserCountChanged();
        }
    }

    /// <summary>True if player was chaser last round</summary>
    [SyncVar]
    private bool wasChaserLastRound = false;
    /// <summary>Was Chaser last round property (Everyone can get, only Server can set)</summary>
    public bool WasChaserLastRound
    {
        get { return wasChaserLastRound; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set was chaser last round to " + value, this.gameObject);
                return;
            }
            BeforeLastRoundChaserChanged(value);
            wasChaserLastRound = value;
            AfterLastRoundChaserChanged();
        }
    }

    /// <summary>Chaser variable (DO NOT USE! USE <see cref="IsChaser"/> INSTEAD)</summary>
    [SyncVar]
    private bool isChaser = false;
    /// <summary>Chaser property (Everyone can get, only Server can set in <see cref="SetChaser(bool)"/>)</summary>
    public bool IsChaser
    {
        get { return isChaser; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Chaser mode to " + value, this.gameObject);
                return;
            }
            BeforeChaserChanged(value);
            isChaser = value;
            AfterChaserChanged();
        }
    }

    /// <summary>Chaser kill count variable (DO NOT USE! USE <see cref="ChaserKillCount"/> INSTEAD)</summary>
    [SyncVar]
    private int chaserKillCount = 0;
    /// <summary>Chaser kill count Property</summary>
    public int ChaserKillCount
    {
        get { return chaserKillCount; }
        private set
        {
            if (!isServer)
            {
                Debug.Log("Client tried to set Chaser kill count to " + value, this.gameObject);
                return;
            }

            BeforeKillCountChaserChanged(value);
            chaserKillCount = value;
            AfterKillCountChaserChanged();
        }
    }
    #endregion

    [SyncVar]
    public bool wannaPlay;

    /// <summary>Current round time property</summary>
    public float LocalRoundTime
    {
        get { return m_localRoundTime; }
        set
        {
            if (value <= 0)
                m_localRoundTime = 0;
            else
                m_localRoundTime = value;

            // change text
            m_TimeText.text = ((int)m_localRoundTime).ToString();
        }
    }

    #region Override Functions
    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //    Initialize();
    //}

    public override void OnStartClient()
    {
        base.OnStartClient();
        Initialize();
    }
    #endregion

    #region Abstract Functions
    #endregion

    #region Public Functions
    #region Set specific values
    /// <summary>
    /// Set new max HP using Property (<see cref="MaxHP"/>)
    /// </summary>
    /// <param name="_newMaxHP">new max HP of player</param>
    public void SetMaxHP (float _newMaxHP)
    {
        MaxHP = _newMaxHP;
    }

    /// <summary>
    /// Set new current HP using Property (<see cref="CurrentHP"/>)
    /// </summary>
    /// <param name="_newCurrentHP">new current HP of player</param>
    public void SetCurrentHP (float _newCurrentHP)
    {
        CurrentHP = _newCurrentHP;
    }

    /// <summary>
    /// Set new current HP using Property (<see cref="CurrentHP"/>)
    /// </summary>
    /// <param name="_damage">Damage taken</param>
    /// <param name="_enemy">Player who dealt damage</param>
    public void GetDamage(float _damage, AEntity _enemy)
    {
        // when enemy is chaser multiply damage
        if (_enemy.IsChaser)
        {
            _damage *= Chaser.DamageMultiplier;
        }
        CurrentHP -= _damage / CurrentArmor;

        // check if dead
        if (CurrentHP <= 0)
        {
            DeathCount++;
            _enemy.KillCount++;
        }
    }

    /// <summary>
    /// Set new max SP using Property (<see cref="MaxSP"/>).
    /// </summary>
    /// <param name="_newMaxSP">new max SP of player</param>
    public void SetMaxSP (float _newMaxSP)
    {
        MaxSP = _newMaxSP;
    }

    /// <summary>
    /// Set new current SP using Property (<see cref="CurrentSP"/>)
    /// </summary>
    /// <param name="_newCurrentSP">new current SP of player</param>
    public void SetCurrentSP (float _newCurrentSP)
    {
        CurrentSP = _newCurrentSP;
    }

    public void SetReducedSP (float _reduceSP)
    {
        CurrentSP = CurrentSP - _reduceSP;
    }

    /// <summary>
    /// Set new kill count using Property (<see cref="KillCount"/>)
    /// </summary>
    /// <param name="_newKillCount">new kill count of player</param>
    public void SetKillCount (int _newKillCount)
    {
        KillCount = _newKillCount;
    }

    /// <summary>
    /// Set new death count using Property (<see cref="DeathCount"/>)
    /// </summary>
    /// <param name="_newDeathCount">new death count of player</param>
    public void SetDeathCount (int _newDeathCount)
    {
        DeathCount = _newDeathCount;
    }

    /// <summary>
    /// Set new player default player armor using Property (<see cref="PlayerArmor"/>).
    /// Change Current armor in <see cref="SetCurrentArmor(float)"/>
    /// </summary>
    /// <param name="_newDefaultPlayerArmor">new player default player armor value</param>
    public void SetDefaultPlayerArmor (float _newDefaultPlayerArmor)
    {
        PlayerArmor = _newDefaultPlayerArmor;
    }

    /// <summary>
    /// Set new player default chaser armor using Property (<see cref="ChaserArmor"/>)
    /// Change Current armor in <see cref="SetCurrentArmor(float)"/>
    /// </summary>
    /// <param name="_newDefaultPlayerArmor">new player default chaser armor value</param>
    public void SetDefaultChaserArmor(float _newDefaultChaserArmor)
    {
        ChaserArmor = _newDefaultChaserArmor;
    }

    /// <summary>
    /// Set new current Armor using Property (<see cref="CurrentArmor"/>)
    /// </summary>
    /// <param name="_newCurrentArmor">new current Armor of player</param>
    public void SetCurrentArmor (float _newCurrentArmor)
    {
        CurrentArmor = _newCurrentArmor;
    }

    /// <summary>
    /// Set Chaser using Property (<see cref="IsChaser"/>) This Function has to be called every round for each player!!!
    /// </summary>
    /// <exception cref="System.NotImplementedException"/>
    /// <param name="_isChaser">chaser value</param>
    public void SetChaser(bool _isChaser)
    {
        // Check if value is the same and if new variable is false
        if (!IsChaser && !_isChaser)
        {
            // set chaser last round variable to false
            WasChaserLastRound = false;
            return;
        }
        // if both values are true, something went wrong
        if (IsChaser && _isChaser)
        {
            Debug.LogException(new System.NotImplementedException("This message should not appear! This Player is chaser two times in a row"), this.gameObject);
        }

        // set new chaser status
        IsChaser = _isChaser;

        // if last round player was no Chaser and now is increase chaser counter
        if (_isChaser)
            ChaserCount++;
        // if last round player was Chaser and now is not set last round chaser to true;
        else
            WasChaserLastRound = true;
    }
    #endregion

    /// <summary>
    /// Resets values for new round (current HP, current SP)
    /// </summary>
    public void NewRoundReset()
    {
        CurrentHP = MaxHP;
        CurrentSP = MaxSP;
        RpcResetWeapons();
    }

    /// <summary>
    /// Resets values for new game (current HP, current SP, killcount, Deathcount, current Armor)
    /// </summary>
    public void NewGameReset()
    {
        CurrentHP = MaxHP;
        CurrentSP = MaxSP;
        KillCount = 0;
        DeathCount = 0;
        CurrentArmor = 0;
    }
    #endregion

    #region Private Functions
    #endregion

    #region Protected Functions    
    
    #endregion

    #region Public static Functions
    #endregion

    #region Private static Functions
    #endregion

    #region Virtual Functions
    #region Max HP Changed
    /// <summary>
    /// Function is called before players Max HP is set
    /// </summary>
    /// <param name="_newValue">new Value of variable</param>
    public virtual void BeforeMaxHPChanged(float _newValue) { }
    /// <summary>
    /// Function is called after players Max HP was set
    /// </summary>
    public virtual void AfterMaxHPChanged() { }
    #endregion

    #region Current HP Changed
    /// <summary>
    /// Function is called before players Current HP is set
    /// </summary>
    /// <param name="_newValue">new Value of variable</param>
    public virtual float BeforeCurrentHPChanged(float _newValue)
    {
        // check if player is dead next frame
        if (CurrentHP > 0 && _newValue <= 0)
        {
            RpcTeleport(SpawnpointHandler.NextDeadPoint(), ETP.DEADTP);
        }
        if (_newValue <= 0)
            return 0;
        else
            return _newValue;
    }
    /// <summary>
    /// Function is called after players Current HP was set
    /// </summary>
    public virtual void AfterCurrentHPChanged() { }
    #endregion

    #region Max Sp Changed
    /// <summary>
    /// Function is called before player Max SP is set
    /// </summary>
    /// <param name="_newValue">new Value of variable</param>
    public virtual void BeforeMaxSPChanged(float _newValue) { }
    /// <summary>
    /// Function is called after players Max SP was set
    /// </summary>
    public virtual void AfterMaxSPChanged() { }
    #endregion

    #region Current SP Changed
    /// <summary>
    /// Function is called before players Current SP is set
    /// </summary>
    /// <param name="_newValue">new Value of variable</param>
    public virtual float BeforeCurrentSPChanged(float _newValue)
    {
        if (_newValue <= 0)
            return 0;
        else
            if (_newValue > MaxSP)
            return MaxSP;
        else
            return _newValue;
    }
    /// <summary>
    /// Function is called after players Current SP was set
    /// </summary>
    public virtual void AfterCurrentSPChanged() { }
    #endregion

    #region Kill Count Player Changed
    /// <summary>
    /// Function is called before players kill count is set (Chaser kills Player)
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeKillCountPlayerChanged(int _newValue) { }
    /// <summary>
    /// Function is called after players kill count was set (Chaser kills Player)
    /// </summary>
    public virtual void AfterKillCountPlayerChanged() { }
    #endregion

    #region Death Count Changed
    /// <summary>
    /// Function is called before players death count is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeDeathCountChanged(int _newValue) { }
    /// <summary>
    /// Function is called after players kill count was set
    /// </summary>
    public virtual void AfterDeathCountChanged() { }
    #endregion

    #region Default Armor Changed    
    /// <summary>
    /// Function is called before players default value for Player Armor is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeDefaultPlayerArmorChanged(float _newValue) { }
    /// <summary>
    /// Function is called after players default value for Player Armor was set
    /// </summary>
    public virtual void AfterDefaultPlayerArmorChanged() { }
    /// <summary>
    /// Function is called before players default value for Chaser Armor is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeDefaultChaserArmorChanged(float _newValue) { }
    /// <summary>
    /// Function is called after players default value for Chaser Armor was set
    /// </summary>
    public virtual void AfterDefaultChaserArmorChanged() { }
    #endregion

    #region Current Armor changed
    /// <summary>
    /// Function is called before players Current armor is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeCurrentArmorChanged(float _newValue) { }
    /// <summary>
    /// Function is called after playes Current armor was set
    /// </summary>
    public virtual void AfterCurrentArmorChanged()
    {
        // check if armor was given right

        // check if player is chaser
        if (IsChaser)
        {
            // check if current armor equals Chaser armor
            if (CurrentArmor != ChaserArmor)
            {
                // if not throw new exception
                Debug.LogException(
                    new System.NotImplementedException(
                        "In Chaser Mode, Current Armor has to be "
                        + ChaserArmor
                        + ". Your Armor is "
                        + CurrentArmor
                        + ".")
                        );

            }
        }

        // check is player is not cheaser
        else if (!IsChaser)
        {
            // check if current armor equals Player armor
            if (CurrentArmor != PlayerArmor)
            {
                // if not throw new exception
                throw new System.NotImplementedException(
                    "In Player Mode, Current Armor has to be "
                    + PlayerArmor
                    + ". Your Armor is "
                    + CurrentArmor
                    + ".");
            }

        }
    }
    #endregion

    #region Chaser Count Changed
    /// <summary>
    /// Function is called before players Chaser count is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeChaserCountChanged(int _newValue) { }
    /// <summary>
    /// Function is called after players Chaser count was set
    /// </summary>
    public virtual void AfterChaserCountChanged() { }
    #endregion

    #region Last Round Chaser changed
    /// <summary>
    /// Function is called before players last round Chaser value is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeLastRoundChaserChanged(bool _newValue) { }
    /// <summary>
    /// Function is called after players last round Chaser value was set
    /// </summary>
    public virtual void AfterLastRoundChaserChanged() { }
    #endregion

    #region Chaser changed
    /// <summary>
    /// Function is called before players Chaser status is set
    /// </summary>
    /// <param name="_newValue">New value of Variable</param>
    public virtual void BeforeChaserChanged(bool _newValue) { }
    /// <summary>
    /// Function is called after players Chaser status was set
    /// Set Armor
    /// </summary>
    public virtual void AfterChaserChanged()
    {
        if (IsChaser)
        {
            CurrentArmor = ChaserArmor;
        }
        else
        {
            CurrentArmor = PlayerArmor;
        }
    }
    #endregion

    #region Kill Count Chaser Changed
    /// <summary>
    /// Function is called before players Kill Count Chaser is set (Player kills Chaser)
    /// </summary>
    /// <param name="_newValue">New value of variable</param>
    public virtual void BeforeKillCountChaserChanged(int _newValue) { }
    /// <summary>
    /// Function is called after players Kill Count Chaser was set (Player kills Chaser)
    /// </summary>
    public virtual void AfterKillCountChaserChanged() { }
    #endregion

    #region Update
    protected virtual void Update()
    {

        if (!isServer) return;

        // regenerate SP
        if (Time.time > m_TimeStamp) CurrentSP += RegenSP * SPRegenMultiplier;

        // if player fell of the stage reset position
        if (transform.position.y <= -500)
        {
            Debug.Log("Teleport");
            RpcSetVelocity(Vector3.zero);
            if (wannaPlay)
            {
                if (IsChaser)
                {
                    RpcTeleport(SpawnpointHandler.NextChaserpoint(), ETP.CHASERTP);
                }
                else if (CurrentHP <= 0)
                {
                    RpcTeleport(SpawnpointHandler.NextDeadPoint(), ETP.DEADTP);
                }
                else
                {
                    RpcTeleport(SpawnpointHandler.NextSpawnpointPlayer(), ETP.HUNTEDTP);
                }
            }
            else
            {
                RpcTeleport(SpawnpointHandler.NextLobbypoint(), ETP.LOBBYTP);
            }
        }

    }
    #endregion
    #endregion

    #region Network
    #region RPC

    /// <summary>
    /// Set Velocity of player
    /// </summary>
    /// <param name="_velocity">new velocity</param>
    [ClientRpc]
    public void RpcSetVelocity(Vector3 _velocity)
    {
        m_rigidbody.velocity = _velocity;
    }
    /// <summary>
    /// Set new position of player (RPC Call)
    /// </summary>
    /// <param name="_newPosition">The new position.</param>
    [ClientRpc]
    public void RpcTeleport(Vector3 _newPosition, ETP _tpType)
    {
        gameObject.transform.position = _newPosition;

        switch (_tpType)
        {
            case ETP.LOBBYTP:
                LobbyUINotReady();
                break;
            case ETP.HUNTEDTP:
                GameUI();
                break;
            case ETP.CHASERTP:
                GameUI();
                break;
            case ETP.DEADTP:
                GameUI();
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Set new Position of Player (Server Only)
    /// </summary>
    /// <param name="_newPosition">The new position.</param>
    public void ServerTeleport(Vector3 _newPosition)
    {
        gameObject.transform.position = _newPosition;
    }
    /// <summary>
    /// Changes the text.
    /// </summary>
    /// <param name="_text">Text UI Text to change</param>
    /// <param name="_currentValue">Current value</param>
    /// <param name="_maxValue">Maximum value</param>
    [ClientRpc]
    protected void RpcChangeTextHP(float _currentValue, float _maxValue)
    {
        // set new UI Text
        m_HPText.text = _currentValue + " / " + _maxValue;
    }

    /// <summary>
    /// Changes the text.
    /// </summary>
    /// <param name="_text">Text UI Text to change</param>
    /// <param name="_currentValue">Current value</param>
    /// <param name="_maxValue">Maximum value</param>
    [ClientRpc]
    protected void RpcChangeTextSP(float _currentValue, float _maxValue)
    {
        // set new UI Text
        m_SPText.text = _currentValue + " / " + _maxValue;
    }

    /// <summary>
    /// Change Color of current chaser and last chaser
    /// </summary>
    /// <param name="_chaser">current chaser</param>
    /// <param name="_lastChaser">chaser of last round</param>
    [ClientRpc]
    public void RpcSetChaserColor(GameObject _chaser, GameObject _lastChaser)
    {
        if (_lastChaser != null)
            _lastChaser.GetComponent<PlayerEntity>().m_body.GetComponent<Renderer>().material = PlayerMaterial;
        else
            Debug.Log("last chaser is null");

        if (_chaser != null)
            _chaser.GetComponent<PlayerEntity>().m_body.GetComponent<Renderer>().material = ChaserMaterial;
        else
            Debug.Log("current chaser is null");
    }

    [ClientRpc]
    public void RpcDeActivateValkyrie(GameObject _chaser, GameObject _lastChaser)
    {
        if (_lastChaser != null)
            _lastChaser.GetComponent<PlayerEntity>().m_Valkyrie.SetActive(false);
        else
            Debug.Log("last chaser is null");

        if (_chaser != null)
            _chaser.GetComponent<PlayerEntity>().m_Valkyrie.SetActive(true);
        else
            Debug.Log("current chaser is null");
    }

    /// <summary>
    /// Reset Color of current chaser
    /// </summary>
    /// <param name="_chaser">current chaser</param>
    [ClientRpc]
    public void RpcChangeToDefaultColor(params GameObject[] _players)
    {
        foreach (GameObject go in _players)
        {
            if (go == null) continue;
            PlayerEntity pe = go.GetComponent<PlayerEntity>();
            if (pe == null) continue;

            pe.m_body.GetComponent<Renderer>().material = DefaultMaterial;

        }

    }

    /// <summary>
    /// Set next round time and save it local
    /// </summary>
    /// <param name="_roundTime">time for next round</param>
    [ClientRpc]
    public void RpcSetRoundTime(float _roundTime)
    {
        LocalRoundTime = _roundTime;
    }

    [ClientRpc]
    public void RpcSetWeaponActiveState(GameObject _player, bool _setActive, int _index)
    {
        PlayerEntity pe = _player.GetComponent<PlayerEntity>();
        pe.m_WeaponsGO[_index].SetActive(_setActive);
    }

    [ClientRpc]
    public void RpcSetGOActiveState(GameObject _gameObject, bool _setActive)
    {
        if (_gameObject != null)
            _gameObject.SetActive(_setActive);
    }

    /// <summary>
    /// Change Start Button Text
    /// </summary>
    /// <param name="_text">new Text of Button</param>
    [ClientRpc]
    public void RpcChangeStartButtonTextToStart()
    {
        m_StartButton.GetComponentInChildren<Text>().text = "Start!";
    }

    [ClientRpc]
    public void RpcAddForce(Vector3 _directionFromHit)
    {
        wasHit = true;
        hitTimer = HitTimer;
        _directionFromHit.x *= m_swordHitVelocityMultiplier.x;
        _directionFromHit.y = m_swordHitVelocityMultiplier.y;
        _directionFromHit.z *= m_swordHitVelocityMultiplier.z;
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.AddForce(_directionFromHit, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Reset Ammo
    /// </summary>
    [ClientRpc]

    public void RpcResetWeapons()
    {
        foreach (AWeapon weapon in m_Weapons)
        {
            if (weapon.GetMainWeapon == AWeapon.MainWeaponType.GUN)
            {
                weapon.ResetAmmo();
            }
        }
        GetCurrentWeapon.SetAmmoText();
    }
    #endregion

    #region Command

    /// <summary>
    /// When Player hits another Player deal damage
    /// </summary>
    /// <param name="_origin">Start position</param>
    /// <param name="_direction">direction of shot</param>
    [Command]
    public void CmdWeapon(Vector3 _origin, Vector3 _direction, AWeapon.WeaponName _weaponName)
    {
        // if player dont have HP left return
        if (CurrentHP <= 0)
            return;

        // if round is over no damage will be applied
        if (RoundManager.CurrentRoundTime <= 0)
            return;

        // return if only 1 second passed in Round
        if (RoundManager.NextRoundTime - RoundManager.CurrentRoundTime <= 1) return;

        Ray ray = new Ray(_origin, _direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.collider.gameObject.tag, hit.collider.gameObject);
            // check if hit object is player or Valkyrie
            if (hit.collider.gameObject.tag != DamageTag)
                if (hit.collider.gameObject.tag != ValkyrieTag)
                    return;

            PlayerEntity p = null;

            if (hit.collider.gameObject.tag == DamageTag)
                p = hit.collider.gameObject.GetComponentInParent<Rig>().GetComponentInParent<PlayerEntity>();
            else
                p = hit.collider.gameObject.GetComponentInParent<PlayerEntity>();

            // get Playerentity (GetParent because Capsule is hit and Capsules parent has playerentity)

            // check if chaserstatus is not equal
            if (IsChaser != p.IsChaser)
            {
                p.GetDamage(WeaponDamage.Damage(_weaponName), p);
            }
        }
    }

    [Command]
    public void CmdSword(AWeapon.WeaponName _weaponName, GameObject _hit, Vector3 _forward)
    {
        // if player dont have HP left return
        if (CurrentHP <= 0)
            return;

        // if round is over no damage will be applied
        if (RoundManager.CurrentRoundTime <= 0)
            return;

        // return if only 1 second passed in Round
        if (RoundManager.NextRoundTime - RoundManager.CurrentRoundTime <= 1) return;

        if (_hit == null) return;

        PlayerEntity pHit = null;
        Debug.Log(_hit.tag, _hit);
        if (_hit.tag == DamageTag)
            pHit = _hit.GetComponentInParent<Rig>().GetComponentInParent<PlayerEntity>();
        else
            pHit = _hit.GetComponentInParent<PlayerEntity>();

        if (pHit == null) return;

        pHit.GetDamage(WeaponDamage.Damage(_weaponName), this);
        pHit.RpcAddForce(_forward);
    }
    


    /// <summary>
    /// Tell Server if player wants to join next round or retreat
    /// </summary>
    /// <param name="_joinGame">true: join next round || false: do not join next round</param>
    [Command]
    public void CmdStartGame(bool _joinGame)
    {
        // If true add to player list
        if (_joinGame)
        {
            MyNetworkManager.AddPlayer(this.gameObject);
            wannaPlay = true;
        }
        // if false remove player from list
        else
        {
            MyNetworkManager.RemovePlayer(this.gameObject);
            wannaPlay = false;
        }
    }

    /// <summary>
    /// Set Sp regeneration
    /// </summary>
    /// <param name="_regenSP">new SP regeneration value</param>
    [Command]
    public void CmdSetRegenSP(float _regenSP)
    {
        RegenSP = _regenSP;
    }

    /// <summary>
    /// Set WannaPlay variable (see <see cref="wannaPlay"/>)
    /// </summary>
    /// <param name="_wannaplay">new value</param>
    [Command]
    public void CmdSetWannaPlay(bool _wannaplay)
    {
        wannaPlay = _wannaplay;
    }

    /// <summary>
    /// Set Gameobject isactive state
    /// </summary>
    /// <param name="_setActive">active state</param>
    /// <param name="_go">all gameobjects that have to change</param>
    [Command]
    public void CmdSetGOActiveState(bool _setActive, int _index)
    {
        foreach (PlayerEntity pe in MyNetworkManager.AllPlayers)
        {
            pe.RpcSetWeaponActiveState(this.gameObject, _setActive, _index);
        }
    }

    /// <summary>
    /// Reduces SP when character dashes
    /// </summary>
    [Command]
    public void CmdDash()
    {
        CurrentSP -= DashConsumption;
        m_TimeStamp = Time.time + m_Cooldown;
    }

    /// <summary>
    /// Reduces SP when character do a walljump
    /// </summary>
    [Command]
    public void CmdWalljump()
    {
        CurrentSP -= WalljumpConsumption;
    }
    #endregion

    #endregion

    /// <summary>
    /// Initializes this instance
    /// </summary>
    protected virtual void Initialize()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        MyNetworkManager.ReloadPlayers();
        NetworkManagerHUD hud = GameObject.Find("Network Manager").GetComponent<NetworkManagerHUD>();
        hud.showGUI = false;

        // get Weapon Class
        m_Weapons = new AWeapon[m_WeaponsGO.Length];

        for (int i = 0; i < m_Weapons.Length; i++)
        {
            m_WeaponsGO[i].SetActive(true);
            m_Weapons[i] = m_WeaponsGO[i].GetComponent<AWeapon>();

            if (i != 0) m_WeaponsGO[i].SetActive(false);
        }
    }

    /// <summary>
    /// Start Button function
    /// </summary>
    public void StartGame()
    {
        if (m_StartButton.GetComponentInChildren<Text>().text == "Start!")
        {
            // check if server pressed button
            if (isServer)
            {
                // check if other player clicked start
                if (MyNetworkManager.AllPlayersWannaPlay.Count + 1 < RoundManager.minimumPlayers) return;
            }
            m_StartButton.GetComponentInChildren<Text>().text = "Cancel";

            // set UI active
            LobbyUIReady();

            // Tell Server to join game next round
            CmdStartGame(true);
        }
        else
        {
            m_StartButton.GetComponentInChildren<Text>().text = "Start!";

            // set ui inactive
            LobbyUINotReady();

            // Tell Server to not join game next round
            CmdStartGame(false);
        }
    }

    #region UI Themes

    /// <summary>
    /// UI in Lobby but not ready
    /// </summary>
    protected void LobbyUINotReady()
    {
        m_UI.gameObject.SetActive(true);
        m_HPText.gameObject.SetActive(false);
        m_SPText.gameObject.SetActive(false);
        m_CrossHair.gameObject.SetActive(false);
        m_TimeText.gameObject.SetActive(false);
        m_AmmoText.gameObject.SetActive(false);
        m_StartButton.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CmdSetWannaPlay(false);
    }

    /// <summary>
    /// UI in Lobby and ready
    /// </summary>
    protected void LobbyUIReady()
    {
        if (!isLocalPlayer) return;
        m_UI.gameObject.SetActive(true);
        m_HPText.gameObject.SetActive(true);
        m_SPText.gameObject.SetActive(true);
        m_CrossHair.gameObject.SetActive(true);
        m_TimeText.gameObject.SetActive(true);
        m_AmmoText.gameObject.SetActive(true);
        m_StartButton.gameObject.SetActive(true);

        // set ammo text
        m_AmmoText.GetComponentInChildren<Text>().text = GetCurrentWeapon.AmmoText;


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CmdSetWannaPlay(true);
    }

    /// <summary>
    /// UI when playing
    /// </summary>
    protected void GameUI()
    {
        if (!isLocalPlayer) return;
        m_UI.gameObject.SetActive(true);
        m_StartButton.GetComponentInChildren<Text>().text = "Cancel";
        m_StartButton.gameObject.SetActive(false);
        m_HPText.gameObject.SetActive(true);
        m_SPText.gameObject.SetActive(true);
        m_CrossHair.gameObject.SetActive(true);
        m_AmmoText.gameObject.SetActive(true);
        m_TimeText.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CmdSetWannaPlay(true);
    }

    /// <summary>
    /// UI when in no lobby
    /// </summary>
    protected void JoinUI()
    {
        if (!isLocalPlayer) return;
        m_UI.gameObject.SetActive(true);
        m_StartButton.gameObject.SetActive(false);
        m_HPText.gameObject.SetActive(false);
        m_SPText.gameObject.SetActive(false);
        m_CrossHair.gameObject.SetActive(false);
        m_TimeText.gameObject.SetActive(false);
        m_AmmoText.gameObject.SetActive(false);
        m_UI.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CmdSetWannaPlay(false);
    }

    #endregion
}
