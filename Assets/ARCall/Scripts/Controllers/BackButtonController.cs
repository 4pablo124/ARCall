using UnityEngine;

public class BackButtonController : MonoBehaviour {
    private void Start() {
        DontDestroyOnLoad(this.gameObject);    
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log("Back key pressed");
            UISceneNav.BackScene();
        }
    }
}
