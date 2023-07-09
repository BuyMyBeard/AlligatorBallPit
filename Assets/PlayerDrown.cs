using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrown : MonoBehaviour
{
    PlayerMove playerMove;
    [SerializeField] Slider airSlider;
    [SerializeField] bool inverted = false;
    [SerializeField] float fillRate = 0.3f;
    [SerializeField] float emptyRate = 0.3f;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();

    }
    void Update()
    {
        if (inverted)
        {
            if (airSlider.value >= 1)
                airSlider.gameObject.SetActive(false);
            if (!playerMove.Submerged)
            {
                airSlider.gameObject.SetActive(true);
                airSlider.value -= emptyRate * Time.deltaTime;
            }
            else
            {
                airSlider.value += fillRate * Time.deltaTime;
            }
        }
        else
        {
            if (airSlider.value >= 1)
                airSlider.gameObject.SetActive(false);
            if (playerMove.Submerged)
            {
                airSlider.gameObject.SetActive(true);
                airSlider.value -= emptyRate * Time.deltaTime;
            }
            else
            {
                airSlider.value += fillRate * Time.deltaTime;
            }
        }
        if (airSlider.value <= 0)
        {
            playerMove.Drown();
        }
    }
}
