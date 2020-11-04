using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public HUD hud;
    public GridManager grid;
    public int timeToPush;
    public int targetScore;
    public Transform gameOverMark;

    private float timer;
    private bool gameOver;

    protected int currentScore;

    private void OnEnable()
    {
        grid.OnLimitReached += GameOverHandler;
        grid.OnPieceCleared += PieceClearedHandler;
        hud.OnPushClicked += PushColumnHandler;
    }

    private void OnDestroy()
    {
        grid.OnLimitReached -= GameOverHandler;
        grid.OnPieceCleared -= PieceClearedHandler;
        hud.OnPushClicked -= PushColumnHandler;
    }

    void Start()
    {
        hud.SetScore(currentScore);
        hud.SetTarget(targetScore);
        hud.SetRemaining(string.Format("{0}:{1:00}", timeToPush / 60, timeToPush % 60));
        gameOverMark.position = grid.GetWorldPosition(grid.maxWidth - 1, 2);
    }

    private void PieceClearedHandler(int pieceScore)
    {
        currentScore += pieceScore;
        hud.SetScore(currentScore);
    }

    private void GameOverHandler()
    {
        hud.OnGameLose();
        gameOver = true;
    }

    private void PushColumnHandler()
    {
        timer = 0;
        grid.PushColumns();
    }

    void Update()
    {
        if(gameOver)
        {
            return;
        }

        timer += Time.deltaTime;
        hud.SetRemaining(string.Format("{0}:{1:00}", (int)Mathf.Max((timeToPush - timer) / 60, 0), (int)Mathf.Max((timeToPush - timer) % 60, 0)));

        if (timeToPush - timer <= 0)
        {
            timer = 0;
            PushColumnHandler();
        }
    }
}
