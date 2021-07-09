using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    PlayerInput Inputs;
    Vector2 Movement = Vector2.zero;
    GameObject GO;
    Rigidbody2D RB;
    SpriteRenderer SR;
    Animator ANIM;
    

    public enum PlayerState { Idle, Run, Jumping, Falling, Dashing }

    [SerializeField] float MOVE_SPEED;
    [SerializeField] float JUMP_SPEED;
    [SerializeField] int DASH_PULSE_FRAMES;
    [SerializeField] int DASH_STUN_FRAMES;
    [SerializeField] float DASH_VELOCITY;
    [SerializeField] Vector2 OUTPUT_VELOCITY;
    [SerializeField] PlayerState State;

    float DefaultLinearDrag;

    bool _JumpInputFlag = false;
    bool _DashInputFlag = false;
    bool _OnGroundFlag = false;

    void Awake()
    {
        
        // Capture Object Components
        GO = GameObject.Find("PlayerObject");
        ANIM = GO.GetComponent<Animator>();
        RB = GO.GetComponent<Rigidbody2D>();
        SR = GO.GetComponent<SpriteRenderer>();
        DefaultLinearDrag = RB.drag;

        // Initialize Inputs
        Inputs = new PlayerInput();
        Inputs.Enable();
        Inputs.Player.Move.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        Inputs.Player.Jump.performed += ctx => _JumpInputFlag = true;
        Inputs.Player.Dash.performed += ctx => _DashInputFlag = true;

        // Set starting state
        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        OUTPUT_VELOCITY = RB.velocity;
    }

    void FixedUpdate()
    {

        if (_OnGroundFlag)
        {
            _OnGroundFlag = false;
            if (RequestLand()) ChangeState(PlayerState.Idle);
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
            if (State == PlayerState.Run) ChangeState(PlayerState.Idle);
        }

        if (State == PlayerState.Jumping)
        {
            if (RB.velocity.y < 0) ChangeState(PlayerState.Falling);
        }

    }

    #region HandleInputs
    private bool RequestJump()
    {
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

    private void Dash(Vector2 DashMovement)
    {

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
            case PlayerState.Dashing:
                return false;
            default:
                Move();
                return true;
        }
    }

    #endregion


    void SetXVelocityWithForce(float DesiredVelocity)
    {
        RB.AddForce((new Vector2(DesiredVelocity, 0) - new Vector2(RB.velocity.x, 0)) / Time.fixedDeltaTime);
        // ANIM.SetFloat("AbsTargetSpeedX", Mathf.Abs(DesiredVelocity));
    }

    void SetYVelocityWithForce(float DesiredVelocity)
    {
        RB.AddForce((new Vector2(0, DesiredVelocity) - new Vector2(0, RB.velocity.y)) / Time.fixedDeltaTime);
    }

    void Jump()
    {
        SetYVelocityWithForce(JUMP_SPEED);
        // ANIM.SetTrigger("JumpStartTrigger");
    }

    void Move()
    {
        // Check for Move Invoked While Dashing
        //if (State == PlayerState.Dashing)
        //{
            Debug.Log("Move invoked.");
        //}
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
        Color[] Colors = { Color.cyan, Color.blue, Color.green, Color.red, Color.yellow};
        SR.color = Colors[(int)State];
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _OnGroundFlag = true;
    }

}
