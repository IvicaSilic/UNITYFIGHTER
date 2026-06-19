using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    public FighterController player1;
    public FighterController player2;

    [Header("UI")]
    public Slider healthBarP1;
    public Slider healthBarP2;

    public TextMeshProUGUI winText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public Button restartButton;

    [Header("Rounds")]
    public int p1Rounds = 0;
    public int p2Rounds = 0;
    public int roundsToWin = 2;

    private float timeLeft = 99f;

    private bool gameOver = false;
    private bool roundStarting = false;
    private bool matchEnded = false;

    void Awake()
    {
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
    }

    void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Players nisu postavljeni!");
            return;
        }

        if (healthBarP1 != null)
            healthBarP1.maxValue = player1.maxHealth;

        if (healthBarP2 != null)
            healthBarP2.maxValue = player2.maxHealth;

        ResetRoundState();
        StartCoroutine(StartRoundCountdown());
    }

    void Update()
    {
        if (roundStarting || gameOver || matchEnded)
            return;

        if (healthBarP1 != null)
            healthBarP1.value = player1.currentHealth;

        if (healthBarP2 != null)
            healthBarP2.value = player2.currentHealth;

        timeLeft -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
        {
            if (player1.currentHealth > player2.currentHealth)
                EndRound("PLAYER 1 WINS!");
            else if (player2.currentHealth > player1.currentHealth)
                EndRound("PLAYER 2 WINS!");
            else
                EndRound("DRAW!");

            return;
        }

        if (player1.currentHealth <= 0)
            EndRound("PLAYER 2 WINS!");

        if (player2.currentHealth <= 0)
            EndRound("PLAYER 1 WINS!");
    }

    IEnumerator StartRoundCountdown()
    {
        roundStarting = true;

        player1.inputLocked = true;
        player2.inputLocked = true;

        if (roundText != null)
            roundText.text = "ROUND " + (p1Rounds + p2Rounds + 1);

        yield return new WaitForSeconds(1f);

        if (winText != null) winText.text = "3";
        yield return new WaitForSeconds(1f);

        if (winText != null) winText.text = "2";
        yield return new WaitForSeconds(1f);

        if (winText != null) winText.text = "1";
        yield return new WaitForSeconds(1f);

        if (winText != null) winText.text = "FIGHT!";
        yield return new WaitForSeconds(0.8f);

        if (winText != null) winText.text = "";

        roundStarting = false;

        player1.inputLocked = false;
        player2.inputLocked = false;
    }

    void EndRound(string result)
    {
        if (gameOver || matchEnded)
            return;

        gameOver = true;

        if (winText != null)
            winText.text = result;

        if (result.Contains("PLAYER 1"))
            p1Rounds++;
        else if (result.Contains("PLAYER 2"))
            p2Rounds++;

        Invoke(nameof(CheckMatchWinner), 2f);
    }

    void CheckMatchWinner()
    {
        if (matchEnded)
            return;

        if (p1Rounds >= roundsToWin)
        {
            ShowMatchEnd("PLAYER 1 CHAMPION!");
            return;
        }

        if (p2Rounds >= roundsToWin)
        {
            ShowMatchEnd("PLAYER 2 CHAMPION!");
            return;
        }

        StartNextRound();
    }

    void ShowMatchEnd(string text)
    {
        matchEnded = true;

        if (winText != null)
            winText.text = text;

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }

    void StartNextRound()
    {
        ResetRoundState();
        StartCoroutine(StartRoundCountdown());
    }

    void ResetRoundState()
    {
        gameOver = false;
        roundStarting = true;

        timeLeft = 99f;

        Time.timeScale = 1f;

        if (player1 != null)
            player1.ResetFighter(new Vector3(-3, 0, 0));

        if (player2 != null)
            player2.ResetFighter(new Vector3(3, 0, 0));

        if (timerText != null)
            timerText.text = "99";

        if (winText != null)
            winText.text = "";

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}