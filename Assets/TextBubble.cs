using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class TextBubble : MonoBehaviour
{
    TextMeshProUGUI textBox;
    [SerializeField] float typingInterval = 0.04f;
    [SerializeField] float periodPause = 1f;
    [SerializeField] float commaPause = 0.5f;
    AudioSource audioSource;
    private void Awake()
    {
        textBox = GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
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
            yield return new WaitForSeconds(typingInterval);
            textBox.text += c;
            switch (c)
            {
                case ',':
                    //audioSource.Stop();
                    yield return new WaitForSeconds(commaPause);
                    break;

                case '.':
                    //audioSource.Stop();
                    yield return new WaitForSeconds(periodPause);
                    break;

                case ' ':
                    //audioSource.Stop();
                    break;

                default:
                    //audioSource.Play();
                    audioSource.PlayOneShot(audioSource.clip);
                    break;
            }
        }
    }
}
