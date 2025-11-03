using Cinemachine;
using System.Collections;
using UnityEngine;

public class camera : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineVirtualCamera virtualCamera;
    public Camera mainCamera;

    [Header("Camera Settings")]
    public Vector3 cameraRotation = new Vector3(30f, 45f, 0f);
    public float orthographicSize = 10f;
    public float cameraDistance = 20f;

    [Header("Following Settings")]
    public float followSpeed = 2f;
    public Vector3 offset = Vector3.zero;

    private Transform carTarget;
    private bool cameraSetup = false;

    void Start()
    {
        // Start checking for the local player's car
        StartCoroutine(FindLocalPlayerCar());
    }

    IEnumerator FindLocalPlayerCar()
    {
        // Wait until we find a car with PhotonView.IsMine = true
        GameObject localCar = null;

        while (localCar == null)
        {
            // Find all objects tagged as "Player"
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject car in cars)
            {
                var photonView = car.GetComponent<Photon.Pun.PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    localCar = car;
                    break;
                }
            }

            // Wait a bit before checking again
            if (localCar == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Setup camera for the local player's car
        SetupCamera(localCar);
    }

    public void SetupCamera(GameObject car)
    {
        if (cameraSetup) return; // Prevent multiple setups

        // Get or create follow target
        Transform followTarget = car.transform.Find("FollowTarget");
        if (followTarget == null)
        {
            GameObject followTargetObj = new GameObject("FollowTarget");
            followTargetObj.transform.SetParent(car.transform);
            followTargetObj.transform.localPosition = offset;
            followTarget = followTargetObj.transform;
        }

        carTarget = followTarget;

        // Setup main camera as orthographic
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = orthographicSize;
        }

        // Setup virtual camera
        SetupVirtualCamera();

        cameraSetup = true;
        Debug.Log("Camera setup complete for local player car");
    }

    void SetupVirtualCamera()
    {
        if (virtualCamera == null) return;

        // Set targets
        virtualCamera.Follow = carTarget;
        virtualCamera.LookAt = carTarget;

        // Set rotation
        virtualCamera.transform.rotation = Quaternion.Euler(cameraRotation);

        // Configure lens for orthographic
        var lens = virtualCamera.m_Lens;
        lens.Orthographic = true;
        lens.OrthographicSize = orthographicSize;
        virtualCamera.m_Lens = lens;

        // Setup Framing Transposer if available
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            framingTransposer.m_CameraDistance = cameraDistance;
            framingTransposer.m_ScreenX = 0.5f;
            framingTransposer.m_ScreenY = 0.5f;
            framingTransposer.m_DeadZoneWidth = 0.1f;
            framingTransposer.m_DeadZoneHeight = 0.1f;
            framingTransposer.m_SoftZoneWidth = 0.8f;
            framingTransposer.m_SoftZoneHeight = 0.8f;
            framingTransposer.m_XDamping = followSpeed;
            framingTransposer.m_YDamping = followSpeed;
            framingTransposer.m_ZDamping = followSpeed;
        }

        // Set high priority
        virtualCamera.Priority = 10;

        // Force update
        virtualCamera.PreviousStateIsValid = false;
    }

    // Optional: Call this if you need to reassign target
    public void AssignNewTarget(GameObject newCar)
    {
        cameraSetup = false;
        SetupCamera(newCar);
    }
}