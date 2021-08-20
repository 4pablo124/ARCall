using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{

    public GameObject volume;
    private float normalizedValue;
    private float scale;
    public float max = 1.0f;
    public float min = 0.75f;
    private AudioManager audioManager;
    private AudioSource audioSource;
    private float[] spectrum;
    private float spectrumValue;


    private void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioSource = gameObject.GetComponent<AudioSource>();

        spectrum = new float[128];
    }

    void FixedUpdate()
    {
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);
        float sum = 0.0f;
        foreach (var sample in spectrum)
        {
            sum += sample * sample;
        }
        spectrumValue = Mathf.Sqrt(sum / spectrum.Length) * 1000;

        scale = Mathf.Clamp(spectrumValue * (max - min) + min, min, max);
        volume.transform.localScale = new Vector3(scale, scale, 1.0f);
    }
}
