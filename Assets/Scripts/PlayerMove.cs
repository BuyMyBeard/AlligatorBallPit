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
    //[SerializeField] protected float horizontalAcceleration = 20;
    //[SerializeField] protected float horizontalDecceleration = -20;
    //[SerializeField] protected float airControl = 1;
    [SerializeField] public float horizontalSpeed = 7;
    [SerializeField] protected float jumpVelocity = 12;
    [SerializeField] protected float groundCheckDistance = 0.1f;
    [SerializeField] protected float compensation;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask platformLayer;
    [SerializeField] protected PhysicsMaterial2D noFriction;
    [SerializeField] protected PhysicsMaterial2D highFriction;
    [SerializeField] Vector2 groundCheckBoxSize = Vector2.one;
    //AudioManagerComponent audioManager;

    protected bool isGrounded = false;
    protected Animator animator;
    public SpriteRenderer Sprite { get; set; }
    public Rigidbody2D RB { get; protected set; }
    public CapsuleCollider2D CC { get; protected set; }
    public BoxCollider2D BC { get; protected set; }
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

    public bool IsWalking
    {
        get => isGrounded && Velocity.x != 0;
    }

    protected void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        CC = GetComponent<CapsuleCollider2D>();
        BC = GetComponent<BoxCollider2D>();
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

    protected virtual void FloorCheck()
    {
        Vector2 rayOrigin = transform.position - new Vector3(0, ColliderSize.y / 2) + (Vector3)(CC.offset * transform.localScale);
        GroundCheck(rayOrigin);
    }

    protected void GroundCheck(Vector2 rayOrigin)
    {
        bool wasGrounded = isGrounded;
        RaycastHit2D groundHit = Physics2D.BoxCast(rayOrigin, groundCheckBoxSize, 0, Vector2.down, groundCheckDistance, groundLayer);
        //RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = !IsJumping && groundHit;

        //if (!wasGrounded && isGrounded)
        //    transform.Translate(0, -groundHit.distance, 0);

        //Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, Color.red);
        //Gizmos.color = new Color(1, 0, 0, 0.3f);
        //Gizmos.DrawCube(transform.position - new Vector3(0, ColliderSize.y / 2) + (Vector3)(CC.offset * transform.localScale) + Vector3.down * groundCheckDistance, groundCheckBoxSize);
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
