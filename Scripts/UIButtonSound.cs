using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    public AudioSource clickSound;

    public void PlayClickSound()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}
