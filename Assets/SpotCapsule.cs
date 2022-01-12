using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotCapsule : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetActive(bool isActive)
    {
        GetComponent<Renderer>().enabled = isActive;
    }
}
