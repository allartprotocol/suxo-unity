using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using AllArt.SUI.RPC.Response;
using System.Linq;
using System;
using SimpleScreen;

[System.Serializable]
public class LevelContainer
{
    public LevelData[] levels;
}

[System.Serializable]
public class LevelData
{
    public ushort levelNumber;
    public ulong costToPlay;
    public ulong coinsToEarn;
    public ulong bonusCoins;
    public EnemyData[] enemies;
    public SpyData[] spies;
}

[System.Serializable]
public class EnemyData
{
    public string type;
    public uint count;
    public ulong speed;
}

[System.Serializable]
public class SpyData
{
    public uint count;
    public float speed;
}

public class Player 
{
    public string address;
    public string uid;
	public int currentLevel;
    public int lives;
    public ulong coins;
    public ulong lifeCoins;
    public ulong score;
    public float percent;
}

public class GameManager : MonoBehaviour
{
	public const ulong LIFE_PRICE = 10;
    
	public GridManager gridManager;
    public List<EnemyController> enemies = new List<EnemyController>();
    public List<SpyController> spies = new List<SpyController>();
    public SpyController spyPrefab;
    public EnemyController enemyPrefab;
    public PlayerController playerController;
	public GameView gameView;
	public int numberOfEnemies = 5;
    public int numberOfSpies = 1;
    public float enemySpeed = 20;
	public float winTreshold = 0.75f;
    public bool paused = false;
    public Player player;
	
    public LevelContainer levels;
	public GameFields GameData {
		get;
		private set;
	}

	public event Action<GameFields> OnGameDataLoaded;
	public event Action<Player> OnPlayerReady;
	public event Action<Player> OnPlayerUpdated;
	public event Action OnPlayerCreateError;
	public event Action OnNoSUIError;

    void Awake()
    {
        Application.targetFrameRate = 120;
    }
    void Start()
    {
		StartCoroutine(LoadGameData());
		GameStateManager.Instance.ChangeState(GameState.MainMenu);
    }

	public IEnumerator LoadGameData()
	{
		var task = SUINetworking.GetGame();
		yield return new WaitUntil(() => task.IsCompleted);
		if (task.IsCompletedSuccessfully && task.Result.error == null){
			GameData = task.Result.data.content.fields;
			levels = new LevelContainer {
				levels = new List<Content<GameLevel>>(GameData.levels).ConvertAll<LevelData>((l)=>{
					return new LevelData {
						levelNumber = l.fields.level_number,
						costToPlay = l.fields.cost,
						coinsToEarn = l.fields.reward,
						bonusCoins = l.fields.bonus,
						enemies = new List<Content<LevelEnemy>>(l.fields.enemies).ConvertAll<EnemyData>((e)=>{
							return new EnemyData {
								type = e.fields.type.ToString(),
								count = (uint)e.fields.count,
								speed = e.fields.speed
							};
						}).ToArray(),
						spies = new List<Content<LevelSpy>>(l.fields.spies).ConvertAll<SpyData>((s)=>{
							return new SpyData {
								count = (uint)s.fields.count,
								speed = s.fields.speed
							};
						}).ToArray()
					};
				}).ToArray()
			};
			OnGameDataLoaded?.Invoke(GameData);
		}
		else
		{
			if(task.Result.error != null) Debug.LogError(task.Result.error.message);
			else Debug.LogError("Failed to load game data.");
		}
	}
    
	public IEnumerator InitPlayer() 
    {
		var loadPlayerTask = LoadPlayer();
		yield return new WaitUntil(() => loadPlayerTask.IsCompleted);
		if(loadPlayerTask.IsCompletedSuccessfully){
			player = loadPlayerTask.Result;
			OnPlayerReady?.Invoke(loadPlayerTask.Result);
		}
		else
		{
			var createPlayerTask = CreatePlayer();
			yield return new WaitUntil(() => createPlayerTask.IsCompleted);
			if(createPlayerTask.IsCompletedSuccessfully){
				player = createPlayerTask.Result;
				OnPlayerReady?.Invoke(player);
			}
			else{
				Debug.Log(createPlayerTask.Exception != null ? createPlayerTask.Exception.Message : "Failed to create player.");
				OnPlayerCreateError?.Invoke();
			}
		}
    }
	private async Task<Player> CreatePlayer() {
		try{
			var coins = await SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.SUI_COIN);
			if(coins.Count == 0){
				OnNoSUIError?.Invoke();
				throw new Exception("No SUI coins found.");
			}
			await SUINetworking.CreatePlayer(WalletComponent.Instance.currentWallet);
			var player = await SUINetworking.GetPlayer(WalletComponent.Instance.currentWallet);
			if(player.error != null) throw new Exception(player.error.message);
			return new Player {
				address = WalletComponent.Instance.currentWallet.publicKey,
				uid = player.data.content.fields.id.id,
				lives = 3,
			};
		}
		catch(Exception e){
			throw e;
		}
	}
	private async Task<Player> LoadPlayer() {
		try{
			var player = await SUINetworking.GetPlayer(WalletComponent.Instance.currentWallet);
			if(player.error != null) throw new Exception(player.error.message);
			var coins = await SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.SCORE_COIN);
			var life = await SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.LIFE_COIN);
			return new Player {
				address = WalletComponent.Instance.currentWallet.publicKey,
				uid = player.data.content.fields.id.id,
				lifeCoins = life.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance)),
				coins = coins.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance))
			};
		}
		catch(Exception e){
			throw e;
		}
	}

	public void StartGame()
	{
		GameStateManager.Instance.ChangeState(GameState.Playing);
	}

	public IEnumerator PlayLevel(int level, Action callback = null){
		LevelData levelData = GetLevelByNymber(level);
		ResetGame();
		if (levelData.costToPlay > 0)
		{
			Task playLevelTask = SUINetworking.PlayLevel(WalletComponent.Instance.currentWallet, (ushort)level, player.uid);
			yield return new WaitUntil(() => playLevelTask.IsCompleted);
			if (!playLevelTask.IsCompletedSuccessfully) throw new Exception("Failed to play level.");
			
			Task<List<SUIObjectResponse<Fields>>> coinsTask = SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.SCORE_COIN);
			yield return new WaitUntil(() => coinsTask.IsCompleted);
			if (coinsTask.IsCompletedSuccessfully){
				player.coins = coinsTask.Result.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance));
			}
			else throw new Exception("Failed to get score coins.");
		}

		player.currentLevel = levelData.levelNumber;
		player.lives = 3;
		player.score = 0;
		player.percent = 0;
		RenderLevel(levelData.levelNumber);
		OnPlayerUpdated?.Invoke(player);
		GameStateManager.Instance.ChangeState(GameState.Playing);
		callback?.Invoke();
	}

    void RenderLevel(int level)
    {
        LevelData levelData = GetLevelByNymber(level);
        gridManager.CreateGrid();
        gridManager.AdjustCamera();
		playerController.ResetPosition();
        SpawnEnemies(levelData.enemies);
        SpawnSpies(levelData.spies);
    }

	LevelData GetLevelByNymber(int levelNumber){
		return levels.levels.FirstOrDefault((l) => l.levelNumber == levelNumber);
	}

    void SpawnEnemies(EnemyData[] newEnemies)
    {
        foreach (var enemyData in newEnemies)
        {
            for (int i = 0; i < enemyData.count; i++)
            {
                if (gridManager.fillableCells.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, gridManager.fillableCells.Count);
                    Cell targetCell = gridManager.fillableCells[randomIndex];
                    Vector2 spawnPosition = targetCell.gridPosition;// + new Vector2(0.5f, 0.5f);

                    EnemyController newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    newEnemy.SetSpeed(enemyData.speed);
                    enemies.Add(newEnemy);
                }
            }
        }
    }

    void SpawnSpies(SpyData[] newSpies)
    {
        foreach (var spyData in newSpies)
        {
            for (int i = 0; i < spyData.count; i++)
            {
                if (gridManager.claimedCells.Count > 0)
                {
                    Cell targetCell = null;
                    Vector2 spawnPosition = Vector2.zero;

                    int attempts = 0;
                    do
                    {
                        int randomIndex = UnityEngine.Random.Range(0, gridManager.claimedCells.Count);
                        targetCell = gridManager.claimedCells[randomIndex];
                        spawnPosition = targetCell.gridPosition;
                        attempts++;
                    } while (Vector2.Distance(spawnPosition, playerController.transform.position) < 30f && attempts < 100);

                    if (attempts < 100)
                    {
                        SpyController newSpy = Instantiate(spyPrefab, spawnPosition, Quaternion.identity);
                        newSpy.SetSpeed(spyData.speed);
                        spies.Add(newSpy);
                        Debug.Log($"Spy spawned at position: {spawnPosition}");
                    }
                    else
                    {
                        Debug.Log("Failed to find a spawn position for the spy that is at least 15f from the player after 100 attempts.");
                    }
                }
            }
        }
    }

    public void PlayerDies()
    {
        player.lives--;
        OnPlayerUpdated?.Invoke(player);

        if (player.lives <= 0)
        {
            GameStateManager.Instance.ChangeState(GameState.GameOver);
            StartCoroutine(WaitForFuneral());
        }
        else
        {
            GameStateManager.Instance.ChangeState(GameState.PlayerDied);
            StartCoroutine(WaitForPlayingAgain());
        }
    }

    private IEnumerator WaitForPlayingAgain()
    {
        yield return new WaitUntil(() => GameStateManager.Instance.CurrentState == GameState.Playing);
        gridManager.ClearPath();
        playerController.ResetPosition();
    }

    private IEnumerator WaitForFuneral()
    {
        yield return new WaitUntil(() => GameStateManager.Instance.CurrentState == GameState.Playing);
        gridManager.ClearPath();
        playerController.ResetPosition();
        EndGame();
    }

    void EndGame()
    {
		gameView.ShowGameOver();
        GameStateManager.Instance.ChangeState(GameState.MainMenu);
    }

    public void ContinueGame()
    {
		gameView.manager.Hide();
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }

	void ResetGame()
    {
        foreach (EnemyController enemy in enemies)
            Destroy(enemy.gameObject);
        enemies.Clear();
        SpyController[] spies = FindObjectsOfType<SpyController>();
        foreach (SpyController spy in spies)
            Destroy(spy.gameObject);
        gridManager.ClearGrid();
    }

	IEnumerator LoadCoins()
	{
		var lifeTask = SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.LIFE_COIN);
		var coinsTask = SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.SCORE_COIN);
		yield return new WaitUntil(() => lifeTask.IsCompleted && coinsTask.IsCompleted);
		
		if(lifeTask.IsCompletedSuccessfully){
			player.lifeCoins = lifeTask.Result.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance));
			OnPlayerUpdated?.Invoke(player);
		}
		if(coinsTask.IsCompletedSuccessfully){
			player.coins = coinsTask.Result.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance));
			OnPlayerUpdated?.Invoke(player);
		}
	}

    public IEnumerator BuyLife(Action callback = null)
    {
		var redeemLifeTask = SUINetworking.RedeemLife(WalletComponent.Instance.currentWallet);
		yield return new WaitUntil(() => redeemLifeTask.IsCompleted);
		if (!redeemLifeTask.IsCompletedSuccessfully){
			InfoPopupManager.instance.AddNotif(InfoPopupManager.InfoType.Error, "Failed to buy life");
			callback?.Invoke();
			yield break;
		}
		yield return LoadCoins();
		InfoPopupManager.instance.AddNotif(InfoPopupManager.InfoType.Info, "Life bought successfully");
		callback?.Invoke();
	}

	public IEnumerator UseLife(Action callback = null)
    {
		var useLifeTask = SUINetworking.UseLife(WalletComponent.Instance.currentWallet);
		yield return new WaitUntil(() => useLifeTask.IsCompleted);
		if (!useLifeTask.IsCompletedSuccessfully){
			InfoPopupManager.instance.AddNotif(InfoPopupManager.InfoType.Error, "Failed to use life");
			callback?.Invoke();
			yield break;
		}
		
		yield return LoadCoins();
		player.lives++;
		OnPlayerUpdated?.Invoke(player);
		callback?.Invoke();
	}

    public void IncreaseScore(int score)
    {
        player.score += (ulong)score;
		OnPlayerUpdated?.Invoke(player);
    }

    public void Percent(float percent)
    {
        player.percent = percent;
        if (percent > winTreshold){
			StartCoroutine(Win(percent));
		}
		else OnPlayerUpdated?.Invoke(player);
    }

    IEnumerator Win(float percent)
    {
        GameStateManager.Instance.ChangeState(GameState.NextLevel);
		LevelData level = GetLevelByNymber(player.currentLevel);
		ulong bonus = 0;
		if(percent > 0.95f) bonus = level.bonusCoins;
		else bonus = (ulong)(((percent - 0.75f) / 0.2f) * level.bonusCoins);
        gameView.ShowCongratulations(level.coinsToEarn, level.bonusCoins, bonus);
		
		var completeLevelTask = SUINetworking.CompleteLevel(WalletComponent.Instance.currentWallet, player.uid, (ushort)player.currentLevel, player.score, (ulong)(percent * 1000));
		yield return new WaitUntil(() => completeLevelTask.IsCompleted);
		if (!completeLevelTask.IsCompletedSuccessfully){
			Debug.LogError("Failed to play level.");
		}
		
		var coinsTask = SUINetworking.GetTokens(WalletComponent.Instance.currentWallet, SUINetworking.SCORE_COIN);
		yield return new WaitUntil(() => coinsTask.IsCompleted);
		if (!coinsTask.IsCompletedSuccessfully){
			Debug.LogError("Failed to get score coins.");
		}
		player.coins = coinsTask.Result.Aggregate<SUIObjectResponse<Fields>, ulong>(0, (acc, coin) => acc + ulong.Parse(coin.data.content.fields.balance));
		player.percent = 0;
		player.score = 0;
		OnPlayerUpdated?.Invoke(player);
    }
}