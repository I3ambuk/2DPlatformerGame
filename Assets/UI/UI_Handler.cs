using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_Handler : MonoBehaviour
{
    [SerializeField] Transform gravityDir;
    [SerializeField] Transform playerTransform;
    Canvas canvas;
    GameObject bottomPanel;
    RectTransform UITransform;

    // Start is called before the first frame update
    void Start()
    {
        UITransform = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();

        //Create bottomPanel
        RectTransform bottomPanelTransform = canvas.GetComponent<RectTransform>();
        bottomPanelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, UITransform.rect.width);
        bottomPanelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UITransform.rect.height / 7);
    }

    // Update is called once per frame
    void Update()
    {
        gravityDir.transform.rotation = playerTransform.transform.rotation;
    }
}
