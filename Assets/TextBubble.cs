using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TextBubble : MonoBehaviour
{
    TextMeshProUGUI textBox;
    [SerializeField] float typingInterval = 0.1f;
    private void Awake()
    {
        textBox = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartWrite("I like nair cream on my butthole hair, it works well. I hope you try it too");
    }
    private void Clear() => textBox.text = "";
    public void StartWrite(string text) => StartCoroutine(Write(text));
    private IEnumerator Write(string text)
    {
        Clear();
        foreach (var c in text)
        {
           textBox.text += c;
            yield return new WaitForSeconds(typingInterval);
        }
    }
}
