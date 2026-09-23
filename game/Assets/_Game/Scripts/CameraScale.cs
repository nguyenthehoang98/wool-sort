using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScale : MonoBehaviour
{
    [SerializeField] private bool calculateFromAwake;
    [Header("Default")]
    [SerializeField] private Vector2 ratioDefaultVector2;
    [SerializeField] private float sizeCameraDefault = 10.8f;

    [Header("Current calculate")]
    public float sizeCameraCurrent;

    private float ratioDefault;
    private float ratioReal;

    private Camera currentCam;
    
    private void Awake()
    {
        currentCam = GetComponent<Camera>();
        if (calculateFromAwake)
        {
            CalculateCameraSize();
        }
    }

    public void CalculateCameraSize()
    {
        ratioDefault = ratioDefaultVector2.x/ratioDefaultVector2.y;
        Debug.Log("ratio default = " + ratioDefault);
        ratioReal = Screen.width/(float)Screen.height;
        Debug.Log("ratio real = " + ratioReal);
        var ratioDiff = ratioDefault / ratioReal;
        Debug.Log("ratio diff " + ratioDefault / ratioReal);
        if(ratioDiff > 1) // màn hình hẹp và dài hơn, cần căn chỉnh để khi zoom ra fit vừa màn hình
        {
            sizeCameraCurrent = sizeCameraDefault * ratioDiff;
        }
        else
        {
            sizeCameraCurrent = sizeCameraDefault;
        }

        currentCam.orthographicSize = sizeCameraCurrent;
    }
}