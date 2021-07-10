﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public enum PlayerState { Idle, Run, Jumping, Falling, Dashing, Dead }

    // Unity Component Handles
    GameObject GO;
    Rigidbody2D RB;
    SpriteRenderer SR;
    Animator ANIM;
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
        ANIM = GO.GetComponent<Animator>();
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
        Inputs.Player.CheatBubble.performed += ctx => AddBubble();

        // Set starting state
        ChangeState(PlayerState.Idle);

    }

    private void Start()
    {
        AddBubble();
    }

    private void Update()
    {
        OUTPUT_VELOCITY = RB.velocity;
        UpdateBubblePositions();
    }

    void FixedUpdate()
    {
        
        if (_OnGroundFlag)
        {
            _OnGroundFlag = false;
            if (RequestLand())
            {
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
            
        }

        IEnumerator DeathAnimation() {
            GameAssets.Sound.death.Play();
            RB.simulated = false;
            yield return new WaitForSeconds(3);
            ReloadLevel();
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

        if (State == PlayerState.Jumping)
        {
            if (RB.velocity.y < 0) ChangeState(PlayerState.Falling);
        }

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
        ChangeState(PlayerState.Falling);

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

    void AddBubble() {
        AttachBubble(Instantiate(Resources.Load<GameObject>("Bubble"), transform.position, Quaternion.identity));
    }

    void AttachBubble(GameObject BubbleObject)
    {
        GameAssets.Sound.pop1.Play();
        Bubbles.Add(BubbleObject);
        Bubble BubbleComponent = BubbleObject.GetComponent<Bubble>();
        BubbleComponent.AttachToPlayer(BUBBLE_RADIUS_MIN + BUBBLE_RADIUS_INC * (Bubbles.Count - 1));
    }

    void PopBubble()
    {
        Bubbles[Bubbles.Count - 1].GetComponent<Bubble>().UnattachFromPlayer();
        Bubbles.RemoveAt(Bubbles.Count - 1);
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
        GameAssets.Sound.pop3.Play();
        PopBubble();
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
        State = _State;
        Color[] Colors = { Color.cyan, Color.blue, Color.green, Color.red, Color.yellow, Color.white};
        SR.color = Colors[(int)State];
    }

    private bool OnGround()
    {

        RaycastHit2D[] _Hits;
        float XScale = 0.5f;
        Vector2 _Origin = gameObject.transform.position; // + (0.5f * XScale * Vector3.left);
        Vector2 _Size = new Vector2(XScale, 0.1f);

        _Hits = Physics2D.BoxCastAll(_Origin, _Size, 0f, Vector2.down, 0.2f);
        
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
