using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuyComponent : MonoBehaviour
{
    [SerializeField] Menu winMenu;
    [SerializeField] TextBubble textBubble;
    private void Awake()
    {
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

}
