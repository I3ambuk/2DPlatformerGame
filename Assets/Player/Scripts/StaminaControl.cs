using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaControl : MonoBehaviour
{
    [SerializeField] float staminaLossSpeed = 1f;
    [SerializeField] float staminaRefillSpeed = 1f;
    [SerializeField] float pauseAfterZero = 5f;
    PlayerController playerController;
    Vector2 standardUp = Vector2.up;
    public float stamina = 100f;
    private float timerAfterZeroToFill = 0f;
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (playerController.up != standardUp)
        {
            //lose stamina
            if (stamina > 0f) { 
                stamina -= staminaLossSpeed * Time.deltaTime;
            } else
            {
                stamina = 0f;
                playerController.changePlayerGravity(-standardUp);
                timerAfterZeroToFill = pauseAfterZero;
            }
        }
        else
        {
            if (stamina < 100f)
            {
                if (timerAfterZeroToFill == 0f)
                {
                    stamina += staminaRefillSpeed * Time.deltaTime;
                }
            } else
            {
                stamina = 100f;
            }
        }
        timerAfterZeroToFill = timerAfterZeroToFill > 0f? timerAfterZeroToFill - Time.deltaTime: 0f;

    }

}
