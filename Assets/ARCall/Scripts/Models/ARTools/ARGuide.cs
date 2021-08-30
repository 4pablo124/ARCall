using System;
using UnityEngine;

/// <summary>
/// Modela el comportamiento de la herramienta ARGuide
/// </summary>
public class ARGuide : MonoBehaviour
{
    [HideInInspector] public Transform target;

    private Camera arCam;
    private Vector2 targetScreenPos;
    private Vector3 guideScreenPos;
    private int cursorWidth;

    private VideoManager videoManager;


    /// <summary>
    /// Llamada al crear el <see cref="GameObject"/> asociado
    /// <para>Inicializa los modelos referenciados</para>
    /// </summary>
    private void Awake()
    {
        videoManager = GameObject.Find("VideoManager")?.GetComponent<VideoManager>();
        arCam = GameObject.Find("ARCamera").GetComponent<Camera>();
    }

    /// <summary>
    /// Llamada justo antes del primer fotograma
    /// <para>Establece los valores iniciales</para>
    /// </summary>
    private void Start()
    {
        GetComponent<Renderer>().enabled = false;
        guideScreenPos.z = 19;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Llamada al comienzo de cada fotograma
    /// </summary>
    private void Update()
    {
        cursorWidth = (int)Math.Round(Screen.width * 0.01f);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorWidth);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorWidth);

        targetScreenPos = arCam.WorldToScreenPoint(target.position);


        int width;
        int height;
        if (videoManager != null)
        {
            width = videoManager.width;
            height = videoManager.height;
        }
        else
        {
            width = Screen.width;
            height = Screen.height;
        }

        if ((targetScreenPos.x > 0 && targetScreenPos.x < width) &&
            targetScreenPos.y > 0 && targetScreenPos.y < height)
        {
            GetComponent<Renderer>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;


            guideScreenPos.x = Mathf.Clamp(targetScreenPos.x, 0, width);
            guideScreenPos.y = Mathf.Clamp(targetScreenPos.y, 0, height);

            transform.position = arCam.ScreenToWorldPoint(guideScreenPos);
            transform.LookAt(arCam.transform);

        }
    }
}
