﻿using UnityEngine;
using UnityHelpers;

public class MouseToValueManager : MonoBehaviour
{
    [RequireInterface(typeof(IValueManager))]
    public GameObject affectedObject;
    public IValueManager valueManager;
    private Camera viewingCamera;

    private Vector2 prevMousePos;

    void Start()
    {
        if (valueManager == null)
            valueManager = affectedObject.GetComponent<IValueManager>();
        viewingCamera = FindObjectOfType<Camera>();
    }

    void Update()
    {
        Vector3 projectedPoint = viewingCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewingCamera.transform.position.y));
        valueManager.SetAxis("Horizontal", projectedPoint.x);
        valueManager.SetAxis("Vertical", projectedPoint.z);
        valueManager.SetToggle("Grab", Input.GetMouseButton(0));

        prevMousePos = Input.mousePosition;
    }
}
