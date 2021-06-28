using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class testline : MonoBehaviour
{

    public GameObject drawings;
    public GameObject linePrefab;
    private LineRenderer line;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()  {
        if(Input.GetMouseButton(0)){
            RaycastHit hitResults; 

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hitResults)){
                if(line == null) {
                    line = createLineStart(hitResults.point, Color.red);
                }else{
                    drawNextPointInLine(line,hitResults.point);
                }
            }
        }else if(line != null) {
            var lineClone = GameObject.Instantiate(line);
            lineClone.transform.parent = drawings.transform;
            
            Destroy(line.gameObject);
        }
    }

    
    private LineRenderer drawNextPointInLine(LineRenderer line, UnityEngine.Vector3 point){
        line.SetPosition(line.positionCount++,point);
        line.Simplify(0.00001f);
        return line;
    }

    private LineRenderer createLineStart(UnityEngine.Vector3 start,Color color){
        LineRenderer line = GameObject.Instantiate(linePrefab, start, UnityEngine.Quaternion.identity).GetComponent<LineRenderer>();
        line.endColor = line.startColor = color;
        line.SetPosition(0,start);
        return line;
    }
}
