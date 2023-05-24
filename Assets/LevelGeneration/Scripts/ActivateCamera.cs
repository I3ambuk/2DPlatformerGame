using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCamera : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger");
        if (collision.CompareTag("Player"))
        {
            switchCamera();
        }
    }

    private void switchCamera()
    {
        MainCamera maincamera = Camera.main.GetComponent<MainCamera>();
        if (maincamera != null)
        {
            maincamera.switchCameraToPos(transform.position);
        }
    }
}
