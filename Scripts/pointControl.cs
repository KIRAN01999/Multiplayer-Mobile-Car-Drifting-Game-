using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pointControl : MonoBehaviour
{
    public static int coinCount;
    public Text coinCountDisplay;
    public Text coinEndDisplay;

    public Text totalCoinDisplay;
    void Update()
    {
        coinCountDisplay.text = coinCount.ToString();
        coinEndDisplay.text = coinCount.ToString();

        int totalCoins = PlayerPrefs.GetInt("c", 0); // "c" is just the internal key
        totalCoinDisplay.text = totalCoins.ToString();
    }
}
