using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    #region Variables
    public AudioClip menuTheme;
    #endregion

    #region Unity Methods

    void Start()
    {
       // Screen.fullScreen = !Screen.fullScreen;
        AudioManager.instance.PlayMusic(menuTheme, 2);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

	#endregion

}
