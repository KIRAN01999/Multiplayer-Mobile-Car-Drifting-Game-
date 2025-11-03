using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class carselection : MonoBehaviour
{
    public GameObject[] cars; // Assign your 4 car prefabs in order
    public CinemachineVirtualCamera[] cameras; // Assign 4 Virtual Cameras
    private int selectedCarIndex = 0;
    public GameObject adpanelToActivate;
    public GameObject multiplayerPanelToActivate;



    void Start()
    {
        ShowSelectedCar();
    }

    void ShowSelectedCar()
    {
        // Enable the selected car camera, disable others
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == selectedCarIndex) ? 10 : 0;
        }
    }

    public void SelectNextCar()
    {
        selectedCarIndex = (selectedCarIndex + 1) % cars.Length;
        ShowSelectedCar();
    }

    public void SelectPreviousCar()
    {
        selectedCarIndex = (selectedCarIndex - 1 + cars.Length) % cars.Length;
        ShowSelectedCar();
    }

    public void OnRaceButtonClicked()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        SceneManager.LoadScene("Scene2"); // replace with your race scene name
    }
    public void Exit()
    {
        // This will work in a built game
        Application.Quit();
    }
    public void ads()
    {
        adpanelToActivate.SetActive(true);
    }
    public void multiplyerpanel()
    {
        multiplayerPanelToActivate.SetActive(true);
    }
    public void LoadScene1()
    {
        SceneManager.LoadScene("MainMenu"); // or use scene name: SceneManager.LoadScene("Scene1");
    }
}
