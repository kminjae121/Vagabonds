using UnityEngine;
using UnityEngine.SceneManagement;

public class EndUiFunction : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
