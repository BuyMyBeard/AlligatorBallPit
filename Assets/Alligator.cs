using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Alligator : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;
    private void Awake()
    {
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
        yield return new WaitForSeconds(3);
        animator.Play("Snap");
        while (true)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            yield return new WaitForSeconds(1);
        }
    }
}
