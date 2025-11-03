using Photon.Pun;
using UnityEngine;
using System.Collections;

public class NetworkCarSpawner: MonoBehaviour
{
    // Array of car prefab names - MUST match EXACTLY:
    // 1. The GameObject names in Scene 1 cars array (index 0 = first car, etc.)
    // 2. The prefab names in your Resources folder
    public string[] carPrefabNames = { "car5", "car2", "car3", "car4", "car1" };

    public Transform[] spawnPoints; // Assign 2–4 spawn points in Inspector

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnSelectedCar();
        }
        else
        {
            Debug.LogError("Not connected to Photon — cannot spawn car!");
        }
    }

    void SpawnSelectedCar()
    {
        // Get the selected car index from PlayerPrefs (saved in car selection scene)
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        // Validate the index
        if (selectedCarIndex < 0 || selectedCarIndex >= carPrefabNames.Length)
        {
            Debug.LogWarning($"Invalid car index {selectedCarIndex}, defaulting to car 0");
            selectedCarIndex = 0;
        }

        // Get the car name to spawn
        string carToSpawn = carPrefabNames[selectedCarIndex];

        // Choose random spawn point
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Debug.Log($"Attempting to spawn car: {carToSpawn} (index {selectedCarIndex})");

        // Spawn the selected car over network
        GameObject spawnedCar = PhotonNetwork.Instantiate(
            carToSpawn,
            spawn.position,
            spawn.rotation
        );

        if (spawnedCar != null)
        {
            Debug.Log($"Successfully spawned: {carToSpawn}");

            // Find and setup camera for the spawned car
            StartCoroutine(SetupCameraAfterSpawn(spawnedCar));
        }
        else
        {
            Debug.LogError($"Failed to spawn car: {carToSpawn}. Make sure it exists in Resources folder!");
        }
    }

    IEnumerator SetupCameraAfterSpawn(GameObject car)
    {
        // Wait one frame to ensure PhotonView is initialized
        yield return null;

        // Only setup camera for our own car
        PhotonView pv = car.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            camera cameraScript = FindObjectOfType<camera>();
            if (cameraScript != null)
            {
                cameraScript.SetupCamera(car);
                Debug.Log("Camera setup for local player's car");
            }
        }
    }
}