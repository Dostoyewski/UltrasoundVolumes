using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var width = Screen.currentResolution.width;
        var height = Screen.currentResolution.height;
        Debug.Log("Screen Res : " + Screen.currentResolution );
        Screen.SetResolution(width / 2, height, false, 60);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
