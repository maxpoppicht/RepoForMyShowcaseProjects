using Assets.Scripts.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Max Poppicht
// Tobias Stroedicke
public class PlayerEntity : AEntity
{
    bool isgrounded;

    float distancetoground;
 
    Vector3 _direction;

    Vector3 walljumpDir;

    Animator Anim;
    
    GameObject lastwalljumped;

    int jumpcount;

    [Header("Player Specified")]
    public float m_maxdashtime = 5f;

    public float m_dashstopspeed = 0.1f;

    public float m_currentdashtime = 0f;

    public float m_resetdashtime = 2f;

    public float m_isgrounedoffset = 0.1f;

    private float m_DefaultMovementSpeed;

    public float m_dashSpeed = 40f;

    public float cooldowntime = 1f;

    float timestamp;

    bool isdashing;

    float timesincelastcall;

    private bool isShooting; 

    private IEnumerator corountine;

    private bool m_PausePressed = false;
    private bool m_ResumePressed = false;

    private bool m_firePressed = false;

    private LineRenderer m_lineRenderer;

    ///<summary>Movement Speed of Player</summary>
    public float m_MovementSpeed;
    public float m_MovementSpeedChaser;
    ///<summary>Speed with which the Player can rotate</summary>
    public float m_RotationSpeed;
    ///<summary>The force with which the Player can Jump</summary>
    public float m_JumpForce;
    private float hitdistance;
    private int m_TracerCounter;
    private float WaitTimer;
    public float WaitTimerDefault;

    // Use this for initialization
    void Start ()
    {
        m_currentdashtime = m_maxdashtime;

        m_DefaultMovementSpeed = m_MovementSpeed;

        distancetoground = GetComponentInChildren<Collider>().bounds.extents.y;

        m_lineRenderer = GetComponent<LineRenderer>();

        Anim = GetComponent<Animator>();

        Anim.enabled = true;

    }

    // Update is called once per frame
    override protected void Update ()
    {
        base.Update();

        // Max Poppicht
        // decrease time for line renderer being enabled
        if (!isLocalPlayer)
        {
            WaitTimer -= Time.deltaTime;

            if (WaitTimer <= 0)
            {
                m_lineRenderer.enabled = false;
            }
        }

        //Only execute code beneath this if this is local player
        if (!isLocalPlayer)
            return;

        TimeCounter();

        isShooting = false;

        // Tobias Stroedicke
        // Pause
        #region Pause

        if (Input.GetAxisRaw("Pause") != 0)
        {
            if (!m_PausePressed && !m_ResumePressed)
            {
                if (!m_Pause.activeSelf)
                {
                    m_Pause.GetComponent<Pause>().CallPauseEnter();
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    m_PausePressed = true;
                    m_Pause.SetActive(true);
                }
                else
                {
                    PlayerResuming();
                    m_ResumePressed = true;
                }
            }
        }
        else
        {
            m_PausePressed = false;
            m_ResumePressed = false;
        }

        if (m_Pause.activeSelf)
        {
            return;
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Tobias Stroedicke
        #region Weapon

        bool weaponChanged = false;

        // change weapon via mouse scroll wheel
        float mousescroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (mousescroll != 0)
        {
            int oldIndex = WeaponIndex;

            if (mousescroll < 0)
                WeaponIndex--;
            else if (mousescroll > 0)
                WeaponIndex++;

            if (oldIndex != WeaponIndex)
            {
                weaponChanged = true;
                Debug.Log("Weapon " + WeaponIndex + ": " + GetCurrentWeapon.GetWeaponName);
            }

        }

        // change weapon via numbers
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int oldIndex = WeaponIndex;
            WeaponIndex = 0;
            if (oldIndex != WeaponIndex)
            {
                Debug.Log("Weapon " + WeaponIndex + ": " + GetCurrentWeapon.GetWeaponName);
                weaponChanged = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            int oldIndex = WeaponIndex;
            WeaponIndex = 1;
            if (oldIndex != WeaponIndex)
            {
                Debug.Log("Weapon " + WeaponIndex + ": " + GetCurrentWeapon.GetWeaponName);
                weaponChanged = true;
            }

        }

        // check if weapon was changed this frame
        if (weaponChanged)
        {
            // change Ammo Text
            AmmoTextBox.text = GetCurrentWeapon.AmmoText;

            // change active state of gameobject
            for (int i = 0; i < m_Weapons.Length; i++)
            {
                if (m_WeaponsGO[i].GetComponent<AWeapon>() == GetCurrentWeapon)
                    CmdSetGOActiveState(true, WeaponIndex);
                else
                    CmdSetGOActiveState(false, PreviousWeaponIndex);
            }
        }
        #endregion

        // Max Poppicht
        timesincelastcall += Time.deltaTime;

        // Calculates the grounded
        isgrounded = Physics.Raycast(transform.position, Vector3.down, distancetoground + m_isgrounedoffset);

        if (isgrounded == true)
        {
            lastwalljumped = null;
            jumpcount = 0;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Shoot
        // if Weapon has rapid fire
        if (GetCurrentWeapon.HasRapidFire)
        {
            if (Input.GetAxisRaw("Fire1") > 0)
                Shoot();
        }
        // if weapon does not have rapid fire
        else
        {
            if (Input.GetAxisRaw("Fire1") > 0)
            {
                if (!m_firePressed)
                {
                    m_firePressed = true;
                    Shoot();
                }
            }
            else
                m_firePressed = false;
        }

        var forward = m_playerCamera.transform.forward;
        forward.y = 0;
        var right = m_playerCamera.transform.right;
        right.y = 0;

        // Mouse Input
        Vector3 dir = Input.GetAxisRaw("Horizontal") * right +
            Input.GetAxisRaw("Vertical") * forward;

        // Max Poppicht
        // Checks for various things before setting direction to Walljump direction
        if(dir.x != 0 && dir.z != 0 && isgrounded)
        {
            walljumpDir = dir;
        }

        // Max Poppicht
        #region ---Animation---


        // Direction for 
        Vector2 animationDir;

        // Get Direction from Mouse input
        animationDir.x = Input.GetAxisRaw("Horizontal");
        animationDir.y = Input.GetAxisRaw("Vertical");



        //Walking Straight, forward
        if (animationDir.y > 0)
        {
            Anim.SetBool("isWalking", true);
        }
        else
        {
            Anim.SetBool("isWalking", false);
        }

        //Walking Left, Sidewards
        if (animationDir.x < 0)
        {
            Anim.SetBool("walkingLeft", true);
        }
        else
        {
            Anim.SetBool("walkingLeft", false);
        }

        //Walking Right, Sidewards
        if (animationDir.x > 0)
        {
            Anim.SetBool("walkingRight", true);
        }
        else
        {
            Anim.SetBool("walkingRight", false);
        }

        //Walking Back, Backwards
        if (animationDir.y < 0)
        {
            Anim.SetBool("walkingBack", true);
        }
        else
        {
            Anim.SetBool("walkingBack", false);
        }

        //Dash Animation
        if (m_MovementSpeed == m_dashSpeed)
        {
            Anim.SetBool("dashing", true);
        }
        else
        {
            Anim.SetBool("dashing", false);
        }

        #endregion

        
        // Move Method
        Move(dir);
        // Dash Method
        Dash(dir);

        // Max Poppicht
        // Checks for shooting to enable and disable the Line Renderer for Bullet Tracer 
        if (!isShooting)
        {
            if (m_lineRenderer.enabled == true)
            {
                WaitTimer -= Time.deltaTime;
                if (m_TracerCounter > 0 && WaitTimer < 0)
                {
                    m_lineRenderer.enabled = false;
                }
            }
        }

        // Tobias Stroedicke
        #region Knockback Timer and bool

        // if player was hit, set variable to false, so move function does not call and player can apply knockback
        if (wasHit)
        {
            if (hitTimer <= 0)
            {
                hitTimer = 0;
                wasHit = false;
            }
            else
                hitTimer -= Time.deltaTime;
        }

        #endregion

    }


    #region Override Functions
    /// <summary>
    /// Initialize Player and set important variables
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        if (!isServer)
            return;
        // set players current values
        SetCurrentHP(MaxHP);
        SetCurrentSP(MaxSP);
        SetCurrentArmor(PlayerArmor);
    }
    #endregion

    #region Movement
    // Max Poppicht
    /// <summary>
    /// Makes the Player Jump
    /// </summary>
    private void Jump()
    {
        if(isgrounded)
        {
            m_rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.VelocityChange);

        }
    }

    /// <summary>
    /// Move Player
    /// </summary>
    /// <param name="_direction">Direction of traveling</param>
    private void Move(Vector3 _direction)
    {
        // Tobias Stroedicke
        #region Look at
        m_lookAt.position = transform.position + m_playerCamera.transform.forward;

        Transform t = m_playerCamera.transform;
        Vector3 pos = (new Vector3(t.position.x, transform.position.y, t.position.z));
        t.position = pos;
        transform.LookAt(m_lookAt);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
        #endregion

        // Max Poppicht
        // Tobias Stroedicke
        #region Run
        if (isgrounded && !wasHit)
        {
            float f = IsChaser ? m_MovementSpeedChaser : m_MovementSpeed;
            Vector3 velocity = _direction.normalized * f;
            velocity.y = m_rigidbody.velocity.y;
            m_rigidbody.velocity = velocity;
            
        }
        #endregion
    }

    /// <summary>
    ///  Makes the Player Jump off walls 
    /// </summary>
    /// <param name="_direction"> Direction of traveling</param>
    // Max Poppicht
    private void WallJump(Vector3 _direction)
    {
        // Tobias Stroedicke
        float f = IsChaser ? m_MovementSpeedChaser : m_MovementSpeed;
        // Max Poppicht
        Vector3 velocity = _direction.normalized * f;
        velocity.y = m_rigidbody.velocity.y;
        m_rigidbody.velocity = velocity;
        jumpcount++;
        CmdWalljump();
    }

    // Max Poppicht
    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    /// <summary>
    /// Walljump
    /// </summary>
    /// <param name="collision"></param>
    // Max Poppicht
    private void OnCollisionStay(Collision collision)
    {
        // Player has to be in the air, and last wall can't be jumped twice
        if (!isgrounded && lastwalljumped != collision.gameObject)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                // Changes direction of multiple walljumps
                if(jumpcount % 2 == 0)
                {
                    WallJump(-walljumpDir);
                }
                else
                {
                    WallJump(walljumpDir);
                }
                // Adds force and saves last wall
                m_rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.VelocityChange);
                lastwalljumped = collision.gameObject;
            }
               
        }
    }

    /// <summary>
    ///  Makes the Player dash
    /// </summary>
    /// <param name="_direction"></param>
    // Max Poppicht
    private void Dash(Vector3 _direction)
    {
        // Checking for Cooldown as well as SP Amount
        if (Time.time > timestamp && CurrentSP > 10 && isgrounded)
        {
            // All 4 dashes are the same except for minor differences
            #region ---DASH RIGHT---
            if (Input.GetButtonDown("LeftShift")  && Input.GetButton("D_Key") || Input.GetButtonDown("D_Key") && Input.GetButton("LeftShift"))
            {
                // Cooldown
                timestamp = Time.time + cooldowntime;

                m_currentdashtime = 0.0f;

                isdashing = true;

                // Cooldown and Call
                if (timesincelastcall >= 1)
                {
                    if (isdashing)
                    {
                        CmdDash();
                    }
                    timesincelastcall = 0;
                }

                // Speed and distance of Dash
                if (m_currentdashtime < m_maxdashtime)
                {
                    m_MovementSpeed = m_dashSpeed;
                }
                isdashing = false;

            }
            #endregion

            #region ---DASH LEFT---
            if (Input.GetButtonDown("LeftShift") && Input.GetButton("A_Key") || Input.GetButtonDown("A_Key") && Input.GetButton("LeftShift"))
            {
                timestamp = Time.time + cooldowntime;

                m_currentdashtime = 0.0f;

                isdashing = true;

                if (timesincelastcall >= 1)
                {
                    if (isdashing)
                    {
                        CmdDash();
                    }
                    timesincelastcall = 0;
                }

                if (m_currentdashtime < m_maxdashtime)
                {
                    m_MovementSpeed = m_dashSpeed;
                }
                isdashing = false;

            }
            #endregion

            #region ---DASH FORWARD---
            if (Input.GetButtonDown("LeftShift") && Input.GetButton("W_Key") || Input.GetButtonDown("W_Key") && Input.GetButton("LeftShift"))
            {
                timestamp = Time.time + cooldowntime;

                m_currentdashtime = 0.0f;

                isdashing = true;

                if (timesincelastcall >= 1)
                {
                    if (isdashing)
                    {
                        CmdDash();
                    }
                    timesincelastcall = 0;
                }

                if (m_currentdashtime < m_maxdashtime)
                {
                    m_MovementSpeed = m_dashSpeed;
                }
                isdashing = false;

            }
            #endregion

            #region ---DASH BACKWARDS---
            if (Input.GetButtonDown("LeftShift") && Input.GetButton("S_Key") || Input.GetButtonDown("S_Key") && Input.GetButton("LeftShift"))
            {
                timestamp = Time.time + cooldowntime;

                m_currentdashtime = 0.0f;

                isdashing = true;

                if (timesincelastcall >= 1)
                {
                    if (isdashing)
                    {
                        CmdDash();
                    }
                    timesincelastcall = 0;
                }

                if (m_currentdashtime < m_maxdashtime)
                {
                    m_MovementSpeed = m_dashSpeed;
                }
                isdashing = false;

            }
            #endregion

        }

        m_currentdashtime += m_dashstopspeed;

        // Resets your speed to default value after dash
        if (m_currentdashtime == m_resetdashtime)
        {
            m_MovementSpeed = m_DefaultMovementSpeed;
        }

    }
    /// <summary>
    /// Makes Player Shoot
    /// </summary>
    // Tobias Stroedicke
    private void Shoot()
    {
        // if player did not press start return
        if (!wannaPlay)
            return;

        // check if player is allowed to shoot, else return
        if (!GetCurrentWeapon.Shoot())
            return;

        // if current weapon is gun calculate ray and activate line renderer
        if (GetCurrentWeapon.GetWeaponName == AWeapon.WeaponName.MACHINEGUN)
        {
            Ray ray = m_playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            CmdWeapon(ray.origin, ray.direction, AWeapon.WeaponName.MACHINEGUN);

            // Max Poppicht
            #region Line Renderer

            m_lineRenderer.positionCount = 2;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hitdistance = hit.distance;
                Vector3 endPos = ray.origin + ray.direction * hitdistance;
                m_lineRenderer.SetPosition(0, GetCurrentWeapon.transform.position);
                m_lineRenderer.SetPosition(1, endPos);
                m_lineRenderer.enabled = true;

                m_TracerCounter++;

                CmdShowTracer(endPos);
            }
            #endregion
            
        }
        isShooting = true;
        WaitTimer = WaitTimerDefault;

        
    }

    /// <summary>
    /// Rotate Player
    /// </summary>
    /// <param name="_rotation">rotation of player</param>
    // Tobias Stroedicke
    private void Rotate(Vector3 _rotation)
    {
        Vector3 rotation = transform.localEulerAngles
        + (_rotation.normalized * Time.deltaTime * m_RotationSpeed);
        transform.localEulerAngles = rotation;
    }
    #endregion

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // activate Player Camera
        m_playerCamera.gameObject.SetActive(true);
        CineMachineObject.gameObject.SetActive(true);
        // activate UI
        m_UI.gameObject.SetActive(true);
        LobbyUINotReady();
    }

    private void OnDisconnectedFromServer(NetworkIdentity info)
    {
        Debug.Log(info);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        JoinUI();
    }

    /// <summary>
    /// decrease current round time
    /// </summary>
    // Tobias Stroedicke
    private void TimeCounter()
    {
        LocalRoundTime -= Time.deltaTime; 
    }

    /// <summary>
    /// Player resuming
    /// </summary>
    // Tobias Stroedicke
    public void PlayerResuming()
    {
        Pause p = m_Pause.GetComponent<Pause>();
        p.Options.SetActive(false);
        p.PauseEnter.SetActive(true);
        m_Pause.SetActive(false);

        if (transform.position.y <= 200f)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// show tracer for other players
    /// </summary>
    /// <param name="_startPos">start position of line renderer</param>
    /// <param name="_endPos">end position of line renderer</param>
    // Max Poppicht
    // Tobias Stroedicke
    [ClientRpc]
    public void RpcShowTracer(Vector3 _startPos, Vector3 _endPos)
    {
        if (!isLocalPlayer)
        {
            m_lineRenderer.SetPosition(0, _startPos);
            m_lineRenderer.SetPosition(1, _endPos);
            m_lineRenderer.enabled = true;
            WaitTimer = WaitTimerDefault;
        }
    }

    /// <summary>
    /// Give server endPos of the Shooting Ray to calculate Tracer
    /// </summary>
    /// <param name="_endPos"></param>
    //  Max Poppicht
    [Command]
    public void CmdShowTracer(Vector3 _endPos)
    {
        RpcShowTracer(GetCurrentWeapon.transform.position, _endPos);
    }
}
