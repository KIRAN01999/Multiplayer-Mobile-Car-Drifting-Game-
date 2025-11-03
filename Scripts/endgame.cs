using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class endgame : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverPanel;

    [Header("Score Display")]
    public Text counterText;              // Counter during gameplay
    public Text finalScoreText;           // Score shown on game over panel
    public Text highestDistanceText;      // High score on game over panel

    [Header("Settings")]
    public float fallYThreshold = -5f;
    private float speed = 100f;

    private Transform playerCar;
    private bool isGameOver = false;
    private int counter = 0;

    void Start()
    {
        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Find player car
        StartCoroutine(FindPlayerCar());
    }

    IEnumerator FindPlayerCar()
    {
        // Wait for car to spawn
        while (playerCar == null)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject car in cars)
            {
                PhotonView pv = car.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    playerCar = car.transform;
                    Debug.Log("Found local player car");
                    break;
                }
            }

            if (playerCar == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void Update()
    {
        if (isGameOver || playerCar == null)
            return;

        // Score counter
        counter += Mathf.RoundToInt(speed * Time.deltaTime);

        if (counterText != null)
            counterText.text = counter.ToString();

        // Backup fall check
        if (playerCar.position.y <= fallYThreshold)
        {
            PhotonView photonView = playerCar.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine && !isGameOver)
            {
                photonView.RPC("PlayerFell", RpcTarget.All);
            }
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("Game Over! Someone fell!");

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Display final score
        if (finalScoreText != null)
        {
            finalScoreText.text = counter.ToString();
        }

        // Stop car audio if car exists
        if (playerCar != null)
        {
            AudioSource carAudio = playerCar.GetComponent<AudioSource>();
            if (carAudio != null)
            {
                carAudio.Stop();
            }
        }

        // Update high score
        int highest = PlayerPrefs.GetInt("h", 0);
        if (counter > highest)
        {
            PlayerPrefs.SetInt("h", counter);
            PlayerPrefs.Save();
            highest = counter;
        }

        if (highestDistanceText != null)
        {
            highestDistanceText.text = highest.ToString();
        }

        // Stop time after everything is set
        Time.timeScale = 0f;
    }

    // Button methods for the game over panel
    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}