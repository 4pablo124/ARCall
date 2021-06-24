using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class testline : MonoBehaviour
{

    public GameObject drawings;
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()  {
        if(Input.GetMouseButton(0)){
            var marker = GameObject.Instantiate(prefab,Input.mousePosition,Quaternion.identity);
            marker.transform.parent = drawings.transform;

            int count = 0;
            foreach (Transform child in drawings.transform)
            {
                if(child.gameObject.tag == "Marker") count++;
            }
            Debug.Log(count);
        }
    }
}
