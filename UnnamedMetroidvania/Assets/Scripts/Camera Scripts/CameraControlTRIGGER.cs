using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

public class CameraControlTRIGGER : MonoBehaviour
{
    //https://www.youtube.com/watch?v=9dzBrLUIF8g (10:40-14:10)
    public CustomInspectorObjects customInspectorObjects;

    private Collider2D _coll;

    private void Start()
    {
        //Henter objektets kollider
        _coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Hvis objektet kommer i kontakt med et objekt med "Player" tagget
        if (collision.CompareTag("Player"))
        {
            //Hvis panCameraOnContact er sandt i inspectoren kører dette
            if (customInspectorObjects.panCameraOnContact)
            {
                //Sender værdierne til CameraManagerens "PanCameraOnContact"
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime, customInspectorObjects.panDirection, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Hvis objektet mister kontakt med et objekt med "Player" tagget
        if (collision.CompareTag("Player"))
        {
            //Laver en vektor mellem "Player" og midtpunktet af triggeren, som bliver normalized
            Vector2 exitDirection = (collision.transform.position - _coll.bounds.center).normalized;
            //Hvis SwapCameras og CameraOnLeft og CameraOnRight er sande eller har en værdi kører if-statementet
            if (customInspectorObjects.swapCameras && customInspectorObjects.cameraOnLeft != null && customInspectorObjects.cameraOnRight != null)
            {
                //Sender værdierne til CameraMangerens "SwapCamera"
                CameraManager.instance.SwapCamera(customInspectorObjects.cameraOnLeft, customInspectorObjects.cameraOnRight, exitDirection);

            }

            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime, customInspectorObjects.panDirection, true);
            }
        }
    }
}

//Laver inspector elementerne
[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}
//Laver en enum over PanDirection mulighederne
public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

//Laver en custom inspector som bruger værdierne fra swapCameras og panCameraOnContact til at udvide inspectoren
#if UNITY_EDITOR //Siger at det er UNITY_EDITOR der arbjedes med, så den kigger kun på UNITY_EDITOR ting
[CustomEditor(typeof(CameraControlTRIGGER))]
public class MyScriptEditor : Editor
{
    CameraControlTRIGGER cameraControlTrigger;

    //Når objekt bliver triggeret kører dette
    private void OnEnable()
    {
        //Opdatere cameraControlTigger til de nyeste værdier af CameraControlTRIGGER klassen og at den har adgang til de samme ting i inspectoren
        cameraControlTrigger = (CameraControlTRIGGER)target;
    }

    //Laver en custom inspector
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Hvis swapCameras er sandt, laver den to inputfields hvor den kun tager i mod et virtual camera, hvor den ene hedder "Camera on Left" og den anden "Camera on Right"
        if (cameraControlTrigger.customInspectorObjects.swapCameras)
        {
            cameraControlTrigger.customInspectorObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera on Left", cameraControlTrigger.customInspectorObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            cameraControlTrigger.customInspectorObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera on Right", cameraControlTrigger.customInspectorObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }
        //Hvis panCameraOnContact er sandt, laver den en enumPopup, ud fra PanDirection og 2 FloatFields, med panDistance og panTime
        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.customInspectorObjects.panDirection);

            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspectorObjects.panDistance);
            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObjects.panTime);
        }
        if (GUI.changed)
        {
            //Gemmer cameraControlTriggerens data når GUI'en bliver ændret
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif