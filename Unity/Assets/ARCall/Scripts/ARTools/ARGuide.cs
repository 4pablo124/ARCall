using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARGuide : MonoBehaviour
{
    [HideInInspector] public Transform target;

    private Camera arCam;
    private Vector2 targetScreenPos;
    private Vector3 guideScreenPos;
    private int cursorWidth;



    // Start is called before the first frame update
    void Start()
    {
        arCam = GameObject.Find("ARCamera").GetComponent<Camera>();
        guideScreenPos.z = 19;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        cursorWidth = (int)Math.Round(Screen.width*0.01f);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);

        targetScreenPos = arCam.WorldToScreenPoint(target.position);

        if((targetScreenPos.x > 0 && targetScreenPos.x < PeerConnection.width) &&
            targetScreenPos.y > 0 && targetScreenPos.y < PeerConnection.height){
                GetComponent<Renderer>().enabled = false;
        }else{
            GetComponent<Renderer>().enabled = true;


            guideScreenPos.x = Mathf.Clamp(targetScreenPos.x, 0, PeerConnection.width);
            guideScreenPos.y = Mathf.Clamp(targetScreenPos.y, 0, PeerConnection.height);

            transform.position = arCam.ScreenToWorldPoint(guideScreenPos);
            transform.LookAt(arCam.transform);

        }
    }
}
