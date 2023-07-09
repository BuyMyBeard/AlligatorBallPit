using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallShroud : MonoBehaviour {

    private bool fade;
    private bool runFade;
    private bool hasFaded;
    private float fadeSpeed = 2f;
    [SerializeField] private bool willFadeAway;

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.TryGetComponent(out PlayerMove _)) {
            if (!hasFaded) {
                hasFaded = true;
                fade = willFadeAway;                
                StartCoroutine(Fade());

            }

            
        }
    }

    /*
    
    private void OnTriggerExit2D(Collider2D collision) {

        if (collision.TryGetComponent(out PlayerMove _)) {
            fade = false;
        }
    }
    */

    

    [SerializeField] Tilemap spriteRenderer;

    private IEnumerator Fade() {
        runFade = true;
        yield return new WaitForSeconds(2f);
        runFade = false;

    }

    private void Update() {

        if (runFade) {
            float fadeAmount = fade ? -fadeSpeed * Time.deltaTime : fadeSpeed * Time.deltaTime;
            float newAlpha = Mathf.Clamp01(spriteRenderer.color.a + fadeAmount);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
        }
        
    }
}