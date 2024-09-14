using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterWaveController : MonoBehaviour
{
    public Rigidbody rg;
    public CharacterController controller;
    public WaterDeformer waterDeformer;
    public bool isPlayer;
    public float standardAmplitude;//速度为2.5时的浪高
    float factor;
    private void Start()
    {
        factor = standardAmplitude / 2.5f;
        this.TryGetComponent<Rigidbody>(out rg);
        this.TryGetComponent<CharacterController>(out controller);
    }
    private void Update()
    {
        if(isPlayer)
        {
            waterDeformer.amplitude = Mathf.Lerp(waterDeformer.amplitude,controller.velocity.magnitude * factor,0.05f);
            WaveDirection(controller.velocity);
        }
        else
        {
            waterDeformer.amplitude = Mathf.Lerp(waterDeformer.amplitude, rg.velocity.magnitude * factor, 0.05f);
            WaveDirection(rg.velocity);
        }
    }
    void WaveDirection(Vector3 velocity)
    {
        waterDeformer.transform.rotation = Quaternion.LookRotation(-velocity.normalized);
    }
}
