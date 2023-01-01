using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Shake Instance;
    //Movement
    private float horizontal;
    private float speed = 8f;
    private float JumpingPower = 25f;
    private bool isFacingRight = true;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    //Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 10f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.5f;
    [SerializeField] private TrailRenderer tr;

    //Double Jump
    private bool doubleJump;

    // Wall Slide
    private bool isWallSliding;
    private float wallSlidingSpeed = 1f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    // Wall Jump
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(3f, 20f);

    //Animaciones
    private Animator anim;

    private void Start()
    {
        anim= GetComponent<Animator>();
    }


    void Update()
    {
        Application.targetFrameRate = 60;

        if (isDashing)
        {
            return;
        }
        if(IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }


        horizontal = Input.GetAxisRaw("Horizontal");



        //Animacion Walk
        if (horizontal == 0f || IsWalled() || isDashing)
        {
            anim.SetBool("isRunning", false);
        }
        else
        {
            anim.SetBool("isRunning", true);
        }
        //Animacion Jump y Fall
        if (Input.GetButton("Jump"))
        {
            if (IsGrounded() || doubleJump)
            {
                anim.SetBool("isJumping", true);
            }
        }
        else
        {
            anim.SetBool("isJumping", false);
        }
        if(!IsGrounded() && !Input.GetButton("Jump"))
        {
            anim.SetBool("isFall", true);
        }
        if (IsGrounded())
        {
            anim.SetBool("isFall", false);
            anim.SetBool("isJumping", false);
        }



        if (Input.GetButtonDown("Jump"))
        {
            if(IsGrounded() || doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, JumpingPower);
                doubleJump = !doubleJump;
            }
        }
        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
        WallSlide();
        WallJump();
        if (!isWallJumping)
        {
            flip();
        }
        
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            anim.SetBool("isDashing", true);
            StartCoroutine(Instance.shake(.03f, .03f));
            return;
        }
        if (!isDashing)
        {
            anim.SetBool("isDashing", false);
        }
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    private void WallSlide()
    {
        if(IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void WallJump()
    {
        if (IsWalled() && !IsGrounded())
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
        if(Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            if(transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
