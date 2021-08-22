using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{

    public GameObject volumeVisualizer;
    private float normalizedValue;
    private float scale;
    public float max = 1.0f;
    public float min = 0.6f;
    private AudioManager audioManager;
    private AudioSource audioSource;
    private float volume;


    private void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        volume = audioManager.GetVolume(audioSource);
        scale = Mathf.Clamp(volume * (max - min) + min, min, max);
        volumeVisualizer.transform.localScale = new Vector3(scale, scale, 1.0f);
    }
}
