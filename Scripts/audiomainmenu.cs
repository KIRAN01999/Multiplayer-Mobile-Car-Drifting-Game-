using UnityEngine;

public class audiomainmenu: MonoBehaviour
{
    public AudioSource mainMenuAudio;

    void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (!mainMenuAudio.isPlaying)
            {
                mainMenuAudio.loop = true;
                mainMenuAudio.Play();
            }
        }
    }
}
