using System;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public GameOver gameOver;

    public Text remainingText;
    public Text targetText;
    public Text scoreText;
    public Button pushButton;

    public Action OnPushClicked;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetTarget(int target)
    {
        targetText.text = target.ToString();
    }

    public void SetRemaining(string remaining)
    {
        remainingText.text = remaining;
    }

    public void OnGameLose()
    {
        gameOver.ShowLose();
    }

    public void OnGameWin(int score)
    {
        gameOver.ShowWin(score);
    }

    public void OnPushButtonClicked()
    {
        OnPushClicked?.Invoke();
    }

    public void EnablePushButton()
    {
        pushButton.interactable = true;
    }
}
