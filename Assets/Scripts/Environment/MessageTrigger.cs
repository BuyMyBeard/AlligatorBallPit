using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    bool triggered = false;
    [SerializeField] string message;
    [SerializeField] TextBubble textBubble;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered)
        {
            triggered = true;
            textBubble.StartWrite(message);
        }
    }
}
