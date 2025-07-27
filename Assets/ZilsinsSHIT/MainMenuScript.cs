using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void Start()
    {
        SceneManager.LoadScene("FirstMission");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
