using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUpdate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
