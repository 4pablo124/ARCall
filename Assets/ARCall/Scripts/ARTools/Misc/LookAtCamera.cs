using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cam.transform);
    }
}
