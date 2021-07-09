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
    PlayerState State;

    public enum PlayerState { Idle, Run, Jumping, Falling, Dashing }

    const float Speed = 10f;
    const float JumpSpeed = 20f;
    const float DashPulseDelay = 0.25f;
    const float DashStunDelay = 0.25f;
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

        // Initialize Inputs
        Inputs = new PlayerInput();
        Inputs.Enable();
        Inputs.Player.Move.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        Inputs.Player.Jump.performed += ctx => _JumpInputFlag = true;
        Inputs.Player.Dash.performed += ctx => _DashInputFlag = true;

        // Set starting state
        ChangeState(PlayerState.Idle);
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
        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
            case PlayerState.Jumping:
            case PlayerState.Falling:
                // Dash();
                return true;
            default:
                return false;
        }
    }

    private bool RequestLand()
    {
        switch (State)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
            case PlayerState.Jumping:
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
        SetYVelocityWithForce(JumpSpeed);
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
            SetXVelocityWithForce(Speed);
            SR.flipX = false;
        }
        else if (Movement.x < 0)
        {
            SetXVelocityWithForce(-Speed);
            SR.flipX = true;
        }
    }

    private void ChangeState(PlayerState _State)
    {
        State = _State;
        Color[] Colors = { Color.cyan, Color.blue, Color.green, Color.red, Color.yellow};
        SR.color = Colors[(int)State];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _OnGroundFlag = true;
    }

}
