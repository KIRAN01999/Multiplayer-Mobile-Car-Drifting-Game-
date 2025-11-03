using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectcoin : MonoBehaviour
{
    public AudioSource coinFX;

   

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensures only car triggers it
        {
            if (coinFX != null)
                coinFX.Play();

            pointControl.coinCount += 1;
            int total = PlayerPrefs.GetInt("c", 0);
            PlayerPrefs.SetInt("c", total + 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false); // Hide the coin
        }
    }
}

