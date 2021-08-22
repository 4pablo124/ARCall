using System.Linq;
using UnityEngine;

public class BackButtonController : MonoBehaviour
{

    private readonly string[] invalidScenes = new[]{
        "RegisterPhone",
        "RegisterName"
    };

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!invalidScenes.Contains(MySceneManager.CurrentScene().name))
            {
                MySceneManager.BackScene();
            }
        }
    }
}
