using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGamePlayScene()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
