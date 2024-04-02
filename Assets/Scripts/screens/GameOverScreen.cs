using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public Button buyLifeButton;
    public Button useLifeButton;
    public Button restartButton;
	public GameView gameView;

	GameManager gameManager;

    void Start()
    {
		gameManager = FindObjectOfType<GameManager>();
		gameManager.OnPlayerUpdated += OnPlayerUpdate;
		gameManager.OnPlayerReady += OnPlayerUpdate;
		if (gameManager.player != null) OnPlayerUpdate(gameManager.player);
		buyLifeButton.onClick.AddListener(OnBuyLife);
		useLifeButton.onClick.AddListener(OnUseLife);
		restartButton.onClick.AddListener(OnRestart);
    }

	void OnDestroy(){
		gameManager.OnPlayerUpdated -= OnPlayerUpdate;
		gameManager.OnPlayerReady -= OnPlayerUpdate;
	}

    void OnBuyLife()
	{
		buyLifeButton.interactable = false;
		StartCoroutine(gameManager.BuyLife(() => {
			buyLifeButton.interactable = gameManager.player.coins >= GameManager.LIFE_PRICE ? true : false;
		}));
	}

	void OnUseLife()
	{
		useLifeButton.interactable = false;
		StartCoroutine(gameManager.UseLife(() => {
			useLifeButton.interactable = gameManager.player.lifeCoins > 0 ? true : false;
			gameManager.ContinueGame();
		}));
	}

	void OnRestart()
	{
		gameView.ShowLevels();
	}

	void OnPlayerUpdate(Player player)
	{
		buyLifeButton.interactable = player.coins >= GameManager.LIFE_PRICE ? true : false;
		useLifeButton.interactable = gameManager.player.lifeCoins > 0 ? true : false;
		if (player.lifeCoins > 0)
		{
			buyLifeButton.gameObject.SetActive(false);
			useLifeButton.gameObject.SetActive(true);
		}
		else
		{
			buyLifeButton.gameObject.SetActive(true);
			useLifeButton.gameObject.SetActive(false);
		}
	}
}
