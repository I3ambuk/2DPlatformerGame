using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float speed = 50;
    private Vector2 endMarker;
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(endMarker.x, endMarker.y, transform.position.z), speed * Time.deltaTime);
    }

    public void switchCameraToPos(Vector3 pos)
    {
        this.endMarker = pos;
    }
}
