using UnityEngine;
using Photon.Pun; // ✅ Required for PhotonNetwork.Instantiate
using Cinemachine;

public class CarSpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] cars;              // Assign your car prefabs in Inspector
    public Transform spawnPoint;           // Assign your spawn point in Inspector
    private camera cameraManager;          // Reference to your camera follow script

    void Start()
    {
        // Find the camera manager in the scene
        cameraManager = FindObjectOfType<camera>();

        // Wait until connected to Photon before spawning
        if (PhotonNetwork.InRoom)
        {
            SpawnCar();
        }
        else
        {
            Debug.LogWarning("Not yet in a Photon room — car will spawn after joining.");
        }
    }

    void SpawnCar()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        // Ensure valid index
        if (selectedIndex >= cars.Length)
            selectedIndex = 0;

        // ✅ Instantiate the car over the network
        GameObject spawnedCar = PhotonNetwork.Instantiate(
            cars[selectedIndex].name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Tag as "Player" for camera reference
        spawnedCar.tag = "Player";

        // Assign camera follow target to the local player's car
        if (cameraManager != null)
        {
            cameraManager.AssignNewTarget(spawnedCar);
        }

        // Only enable movement for the local player's car
        PhotonView view = spawnedCar.GetComponent<PhotonView>();
        carController controller = spawnedCar.GetComponent<carController>();

        if (view != null && controller != null)
        {
            controller.enabled = view.IsMine;
        }

        // Play engine sound (optional)
        AudioSource carAudio = spawnedCar.GetComponent<AudioSource>();
        if (carAudio != null && view != null && view.IsMine)
            carAudio.Play();

        Debug.Log("Spawned car for player: " + PhotonNetwork.NickName);
    }

    // Optional: handle when player joins the room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room! Spawning car...");
        SpawnCar();
    }
}
