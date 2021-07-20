using UnityEngine;

public class BackButtonManager : MonoBehaviour {
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
