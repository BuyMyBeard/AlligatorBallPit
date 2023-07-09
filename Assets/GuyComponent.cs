using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GuyComponent : MonoBehaviour
{
    [SerializeField] Menu winMenu;
    [SerializeField] PlayerMove playerMove;
    [SerializeField] TextBubble textBubble;
    [SerializeField] float fadeOutSpeed = 2f;
    SpriteRenderer sprite;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        if (LevelManager.currentLevel == 1)
            StartCoroutine(Level1Sequence());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<PlayerMove>().StartCompleteLevel();
        StopAllCoroutines();
        StartCoroutine(LevelCompleteSequence());
    }
    IEnumerator LevelCompleteSequence()
    {
        LevelManager.UnlockNextLevel();
        winMenu.BlockPauseMenu();
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeOut());
        yield return new WaitForSeconds(1);
        winMenu.gameObject.SetActive(true);
    }
    IEnumerator Level1Sequence()
    {
        yield return new WaitForSeconds(0.5f);
        textBubble.StartWrite("I have to reach that door!");
        yield return new WaitUntil(() => !playerMove.Frozen && !textBubble.IsWriting);
        textBubble.StartWrite("Come to Papa!");
    }
    public IEnumerator FadeOut()
    {
        for (float t = 0; t <= 1.1; t += Time.deltaTime * fadeOutSpeed)
        {
            Color c = sprite.color;
            c.a = 1 - t;
            sprite.color = c;
            yield return null;
        }
    }
    public void Cry()
    {
        animator.Play("Crying");
    }
}
