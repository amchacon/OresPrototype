using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public GameObject screenParent;
    public GameObject scoreParent;
    public Text loseText;
    public Text scoreText;

    // Use this for initialization
    void Start()
    {
        screenParent.SetActive(false);
    }

    public void ShowLose()
    {
        screenParent.SetActive(true);
        scoreParent.SetActive(false);

        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play("GameOverShow");
        }
    }

    public void ShowWin(int score)
    {
        screenParent.SetActive(true);
        loseText.enabled = false;

        scoreText.text = score.ToString();
        scoreText.enabled = false;

        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play("GameOverShow");
        }

        StartCoroutine(ShowWinCoroutine());
    }

    private IEnumerator ShowWinCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        scoreText.enabled = true;
    }

    public void OnReplayClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnDoneClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
