using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    //https://www.youtube.com/watch?v=9dzBrLUIF8g (6:00-7:46)
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirturalCameras;

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    private float _normYPanAmount;

    private Vector2 _startingTrackedObjectOffset;

    private void Awake()
    {
        //Tjekker om der er en instance af CameraManager i starten af scenen
        if (instance == null)
        {
            instance = this;
        }
        //Laver et for-loop af længden af vores _allVirturalCameras array
        for (int i = 0; i < _allVirturalCameras.Length; i++)
        {
            if (_allVirturalCameras[i].enabled)
            {
                //Sætter det nuværende kamera, der er slået til i inspectoren
                _currentCamera = _allVirturalCameras[i];

                //Sætter _framingTransposer til de værdier der er angivet på VirtualCameraet's body
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }
        //Sætter YDamping mængden så det passer overens med kameraet inspector værdi
        _normYPanAmount = _framingTransposer.m_YDamping;

        //Sætter offsettet af objektet kameraet følger til at passe overens med inspector værdien
        _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;
    }
    #region Lerp the Y Damping
    //Når spilleren falder kører dette. Dette tjekkes for i "CharacterController2D" scriptet
    public void LerpYDamping(bool isPlayerFalling)
    {
        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }
    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;
        //Henter damp mængden angivet i cameraet
        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        //Bestemmer slut damp mængden ud fra om spilleren falder eller ej
        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normYPanAmount;
        }
        //Lerper den bestemte pan mængde
        float elapsedTime = 0f;
        //Lerper med en hastighed der svarer til angivet _fallYPanTime
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            //Lerper mellem tiden der er gået og slutmængden
            float lerpedPanAmount = Mathf.Lerp(elapsedTime, endDampAmount, (elapsedTime / _fallPanAmount));
            //Sætter YDamping til hvad udregningen ovenover giver
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
    #endregion

    #region Pan Camera
    //Henter værdierne fra "CameraControlTRIGGER", når spilleren rør triggeren
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        //Bestemmer PanDirection der skal panes når spilleren rør triggeren
        if (!panToStartingPos)
        {
            //Sætter retningen og afstanden
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
                default:
                    break;
            }
            //endPos bliver skaleret op med panDistance
            endPos *= panDistance;

            startingPos = _startingTrackedObjectOffset;

            //Gøre så endPos også har offsettet der er bestemt i inspectoren
            endPos += startingPos;
        }
        //Hvis spilleren forlader triggeren bliver alt sat "tilbage"
        else
        {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        //Paner kameraet ud fra de oplyste værdier ovenover
        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            _framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
    #endregion

    #region Swap Cameras
    //Henter værdierne fra "CameraControlTRIGGER", hvis der skal swappes kameraer
    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        //Hvis det nuværende kamera er det venstre kamerea og vi forlader triggeren til højre kører dette
        if (_currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            //Aktivere det nye kamerea, det kamera der er sat som højre kameraet
            cameraFromRight.enabled = true;

            //Deaktivere det gamle kamera, det kamera der er sat som venstre
            cameraFromLeft.enabled = false;

            //Opdatere _currentCamera til det nye kamera
            _currentCamera = cameraFromRight;

            //Opdatere _framingTransposer til at passe overens med det nye kameras værdier
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        //Hvis det nuværende kamera er det højre og vi forlader triggeren fra venstre kører dette
        else if (_currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            //Aktivere det nye kamerea, det kamera der er sat som venste kameraet
            cameraFromLeft.enabled = true;

            //Deaktivere det gamle kamera, det kamera der er sat som højre
            cameraFromRight.enabled = false;

            //Opdatere _currentCamera til det nye kamera
            _currentCamera = cameraFromLeft;

            //Opdatere _framingTransposer til at passe overens med det nye kameras værdier
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    #endregion
}