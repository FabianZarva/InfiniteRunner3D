using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
   
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI leaderboardScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardNameText;

    // Variables for scoring and appearing on the leaderboard
    private int score = 0; // starting score
    private int leaderboardID = 19159; // Unique player ID, given by Lootlocker on creating an account for game development
    private int leaderboardTopCount = 10;

    // When the game is over, displays the score, submits name and/or new score to leaderboard, and adds that score as XP using Lootlocker's SDK
    public void StopGame(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        GetLeaderboard();
        AddXP(score);
    }

    // Starting the submitting score coroutine
    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderboard());
    }

    // Method of submitting the player's score to the leaderboard, on game over canvas
    private IEnumerator SubmitScoreToLeaderboard()
    {
        bool? nameSet = null;

        // Sets the player's name using LootLocker SDK
        LootLockerSDKManager.SetPlayerName(inputField.text, (response) =>
        {
            if (response.success)
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

        // Waits until the game sends the player name  to the leaderboard on the screen
        yield return new WaitUntil(() => nameSet.HasValue);

        // If the name was not submitted succesfully, breaks the coroutine
        if (!nameSet.Value) yield break;

        bool? scoreSubmitted = null;

        // Submits the player's score using LootLocker SDK
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

        // Waits until the game sends the score value to the leaderboard on the screen
        yield return new WaitUntil(() => scoreSubmitted.HasValue);

        // If the score was not submitted succesfully, breaks the coroutine
        if (!scoreSubmitted.Value) yield break;

        // Refreshes the leaderboard after submitting the score
        GetLeaderboard();
    }

    // Gets leaderboard information using LootLocker's SDK
    private void GetLeaderboard()
    {
        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), leaderboardTopCount, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully got the scores from leaderboard");

                // Displays leaderboard information(my highest score)
                string leaderboardName = "";
                string leaderboardScore = "";
                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; i++)
                {
                    LootLockerPlayer player = members[i].player;
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

    // Adds XP based on the player's score using LootLocker's SDK
    public void AddXP(int score)
    {
        LootLockerSDKManager.SubmitXp(score, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully added XP.");
            }
            else
            {
                Debug.Log("Failed adding XP.");
            }
        });
    }

    // Restarts the game
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
