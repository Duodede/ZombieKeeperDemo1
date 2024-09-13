using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUpGPSTarget : MonoBehaviour
{
    public GPSManager.IconType iconType;
    public bool isAlwaysShow;
    void Start()
    {
        this.Invoke("SetUp",0.5f);
    }
    void SetUp()
    {
        GPS.manager.ShowObjectInGPS(this.gameObject, iconType, isAlwaysShow);
    }
}
