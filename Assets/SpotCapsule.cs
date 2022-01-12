using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotCapsule : MonoBehaviour
{
    Renderer rend;
    public MeshRenderer meshRenderer;
    private bool isSet;
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool GetMode()
    {
        return isSet;
    }

    public void SetActive(bool isActive)
    {
        isSet = isActive;
        rend.enabled = isActive;
    }
    
    public float GetMeshSize()
    {
        float radius = rend.bounds.extents.magnitude;
        Debug.Log("Cylinder: " + Convert.ToString(radius));
        return radius;
    }
}
