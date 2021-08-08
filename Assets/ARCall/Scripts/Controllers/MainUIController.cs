using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{

    private Button createBtn;
    private Button joinBtn;
    private Button recBtn;
    private Button signOffBtn;

    private void Awake() {
        createBtn = GameObject.Find("CreateBtn").GetComponent<Button>();
        joinBtn = GameObject.Find("JoinBtn").GetComponent<Button>();
        recBtn = GameObject.Find("RecBtn").GetComponent<Button>();
        signOffBtn = GameObject.Find("SignOffBtn").GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        createBtn.onClick.AddListener(()=>{
            MySceneManager.LoadScene("CreateRoom");
        });
        
        joinBtn.onClick.AddListener(()=>{
            MySceneManager.LoadScene("JoinRoom");
        });

        recBtn.onClick.AddListener(()=>{
            MySceneManager.LoadScene("Record");
        });

        signOffBtn.onClick.AddListener(()=>{
            UserManager.SignOut();
            MySceneManager.LoadScene("RegistroTlf");
        });
        
    }

}
