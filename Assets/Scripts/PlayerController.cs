using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{

    public enum PlayerState { Idle, Run, Jumping, Falling, Dashing, Dead, Spawning, Clearing }

    // Unity Component Handles
    GameObject GO;
    Rigidbody2D RB;
    SpriteRenderer SR;
    BoxCollider2D BC;

    // Bubble Parameters
    List<GameObject> Bubbles;

    // Serialized Fields
    [SerializeField] float MOVE_SPEED;
    [SerializeField] float JUMP_SPEED;
    [SerializeField] int DASH_PULSE_FRAMES;
    [SerializeField] int DASH_STUN_FRAMES;
    [SerializeField] float DASH_VELOCITY;
    [SerializeField] Vector2 OUTPUT_VELOCITY;
    [SerializeField] PlayerState State;
    [SerializeField] PlayerState StartState;

    // Level Text
    [SerializeField] string LevelText = "";
    TextMeshProUGUI PlayerText;

    // Player Input Variables
    bool _JumpInputFlag = false;
    bool _DashInputFlag = false;
    bool _OnGroundFlag = false;
    PlayerInput Inputs;
    Vector2 Movement = Vector2.zero;
    float DefaultLinearDrag;

    // Bubble Variables
    float BUBBLE_RADIUS_MIN = 0.75f;
    float BUBBLE_RADIUS_INC = 0.4f;

    #region Unity Events

    void Awake()
    {
        
        // Capture Object Components
        GO = GameObject.Find("PlayerObject");
        RB = GO.GetComponent<Rigidbody2D>();
        SR = GO.GetComponent<SpriteRenderer>();
        BC = GO.GetComponent<BoxCollider2D>();
        DefaultLinearDrag = RB.drag;
        Bubbles = new List<GameObject>();

        // Initialize Inputs
        Inputs = new PlayerInput();
        Inputs.Enable();
        Inputs.Player.Move.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        Inputs.Player.Jump.performed += ctx => _JumpInputFlag = true;
        Inputs.Player.Dash.performed += ctx => _DashInputFlag = true;
        Inputs.Player.CheatBubble.performed += ctx => AddBubble(Bubble.BubbleType.Jump);

        // Set starting state
        Spawn();

    }

    private void Start()
    {
    }

    private void Spawn()
    {
        ChangeState(PlayerState.Spawning);
        Physics2D.gravity = new Vector2(0f, -9.8f);
        StartCoroutine(SpawnAnimation());
    }

    private IEnumerator SpawnAnimation()
    {

        RB.simulated = false;
        ChangeState(PlayerState.Spawning);

        // Fly up from bottom of screen
        SR.material = GameAssets.Material.Offset;
        Vector2 StartOffset = new Vector2(-0.02f, -0.29f);
        Vector2 MidOffset = new Vector2(-0.02f, 0.04f);
        Vector2 EndOffset = new Vector2(-0.02f, 0f);
        Vector2 CurrentOffset = StartOffset;

        Vector3 VelocityRef = Vector3.zero;
        while ((CurrentOffset - MidOffset).SqrMagnitude() < 0.1f) {
            CurrentOffset = Vector3.SmoothDamp(CurrentOffset, MidOffset, ref VelocityRef, 2f);
            SR.material.SetVector("_Offset", CurrentOffset);
            yield return null;
        }

        while (CurrentOffset != EndOffset)
        {
            CurrentOffset = Vector3.SmoothDamp(CurrentOffset, EndOffset, ref VelocityRef, 0.1f);
            SR.material.SetVector("_Offset", CurrentOffset);
            yield return null;
        }

        SR.material = GameAssets.Material.SpriteDefault;
        RB.simulated = true;
        ChangeState(StartState);

        // Set-Up Level Text
        PlayerText = GameObject.Find("PlayerText").GetComponent<TextMeshProUGUI>();
        PlayerText.text = LevelText;

    }

    private void Update()
    {
        OUTPUT_VELOCITY = RB.velocity;
        UpdateBubblePositions();
    }

    IEnumerator DeathAnimation()
    {
        GameAssets.Sound.death.Play();
        RB.simulated = false;
        yield return new WaitForSeconds(1);
        ReloadLevel();
    }

    void FixedUpdate()
    {

        if (State == PlayerState.Spawning || State == PlayerState.Clearing) return;

        if (_OnGroundFlag)
        {
            _OnGroundFlag = false; 
            RequestLand();
        }

        if (_JumpInputFlag)
        {
            _JumpInputFlag = false;
            if (RequestJump()) ChangeState(PlayerState.Jumping);
        }
        
        if (_DashInputFlag)
        {
            _DashInputFlag = false;
            if(RequestDash()) ChangeState(PlayerState.Dashing);
        }

        if (Movement.x != 0)
        {
            RequestMove();
        } 
        else
        {
            if (State != PlayerState.Dashing)
            {
                SetXVelocityWithForce(0f);
            }
            if (State == PlayerState.Run) ChangeState(PlayerState.Idle);
        }

        if (State == PlayerState.Jumping || State == PlayerState.Run || State == PlayerState.Idle)
        {
            if (RB.velocity.y < -1f)
            {
                ChangeState(PlayerState.Falling);
            }
        }

    }

    private void Land()
    {
        if (Bubbles.Count > 0)
        {
            if (GetOuterBubbleType() == Bubble.BubbleType.Land)
            {
                PopBubble();
            }
            else
            {
                MiniBurstEffect();
            }
        }

        if (Bubbles.Count > 0)
        {
            ChangeState(PlayerState.Idle);
        }
        else
        {
            ChangeState(PlayerState.Dead);
            StartCoroutine(DeathAnimation());
        }
    }

    private Bubble.BubbleType? GetBubbleTypeByObject(GameObject _BubbleObject)
    {
        if (Bubbles.Count == 0) return null;
        Bubble BubbleComponent = _BubbleObject.GetComponent<Bubble>();
        return BubbleComponent.Type;
    }
    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        
        // Check for Ground
        _OnGroundFlag = OnGround();

    }

    private void OnTriggerEnter2D(Collider2D _Collider)
    {
        if (_Collider.gameObject.CompareTag("Bubble"))
        {
            AttachBubble(_Collider.gameObject);
        }
    }

    #endregion

    #region Handle Requests
    private bool RequestJump()
    {
        if (Bubbles.Count == 0) return false;
        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
                Jump();
                return true;
            default:
                return false;
        }
    }

    private bool RequestDash()
    {

        // UnityEditor.EditorApplication.isPaused = true;

        // Capture Movement Value
        Vector2 DashMovement = Movement;

        // Do not allow neutral dashing, leads to unknown physics shenanigans
        if (DashMovement == Vector2.zero) return false;

        // Bubble required
        if (Bubbles.Count == 0) return false;

        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
            case PlayerState.Jumping:
            case PlayerState.Falling:
                Dash(DashMovement);
                return true;
            default:
                return false;
        }
    }

    private bool RequestLand()
    {
        switch (State)
        {
            case PlayerState.Falling:
                Land();
                return true;
            default:
                return false;
        }
    }

    private bool RequestMove()
    {
        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
            case PlayerState.Jumping:
            case PlayerState.Falling:
                Move(); 
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Dash Methods

    private void Dash(Vector2 DashMovement)
    {

        // Pop a Bubble
        GameAssets.Sound.pop2.Play();
        PopBubble();

        // Determine Dash Vector
        Vector2 DashVector;
        DashVector = DashMovement.normalized * DASH_VELOCITY;

        Coroutine DashRoutine;
        DashRoutine = StartCoroutine(DashScript(DashVector));
    }

    private IEnumerator DashScript(Vector2 DashVector)
    {

        // Determine Pulse Delay
        float PulseTime = DASH_PULSE_FRAMES * Time.fixedDeltaTime;

        // Time Pulse,
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(PulseTime);

        // Continue Time, Apply constant velocity, remove gravity for duration, temporarily disable drag
        RB.drag = 0f;
        Time.timeScale = 1;
        SetXVelocityWithForce(DashVector.x);
        SetYVelocityWithForce(DashVector.y);
        Physics2D.gravity = Vector2.zero;
        for (int i = 0; i < DASH_STUN_FRAMES; i++)
        {

            yield return new WaitForFixedUpdate();
        }

        // Set state to falling, resume drag and gravity
        Physics2D.gravity = new Vector2(0, -9.8f);
        RB.drag = DefaultLinearDrag;
        SetXVelocityWithForce(0);
        SetYVelocityWithForce(0);
        ChangeState(PlayerState.Idle);

    }

    #endregion

    #region Physics

    void SetXVelocityWithForce(float DesiredVelocity)
    {
        RB.AddForce((new Vector2(DesiredVelocity, 0) - new Vector2(RB.velocity.x, 0)) / Time.fixedDeltaTime);
        // ANIM.SetFloat("AbsTargetSpeedX", Mathf.Abs(DesiredVelocity));
    }

    void SetYVelocityWithForce(float DesiredVelocity)
    {
        RB.AddForce((new Vector2(0, DesiredVelocity) - new Vector2(0, RB.velocity.y)) / Time.fixedDeltaTime);
    }

    #endregion

    #region Bubble Methods

    void AddBubble(Bubble.BubbleType _BubbleType) {
        AttachBubble(Instantiate(Resources.Load<GameObject>("Bubble"), transform.position, Quaternion.identity));
        Bubbles[Bubbles.Count - 1].GetComponent<Bubble>().SetBubbleType(_BubbleType);
    }

    void AttachBubble(GameObject BubbleObject)
    {
        GameAssets.Sound.pop1.Play();
        Bubbles.Add(BubbleObject);
        Bubble BubbleComponent = BubbleObject.GetComponent<Bubble>();
        BubbleComponent.AttachToPlayer(BUBBLE_RADIUS_MIN + BUBBLE_RADIUS_INC * (Bubbles.Count - 1));
        if (BubbleComponent.Type == Bubble.BubbleType.Win)
        {
            Win();
        }
    }

    void Win()
    {
        ChangeState(PlayerState.Clearing);
        PopBubble();
        StartCoroutine(WinAnimation());
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator WinAnimation()
    {

        RB.simulated = false;
        ChangeState(PlayerState.Clearing);

        // Some delay
        yield return new WaitForSeconds(0.1f);

        // Fly up the screen
        SR.material = GameAssets.Material.Offset;
        Vector2 StartOffset = new Vector2(-0.02f, 0f);
        Vector2 EndOffset = new Vector2(-0.02f, 0.6f);
        Vector2 CurrentOffset = StartOffset;

        Vector3 VelocityRef = Vector3.zero;
        while (CurrentOffset != EndOffset)
        {
            CurrentOffset = Vector3.SmoothDamp(CurrentOffset, EndOffset, ref VelocityRef, 0.12f);
            SR.material.SetVector("_Offset", CurrentOffset);
            yield return null;
        }

        LoadNextLevel();
        yield return null;

    }

    void DestroyBubble()
    {
        GameObject BubbleObject = Bubbles[Bubbles.Count - 1];
        PopBubble();
        Destroy(BubbleObject);
    }
    void PopBubble()
    {

        // Make Bubble Effect
        AddBurstEffect(Bubbles[Bubbles.Count - 1]);

        // Remove Bubble from Player
        Bubbles[Bubbles.Count - 1].GetComponent<Bubble>().UnattachFromPlayer();
        Bubbles.RemoveAt(Bubbles.Count - 1);
        GameAssets.Sound.pop3.Play();

    }

    public void AddBurstEffect(GameObject _BubbleObject)
    {

        Bubble _BubbleComponent = _BubbleObject.GetComponent<Bubble>();
        Vector2 ParticlePositionFloat = transform.position + new Vector3(0f, 0.5f, 0f); 

        GameObject ParticleControllerObject = Instantiate(Resources.Load<GameObject>("ParticleController"));

        if (ParticleControllerObject == null)
        {
            Debug.LogError("Particle Object Resource Not Found.");
        }

        ParticleControllerObject.GetComponent<ParticleController>().StartParticle("BubbleBurst", ParticlePositionFloat, 1f);

        GameObject ParticleObject = ParticleControllerObject.GetComponent<ParticleController>().ParticleObject;

        ParticleSystem.MainModule ParticleSetting = ParticleObject.GetComponent<ParticleSystem>().main;
        ParticleSetting.startColor = new ParticleSystem.MinMaxGradient(_BubbleComponent.ActiveColor);

    }

    public void MiniBurstEffect()
    {

        GameObject _BubbleObject = Bubbles[Bubbles.Count - 1];
        Bubble _BubbleComponent = _BubbleObject.GetComponent<Bubble>();
        Vector2 ParticlePositionFloat = transform.position + new Vector3(0f, 0f, 0f);

        GameObject ParticleControllerObject = Instantiate(Resources.Load<GameObject>("ParticleController"));

        if (ParticleControllerObject == null)
        {
            Debug.LogError("Particle Object Resource Not Found.");
        }

        ParticleControllerObject.GetComponent<ParticleController>().StartParticle("BubbleBurst", ParticlePositionFloat, 1f);

        GameObject ParticleObject = ParticleControllerObject.GetComponent<ParticleController>().ParticleObject;

        ParticleObject.transform.localScale /= 3f;
        ParticleSystem.MainModule ParticleSetting = ParticleObject.GetComponent<ParticleSystem>().main;
        ParticleSetting.startColor = new ParticleSystem.MinMaxGradient(_BubbleComponent.ActiveColor);

    }

    void UpdateBubblePositions()
    {
        if (Bubbles.Count == 0) return;
        foreach(GameObject _Bubble in Bubbles)
        {
            _Bubble.transform.position = gameObject.transform.position + Vector3.up * 0.5f;
        }
    }

    #endregion 

    void Jump()
    {

        if (GetOuterBubbleType() == Bubble.BubbleType.Jump)
        {
            PopBubble();
        }
        else
        {
            MiniBurstEffect();
        }

        RB.velocity = new Vector2(RB.velocity.x, 0f);
        SetYVelocityWithForce(JUMP_SPEED);

        // ANIM.SetTrigger("JumpStartTrigger");
    }

    void Move()
    {
        if (State == PlayerState.Idle)
        {
            ChangeState(PlayerState.Run);
        }
        
        if (Movement.x > 0)
        {
            SetXVelocityWithForce(MOVE_SPEED);
            SR.flipX = false;
        }
        else if (Movement.x < 0)
        {
            SetXVelocityWithForce(-MOVE_SPEED);
            SR.flipX = true;
        }
    }

    private void ChangeState(PlayerState _State)
    {
        _OnGroundFlag = false;
        State = _State;
        Color[] Colors = { Color.cyan, Color.blue, Color.green, Color.red, Color.yellow, Color.white, Color.magenta, Color.magenta };
        SR.color = Colors[(int)State];
    }
    
    private Bubble.BubbleType? GetOuterBubbleType()
    {
        if (Bubbles.Count == 0) return null;
        return GetBubbleTypeByObject(Bubbles[Bubbles.Count - 1]);
    }
    private bool OnGround()
    {

        RaycastHit2D[] _Hits;
        float XScale = 0.5f;
        Vector2 _Origin = gameObject.transform.position; // + (0.5f * XScale * Vector3.left);
        Vector2 _Size = new Vector2(XScale, 0.1f);

        _Hits = Physics2D.BoxCastAll(_Origin, _Size, 0f, Vector2.down, 0.01f);
        
        if (_Hits.Length == 0) return false;
        else
        {
            for (int i = 0; i < _Hits.Length; i++)
            {
                if (_Hits[i].collider.gameObject.CompareTag("Wall"))
                {
                    return true;
                }
            }
        }
        return false;

    }

    private bool OnWall()
    {

        RaycastHit2D[] _Hits;
        float XScale = 0.5f;
        Vector2 _Origin = gameObject.transform.position + (0.5f * Vector3.left);
        Vector2 _Size = new Vector2(XScale, 0.2f);

        _Hits = Physics2D.BoxCastAll(_Origin, _Size, 0f, Vector2.down);
        if (_Hits.Length == 0) return false;
        else
        {
            for (int i = 0; i < _Hits.Length; i++)
            {
                if (_Hits[i].collider.gameObject.CompareTag("Wall"))
                {
                    return true;
                }
            }
        }
        return false;

    }

}
