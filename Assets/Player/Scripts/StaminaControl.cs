using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaControl : MonoBehaviour
{
    [SerializeField] float staminaLossSpeed = 1f;
    [SerializeField] float staminaRefillSpeed = 1f;
    [SerializeField] float pauseRefillSec = 5f;
    PlayerController playerController;
    Vector2 standardUp = Vector2.up;
    public float maxStamina = 10f;
    public float stamina;
    private float refillTimer = 0f;
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        stamina = maxStamina;
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
                refillTimer = pauseRefillSec;
            }
        }
        else
        {
            if (stamina < maxStamina)
            {
                if (refillTimer == 0f)
                {
                    stamina += staminaRefillSpeed * Time.deltaTime;
                }
            } else if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
        }
        refillTimer = refillTimer > 0f? refillTimer - Time.deltaTime: 0f;

    }

}
