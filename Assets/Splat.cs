using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splat : MonoBehaviour
{
    bool triggered = false;
    [SerializeField] TextBubble textBubble;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered)
        {
            triggered = true;
            collision.GetComponent<PlayerMove>().Splat();
            textBubble.StartWrite("Wow, you just flattened. Guess Jello isn't so soft after all.");
        }
    }
}
