using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("End Game")]
    [SerializeField] private GameObject _endGameContainer;
    [SerializeField] private TextMeshProUGUI _highScoreText;

    [Range(5, 120)]
    [SerializeField] private float _maxTimeSeconds;
    private float _currentTime;

    private void Start()
    {
        Time.timeScale = 1.0f;
        _currentTime = _maxTimeSeconds;
        _endGameContainer.SetActive(false);
        ScoreManager.Instance.OnEnemyCountChanged += ScoreManager_OnEnemyCountChanged;
    }

    private void ScoreManager_OnEnemyCountChanged(object sender, System.EventArgs e)
    {
        ChangeEnemyText(ScoreManager.Instance.GetKilledEnemyCount());
    }

    private void Update()
    {
        _currentTime -= Time.deltaTime;

        ShowTime();
    }

    private void ShowTime()
    {
        float time = _currentTime;
        if (time <= 60)
        {
            _timeText.text = Mathf.Ceil(time).ToString();
        }
        else
        {
            float minutes = Mathf.Floor(time / 60);
            float seconds = Mathf.Ceil(time % 60);
            string second = "";

            if (seconds < 10) second = "0" + seconds;
            else second = seconds.ToString();

            _timeText.text = minutes.ToString() + ":" + second;
        }

        if (time <= 0)
        {
            EndGame();
        }
    }

    private void ChangeEnemyText(int enemyCount)
    {
        _enemyCountText.text = "Enemy: " + enemyCount;
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void EndGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _endGameContainer.SetActive(true);
        int oldScore = PlayerPrefs.GetInt("Score");
        if (ScoreManager.Instance.GetKilledEnemyCount() > oldScore)
            PlayerPrefs.SetInt("Score", ScoreManager.Instance.GetKilledEnemyCount());

        _highScoreText.text = "High Score: " + PlayerPrefs.GetInt("Score").ToString();

        EnemySpawnerPool.Instance.DanceAllEnemies();
        Time.timeScale = 0f;
    }
}
