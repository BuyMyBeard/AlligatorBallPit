using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alligator : MonoBehaviour
{
    AudioSource audioSource;
    AudioManager audioManager;
    Animator animator;
    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioManager = GetComponent<AudioManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    bool started = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!started)
        {
            StartCoroutine(EatSequence());
        }
    }

    IEnumerator EatSequence()
    {
        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        transform.position = new Vector3(playerMove.transform.position.x, transform.position.y, transform.position.z);
        playerMove.EatenByAlligator();
        FindObjectOfType<CinemachineVirtualCamera>().Follow = transform;
        started = true;
        spriteRenderer.enabled = true;
        animator.Play("Munch");
        audioManager.PlaySFX(0);
        yield return new WaitForSeconds(3);
        animator.Play("Snap");
        while (true)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            yield return new WaitForSeconds(1);
            spriteRenderer.flipX = !spriteRenderer.flipX;
            audioSource.Play();
            yield return new WaitForSeconds(1);
            spriteRenderer.flipX = !spriteRenderer.flipX;
            yield return new WaitForSeconds(1);
        }
    }
}
