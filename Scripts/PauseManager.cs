using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel; // Assign your Pause UI Panel in the Inspector
    private bool isPaused = false;
    private AudioSource carAudio;

    void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Ensure the game is running

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            carAudio = player.GetComponent<AudioSource>();
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze the game

        if (carAudio != null)
        {

            carAudio.Pause();

        }
    }

        public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game

        if (carAudio != null)
        {
            carAudio.UnPause();
        }
            
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Unpause before reloading
        pointControl.coinCount = 0; // Reset coins
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        pointControl.coinCount = 0;// Unpause before going to menu
        SceneManager.LoadScene(0); // Assumes Main Menu is Scene 0 in Build Settings
    }
    public void Exit()
    {
        // This will work in a built game
        Application.Quit();
    }
}
