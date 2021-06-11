using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCatcher : MonoBehaviour
{

    public event Action<string> OnClientInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0)){
            OnClientInput?.Invoke(JsonUtility.ToJson(Input.mousePosition));
        }

        if(Input.GetMouseButtonUp(0)){
            var input = Input.mousePosition;
            input.z = 1;
            OnClientInput?.Invoke(JsonUtility.ToJson(input));
        }
    }
}
