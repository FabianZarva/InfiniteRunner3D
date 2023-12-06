using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI leaderboardScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardNameText;

    private int score = 0;
    private int leaderboardID = 19159;
    private int leaderboardTopCount = 10;

    public void StopGame(int score)
    {        
        gameOverCanvas.SetActive(true);
        this.score = score;
        scoreText.text = score.ToString();
        SubmitScore();

    }
        
    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderboard());
    }

    private IEnumerator SubmitScoreToLeaderboard()
    {
        bool? nameSet = null;
        LootLockerSDKManager.SetPlayerName(inputField.text, (response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully set player's name.");
                nameSet = true;
            }
        else
            {
                Debug.Log("Name sending unsuccessful.");
                nameSet = false;
            }
        
        });
        yield return new WaitUntil(() => nameSet.HasValue);
        if (!nameSet.Value) yield break;
        bool? scoreSubmitted = null;
        LootLockerSDKManager.SubmitScore("", score, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully submitted player's score.");
                scoreSubmitted = true;
            }
            else
            {
                Debug.Log("Player's score sending unsuccessful.");
                scoreSubmitted = false;
            }
        });
        yield return new WaitUntil(() => scoreSubmitted.HasValue);
        if (!scoreSubmitted.Value) yield break;
        GetLeaderboard();
    }

    private void GetLeaderboard()
    {
        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), leaderboardTopCount, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully got the scores from leaderboard");
                string leaderboardName = "";
                string leaderboardScore = "";
                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; i++)
                { LootLockerPlayer player = members[i].player;
                    if (player == null) continue;

                    if (player.name != "")
                    {
                        leaderboardName += player.name + "\n";
                    }
                   else
                    {
                        leaderboardName += player.id + "\n";
                    }
                   leaderboardScore += members[i].score + "\n";              
                }

                leaderboardNameText.SetText(leaderboardName);
                leaderboardScoreText.SetText(leaderboardScore);
            }
            else
            {
                Debug.Log("Failed retrieving scores from leaderboard");
            }

        });
    }

    public void AddXP(int score)
    {

    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
