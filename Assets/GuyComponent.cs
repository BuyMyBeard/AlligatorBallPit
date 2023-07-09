using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuyComponent : MonoBehaviour
{
    [SerializeField] Menu winMenu;
    [SerializeField] PlayerMove playerMove;
    [SerializeField] TextBubble textBubble;
    private void Awake()
    {
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
        yield return new WaitForSeconds(2);
        winMenu.gameObject.SetActive(true);
    }
    IEnumerator Level1Sequence()
    {
        yield return new WaitForSeconds(0.5f);
        textBubble.StartWrite("I have to reach that door!");
        yield return new WaitUntil(() => !playerMove.Frozen && !textBubble.IsWriting);
        textBubble.StartWrite("Come to Papa!");
    }
}
