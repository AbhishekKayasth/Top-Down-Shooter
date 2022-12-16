using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour {

    public Image fadeImage;
    public GameObject LvlCompleteUI;
    public GameObject gameOverUI;
    public GameObject hud;

    public TMP_Text Score;
    public TMP_Text money;
    public GameObject Player;

    Player player;
    RandomSpawner spawner;

	void Start () 
    {
        spawner = FindObjectOfType<RandomSpawner>();
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
	}

    void OnGameOver()
    {
        hud.SetActive(false);
        StartCoroutine(Fade(Color.clear, Color.black, 30));
        gameOverUI.SetActive(true);
        Cursor.visible = true;
    }

    public void OnLvlComplete()
    {
        hud.SetActive(false);
        player.enabled = false;
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, Color.black, 30));
        LvlCompleteUI.SetActive(true);
        StartCoroutine(Count(player.score, player.currentScore, Score));
        StartCoroutine(Count(player.money, player.currentMoney, money));
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime + speed;
            fadeImage.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    IEnumerator Count(int oldValue, int newValue, TMP_Text textObj)
    {
        while (oldValue != newValue)
        {
            oldValue++;
            textObj.text = oldValue.ToString();
            yield return null;
        }
    }

    //UI Input
    public void StartNewGame()
    {
        spawner.currentWaveNumber -= 1;
        LvlCompleteUI.SetActive(false);
        if (player == null)
        {
            Instantiate(Player, Vector3.zero, Quaternion.identity);
            spawner.ResetPlayerPosition();
        }
        player.enabled = true;
        fadeImage.color = Color.clear;
        hud.SetActive(true);
        spawner.NextWave();
        Cursor.visible = false;
    }

    public void NextWave()
    {
        LvlCompleteUI.SetActive(false);
        fadeImage.color = Color.clear;
        hud.SetActive(true);
        player.enabled = true;
        spawner.NextWave();
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

}
