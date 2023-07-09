using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal otherPortal;
    AudioManager sfx;
    [SerializeField] Animator animator;
    [SerializeField] TextBubble bubble;

    private void Awake()
    {
        sfx = GetComponent<AudioManager>();
    }

    public void StartBloodSplatter() => StartCoroutine(BloodSplatter());

    IEnumerator BloodSplatter()
    {
        GuyComponent guyComponent = FindObjectOfType<GuyComponent>();
        SpriteRenderer sprite = animator.GetComponent<SpriteRenderer>();
        guyComponent.Say("Don't worry, he will come back from the other side");
        yield return new WaitForSeconds(3);
        sprite.enabled = true;
        animator.Play("Default");
        sfx.PlaySFX(0);
        guyComponent.Cry();
        yield return new WaitForSeconds(1.2f);
        sprite.enabled = false;
        bubble.StartWrite("Seems like your mother never told you black holes lead you to your doom...");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<PlayerMove>().TakePortal();
        otherPortal.StartBloodSplatter();
        sfx.PlaySFX(1);
    }


}
