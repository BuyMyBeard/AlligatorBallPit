using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public enum GroundedCharacterAnimations { Idle, Walking, Jumping, Raising, Falling, Landing }
public enum Slope { Down, Up, None };

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public abstract class GroundedCharacter : MonoBehaviour
{
    [Header("GroundedCharacter")]
    [Space(10)]
    [SerializeField] protected float terminalVelocity = -6;
    [SerializeField] protected float gravAcceleration = -15;
    [SerializeField] public float horizontalSpeed = 7;
    [SerializeField] protected float jumpVelocity = 12;
    [SerializeField] protected float groundCheckDistance = 0.1f;
    [SerializeField] protected float compensation;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask platformLayer;
    [SerializeField] protected PhysicsMaterial2D noFriction;
    [SerializeField] protected PhysicsMaterial2D highFriction;
    [SerializeField] protected float SlopeUpCompensation = 0.05f;
    //AudioManagerComponent audioManager;

    protected bool isGrounded = false;
    protected Animator animator;
    public Slope slope = Slope.None;
    public SpriteRenderer Sprite { get; set; }
    public Rigidbody2D RB { get; protected set; }
    public CapsuleCollider2D CC { get; protected set; }
    public Vector2 ColliderSize { get; protected set; }

    public Vector2 Velocity
    {
        get => RB.velocity;
        protected set => RB.velocity = value;
    }

    protected Vector2 newVelocity;

    public bool IsFalling
    {
        get => !isGrounded && Velocity.y < 0;
    }
    public bool IsJumping
    {
        get => !isGrounded && Velocity.y > 0;
    }
    public bool IsTouchingSlope
    {
        get => slope != Slope.None;
    }

    public bool IsWalking
    {
        get => isGrounded && Velocity.x != 0;
    }

    protected void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        CC = GetComponent<CapsuleCollider2D>();
        Sprite = GetComponent<SpriteRenderer>();
        ColliderSize = CC.size * transform.localScale;
        //audioManager = GetComponent<AudioManagerComponent>();
    }

    protected void FixedUpdate()
    {
        newVelocity = Velocity;
        FloorCheck();
        AddGravity();
        LimitVelocity();
        Velocity = newVelocity;
    }

    protected virtual void LimitVelocity()
    {
        if (newVelocity.y < terminalVelocity)
            newVelocity.y = terminalVelocity;
    }

    protected virtual void AddGravity()
    {
        if (isGrounded)
        {
            newVelocity.y = 0;
            RB.sharedMaterial = highFriction;
        }
        else
        {
            newVelocity.y += gravAcceleration * Time.deltaTime;
            RB.sharedMaterial = noFriction;
        }
    }

    protected virtual void AddSlopeCompensation()
    {
        if (slope == Slope.Up)
            newVelocity.y = newVelocity.x;
        else if (slope == Slope.Down)
            newVelocity.y = -newVelocity.x;
    }

    protected virtual void FloorCheck()
    {
        Vector2 rayOrigin = transform.position - new Vector3(0, ColliderSize.y / 2) + (Vector3)(CC.offset * transform.localScale);
        SlopeCheck(rayOrigin);
        GroundCheck(rayOrigin);
    }
    protected void GroundCheck(Vector2 rayOrigin)
    {
        bool wasGrounded = isGrounded;
        RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = !IsJumping && groundHit;

        if (!wasGrounded && isGrounded)
            transform.Translate(0, -groundHit.distance, 0);
        else if (!isGrounded)
            slope = Slope.None;
        //supposed to compensate for moving up slope, but breaks a bunch of stuff
        //else if (isTouchingGround && slope == Slope.None && groundHit.distance != 0)
        //    transform.Translate(0, -groundHit.distance, 0);

        Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, Color.red);
    }
    private void SlopeCheck(Vector2 rayOrigin)
    {
        Slope oldSlope = slope;
        RaycastHit2D slopeHitRight = Physics2D.Raycast(rayOrigin, Vector2.right, groundCheckDistance, groundLayer);
        RaycastHit2D slopeHitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, groundCheckDistance, groundLayer);
        if (!slopeHitLeft && !slopeHitRight)
            slope = Slope.None;
        else if (slopeHitLeft && slopeHitRight)
        {
            slope = Slope.None;
            transform.Translate(0, compensation, 0);
        }
        else if (slopeHitRight)
            slope = Slope.Up;

        else //slopeHitBack
            slope = Slope.Down;

        if (oldSlope != Slope.None && slope == Slope.None && isGrounded)
            StartCoroutine(CompensateForSlopeUp());
        Debug.DrawRay(rayOrigin, Vector2.right * groundCheckDistance, Color.green);
        Debug.DrawRay(rayOrigin, Vector2.left * groundCheckDistance, Color.blue);
    }

    IEnumerator CompensateForSlopeUp()
    {
        yield return new WaitForSeconds(SlopeUpCompensation);
        Vector2 rayOrigin = transform.position - new Vector3(0, ColliderSize.y / 2) + (Vector3)(CC.offset * transform.localScale);
        RaycastHit2D floorHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        if (floorHit && isGrounded && slope == Slope.None)
        {
            transform.Translate(0, -floorHit.distance, 0);
        }
    }
}



[RequireComponent(typeof(PlayerInputs))]
public class PlayerMove : GroundedCharacter
{
    enum Animations { MCStunned, MCIdle, MCRun, MCRaising, MCFalling, MCDying };
    [Space(20)]
    [Header("Player")]
    [Space(10)]
    [SerializeField] float ascendingDrag = 1;
    [SerializeField] float holdingJumpDrag = 1;
    [SerializeField] float coyoteTime = 0.2f;
    [SerializeField] float stunnedGravityScale = 1;
    [SerializeField] float deadSpeed = 1;
    [SerializeField] float timeBeforeDeath = 4;
    [SerializeField] PhysicsMaterial2D ragdollPhysics;
    private AudioManager audioManager;

    AudioSource audioSource;
    Animations currentAnimation = Animations.MCIdle;

    PlayerInputs inputs;
    public bool stunned = false;
    float coyoteTimeElapsed = 0;

    public bool bouncedOnEnemy = false;
    private bool isDead = false;
    public bool IsCoyoteTime
    {
        get => coyoteTimeElapsed < coyoteTime;
    }

    new private void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        inputs = GetComponent<PlayerInputs>();
        animator = GetComponent<Animator>();
        //sfx = GetComponent<AudioManagerComponent>();
    }
    private void Start()
    {
        audioManager = GetComponent<AudioManager>();
    }
    new private void FixedUpdate()
    {
        if (isDead)
        {
            transform.Translate(deadSpeed * Time.fixedDeltaTime * Vector2.down);
            return;
        }
        if (stunned)
            return;

        newVelocity = Velocity;
        FloorCheck();
        SetHorizontalVelocity();
        AddGravity();
        AddSlopeCompensation();
        CheckInputs();
        AddDrag();
        LimitVelocity();
        Velocity = newVelocity;
        if (isGrounded)
            ResetCoyoteTime();
    }

    private void Update()
    {
        if (stunned || isDead)
            return;
        if (isGrounded)
        {
            if (Velocity.x != 0)
                SetAnimation(Animations.MCRun);
            else
                SetAnimation(Animations.MCIdle);
        }
        else
        {
            if (Velocity.y <= 0)
                SetAnimation(Animations.MCFalling);
            else
                SetAnimation(Animations.MCRaising);
        }
    }

    private void SetAnimation(Animations animation)
    {
        if (currentAnimation != animation)
        {
            animator.Play(animation.ToString());
            currentAnimation = animation;
        }
    }


    private void SetHorizontalVelocity()
    {
        newVelocity.x = inputs.MoveInput.x * horizontalSpeed;
        if (inputs.MoveInput.x != 0)
            Sprite.flipX = inputs.MoveInput.x < 0;
    }
    private void CheckInputs()
    {
        if ((inputs.JumpPressInput && (isGrounded || IsCoyoteTime) && !IsJumping) || bouncedOnEnemy)
        {
            //audioManager.PlaySFX(0);
            newVelocity.y = jumpVelocity;
            bouncedOnEnemy = false;
        }
    }

    protected override void AddGravity()
    {
        if (isGrounded)
        {
            newVelocity.y = 0;
            RB.sharedMaterial = highFriction;
        }
        else
        {
            newVelocity.y += gravAcceleration * Time.deltaTime;
            coyoteTimeElapsed += Time.deltaTime;
            RB.sharedMaterial = noFriction;
        }
    }

    private void AddDrag()
    {
        if (IsJumping)
        {
            newVelocity.y += ascendingDrag * Time.deltaTime;
            if (inputs.JumpHoldInput)
                newVelocity.y += holdingJumpDrag * Time.deltaTime;
        }
    }

    public void ResetCoyoteTime()
    {
        coyoteTimeElapsed = 0;
    }
    public void TakeKnockBack(Vector2 knockback)
    {
        SetAnimation(Animations.MCStunned);
        currentAnimation = Animations.MCStunned;
        RB.velocity = knockback;
        RB.gravityScale = stunnedGravityScale;
        stunned = true;
        RB.sharedMaterial = ragdollPhysics;
        //audioManager.PlaySFX(2);
    }
    public void Recover()
    {
        SetAnimation(Animations.MCIdle);
        RB.gravityScale = 0;
        stunned = false;
    }
}
