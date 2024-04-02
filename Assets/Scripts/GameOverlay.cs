using UnityEngine;
using TMPro;

public class GameOverlay : MonoBehaviour
{
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
		gameManager.OnPlayerUpdated += OnPlayerUpdate;
    }

    void OnDestroy()
    {
		gameManager.OnPlayerUpdated -= OnPlayerUpdate;
    }

	public void OnPlayerUpdate(Player player)
	{
		levelText.text = player.currentLevel.ToString();
		livesText.text = player.lives.ToString();
		scoreText.text = player.score.ToString();
		percentText.text = Mathf.RoundToInt(player.percent * 100) + "%";
	}
}

