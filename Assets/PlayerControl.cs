using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Inputs InputMap;
    Vector2 Movement = Vector2.zero;
    Rigidbody2D RB;
    SpriteRenderer SR;
    Animator ANIM;

    const float Speed = 8f;
    const float JumpSpeed = 12f;
    const float RollPulseDelay = 0.25f;
    const float RollStunDelay = 0.25f;
    bool _OnGround = false;
    bool _Jumping = false;
    bool _Rolling = false;
    bool _RollInputFlag = false;

    void Awake()
    {
        ANIM = GameObject.Find("Tekking/Main").GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        SR = GameObject.Find("Tekking/Main").GetComponent<SpriteRenderer>();
        InputMap = new Inputs();
        InputMap.Enable();
        InputMap.Player.Move.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        InputMap.Player.Jump.performed += ctx => _JumpInputFlag = true;
    }

    IEnumerator JumpScript()
    {
    }

    void FixedUpdate()
    {

        if (_RollInputFlag)
        {
            _RollInputFlag = false;
            if (!_Rolling && _OnGround)
            {
                _Rolling = true;
                ANIM.SetTrigger("RollStartTrigger");
                StartCoroutine("RollScript");

                if (!SR.flipX)
                {
                    SetXVelocityWithForce(  Speed * 3);
                }
                else
                {
                    SetXVelocityWithForce(- Speed * 3);
                }
            }
        }
        

        if (!_Rolling)
        {
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
            else
            {
                SetXVelocityWithForce(0f);
            }
        }

        if (!_Rolling)
        {
            if (Movement.y > 0 && _OnGround)
            {
                Jump();
            }
        }

    }

    void SetXVelocityWithForce (float DesiredVelocity)
    {
        RB.AddForce((new Vector2(DesiredVelocity, 0) - new Vector2(RB.velocity.x, 0)) / Time.fixedDeltaTime);
        ANIM.SetFloat("AbsTargetSpeedX", Mathf.Abs(DesiredVelocity));
    }

    void SetYVelocityWithForce(float DesiredVelocity)
    {
        RB.AddForce((new Vector2(0, DesiredVelocity) - new Vector2(0, RB.velocity.y)) / Time.fixedDeltaTime);
    }

    void Jump()
    {
        _Jumping = true;
        _OnGround = false;
        SetYVelocityWithForce(JumpSpeed);
        ANIM.SetTrigger("JumpStartTrigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _OnGround = true;
        if (_Jumping)
        {
            _Jumping = false;
            ANIM.SetTrigger("JumpEndTrigger");
        }
        
    }

}
