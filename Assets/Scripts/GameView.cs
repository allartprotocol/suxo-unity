using System.Linq;
using AllArt.SUI.RPC.Response;
using SimpleScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : BaseScreen
{
    public GameManager gameManagar;

    public TextMeshProUGUI coins;
    public TextMeshProUGUI lives;
    
	public Button walletButton;
    public Button startLevel;
    public Button restartGame;
    public Button continueBtn;

    public CongratsScreen congratsScreen;
    public LandingScreen landingScreen;
    public GameOverScreen gameOverScreen;
    
    void Start()
    {
        walletButton.onClick.AddListener(OnWallet);
		gameManagar.OnPlayerReady  += OnPlayerReady;
		gameManagar.OnPlayerUpdated += OnPlayerUpdate;
		gameManagar.OnPlayerCreateError += OnPlayerCreateError;
		gameManagar.OnNoSUIError += OnNoSUIError;
	}
    
	void OnDestroy(){
		gameManagar.OnPlayerReady  	-= OnPlayerReady;
		gameManagar.OnPlayerUpdated -= OnPlayerUpdate;
		gameManagar.OnPlayerCreateError -= OnPlayerCreateError;
		gameManagar.OnNoSUIError -= OnNoSUIError;
	}

	void OnPlayerReady(Player player){
		OnPlayerUpdate(player);
		manager.ShowScreen("GameScreen");
	}
	void OnPlayerUpdate(Player player){
		coins.text = gameManagar.player.coins.ToString();
		lives.text = gameManagar.player.lifeCoins.ToString();
	}
	void OnPlayerCreateError(){
		Debug.Log("ON Player Create Error");
		InfoPopupManager.instance.AddNotif(InfoPopupManager.InfoType.Error, "Error creating player");
		manager.ShowScreen("MainScreen");
	}
	void OnNoSUIError(){
		Debug.Log("ON Player Create Error");
		InfoPopupManager.instance.AddNotif(InfoPopupManager.InfoType.Error, "Not enough SUI, please top up your wallet.");
		manager.ShowScreen("MainScreen");
	}
	void OnWallet(){
        GoTo("MainScreen");
    }

    override public void ShowScreen(object data = null)
    {
        Debug.Log("Game Screen ShowScreen");
		Debug.Log(WalletComponent.Instance.currentWallet);
		base.ShowScreen(data);
		if (gameManagar.player == null || gameManagar.player.address != WalletComponent.Instance.currentWallet.publicKey)
			StartCoroutine(gameManagar.InitPlayer());
    }

    public void ShowCongratulations(ulong reward, ulong maxbonus, ulong bonus)
    {
        manager.Show();
        manager.ShowScreen("GameScreen");
		gameOverScreen.gameObject.SetActive(false);
        landingScreen.gameObject.SetActive(false);
        congratsScreen.gameObject.SetActive(true);
		congratsScreen.Render(reward, maxbonus, bonus);
    }

    public void ShowGameOver()
    {
		manager.Show();
        manager.ShowScreen("GameScreen");
		congratsScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(true);
        landingScreen.gameObject.SetActive(false);
    }

    public void ShowLevels()
    {
        manager.Show();
        manager.ShowScreen("GameScreen");
		congratsScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        landingScreen.gameObject.SetActive(true);
    }


}
