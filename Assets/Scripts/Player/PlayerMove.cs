using Cinemachine;
using JetBrains.Annotations;
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
    [SerializeField] protected float waterMultiplier = 0.5f;
    [SerializeField] protected LayerMask waterLayer;
    protected float movementMultiplier = 1;

    protected bool isGrounded = false;
    protected Animator animator;
    public bool Submerged { get; protected set; } = false;
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
        if (newVelocity.y < terminalVelocity * movementMultiplier)
            newVelocity.y = terminalVelocity * movementMultiplier;
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
            newVelocity.y += gravAcceleration * waterMultiplier * Time.deltaTime;
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



//[RequireComponent(typeof(PlayerInputs))]
public class PlayerMove : GroundedCharacter
{
    enum Animations { Initial, Idle, Walk, Jump, Fall, Open, Blood, Splat };
    [Space(20)]
    [Header("Player")]
    [Space(10)]
    [SerializeField] float ascendingDrag = 1;
    [SerializeField] float holdingJumpDrag = 1;
    [SerializeField] float coyoteTime = 0.2f;
    [SerializeField] float stunnedGravityScale = 1;
    [SerializeField] float deadSpeed = 1;
    [SerializeField] float timeBeforeDeath = 4;
    [SerializeField] TextBubble bottomText;
    private AudioManager audioManager;
    [SerializeField] AudioSource swimSound;

    AudioSource audioSource;
    Animations currentAnimation = Animations.Initial;

    PlayerInputs inputs;
    float coyoteTimeElapsed = 0;

    public bool MovementBlocked { get; private set; } = false;
    public bool Frozen { get; private set; } = false;
    public bool IsCoyoteTime
    {
        get => coyoteTimeElapsed < coyoteTime;
    }

    new private void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        //inputs = GetComponent<PlayerInputs>();
        
        animator = GetComponent<Animator>();
        //sfx = GetComponent<AudioManagerComponent>();

    }
    private void Start()
    {
        inputs = PlayerInputs.Instance;
        if (LevelManager.currentLevel == 1)
            StartCoroutine(IntroAnimation());
        audioManager = GetComponent<AudioManager>();
    }
    new private void FixedUpdate()
    {
        if (Frozen)
            return;
        newVelocity = Velocity;
        FloorCheck();
        if (MovementBlocked)
        {
            AddGravity();
            AddDrag();
            LimitVelocity();
            Velocity = newVelocity;
            return;
        }
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
        if (MovementBlocked || Frozen)
            return;
        if (IsFalling)
        {
            SetAnimation(Animations.Fall);
        }
        else if (IsJumping)
        {
            SetAnimation(Animations.Jump);
        }
        else if (isGrounded)
        {
            if (Velocity.x != 0)
                SetAnimation(Animations.Walk);
            else
                SetAnimation(Animations.Idle);
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
        newVelocity.x = inputs.MoveInput.x * horizontalSpeed * movementMultiplier;
        if (inputs.MoveInput.x != 0)
            Sprite.flipX = inputs.MoveInput.x < 0;
    }
    private void CheckInputs()
    {
        if (inputs.JumpPressInput && (isGrounded || IsCoyoteTime) && !IsJumping)
        {
            audioManager.PlaySFX(0);
            newVelocity.y = jumpVelocity;
            SetAnimation(Animations.Jump);
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
            newVelocity.y += gravAcceleration * movementMultiplier * Time.deltaTime;
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

    public void StartCompleteLevel()
    {
        MovementBlocked = true;
        SetAnimation(Animations.Open);
        Velocity *= new Vector2(0, 1);
    }
    IEnumerator IntroAnimation()
    {
        Vector3 pivot = transform.position + ColliderSize.y * Vector3.down;
        Frozen = true;

        yield return new WaitUntil(() => inputs.NudgeRight && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        transform.RotateAround(pivot, Vector3.forward, -3);
        yield return new WaitUntil(() => inputs.NudgeLeft && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        transform.RotateAround(pivot, Vector3.forward, 6);
        yield return new WaitUntil(() => inputs.NudgeRight && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        transform.RotateAround(pivot, Vector3.forward, -8);
        yield return new WaitUntil(() => inputs.NudgeLeft && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        transform.RotateAround(pivot, Vector3.forward, 10);
        yield return new WaitUntil(() => inputs.NudgeRight && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        transform.RotateAround(pivot, Vector3.forward, -13);
        yield return new WaitUntil(() => inputs.NudgeLeft && Time.timeScale == 1);
        audioManager.PlaySFX(1);
        Frozen = false;
        transform.RotateAround(pivot, Vector3.forward, 8);

        Music.Instance.PlayMusic();
        //GameObject.FindGameObjectWithTag("Music").GetComponent<Music>().PlayMusic();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (waterLayer.IncludesLayer(collision.gameObject.layer))
        {
            movementMultiplier = waterMultiplier;
            Submerged = true;
            animator.speed = 0.5f;
            swimSound.Play();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (waterLayer.IncludesLayer(collision.gameObject.layer))
        {
            movementMultiplier = 1;
            Submerged = false;
            animator.speed = 1;
            swimSound.Stop();
        }
    }

    public void Drown()
    {
        CC.enabled = false;
        Frozen = true;
        SetAnimation(Animations.Fall);
        GetComponent<SpriteRenderer>().sortingOrder = 30;
        FindObjectOfType<CinemachineVirtualCamera>().Follow = null;
        RB.gravityScale = 1;
        RB.velocity = new Vector2(0, 3);
        if (LevelManager.currentLevel == 4)
            bottomText.StartWrite("A door drowning in the air sounds fishy...");
        else if (LevelManager.currentLevel == 5)
            bottomText.StartWrite("Seems like doors can't breathe underwater after all... Wait What?");
        FindObjectOfType<GuyComponent>().Cry();
    }

    public void EatenByAlligator()
    {
        Frozen = true;
        Velocity = Vector2.zero;
        SetAnimation(Animations.Blood);
        bottomText.StartWrite("Seems like you didn't read the name of the game. I wonder what you were expecting.");
        FindObjectOfType<GuyComponent>().Cry();
    }

    internal void TakePortal()
    {
        Frozen = true;
        Velocity = Vector2.zero;
        Sprite.enabled = false;
    }
    public void Splat()
    {
        Frozen = true;
        Velocity = Vector2.zero;
        SetAnimation(Animations.Splat);
        audioManager.PlaySFX(2);
    }

    public void StartSling()
    {
        MovementBlocked = true;
        transform.Translate(0.3f * Vector3.up);
        Velocity = new Vector2(20, 8);
        Sprite.flipX = false;
        SetAnimation(Animations.Fall);
    }
}
public static class ExtensionMethods
{
    public static bool IncludesLayer(this LayerMask mask, int layer) => mask == (mask | (1 << layer));
}
