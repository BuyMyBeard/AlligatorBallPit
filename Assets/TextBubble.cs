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
    [SerializeField] float bubbleIdleSpan = 3;
    public bool IsWriting { get; private set; } = false;
    AudioSource audioSource;
    private void Awake()
    {
        textBox = GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Clear() => textBox.text = "";
    public void StartWrite(string text)
    {
        StopAllCoroutines();
        StartCoroutine(Write(text));
    }
    private IEnumerator Write(string text)
    {
        gameObject.SetActive(true);
        Clear();
        IsWriting = true;
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
        IsWriting = false;
        yield return new WaitForSeconds(bubbleIdleSpan);
        gameObject.SetActive(false);
    }
}
