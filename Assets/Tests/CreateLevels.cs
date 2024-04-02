using System.Collections;
using AllArt.SUI.RPC;
using AllArt.SUI.Wallets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// This test is used to deploy all levels to the SUI blockchain. It is ment to run only once by the developer who has Admin capabilities, 
/// in our case it would be the developer who deployed the SUI package. Levels are declared in a JSON file in the Resources folder. 
/// The JSON file is then loaded and the levels are deployed to the blockchain. The test is successful if all levels are deployed and the number 
/// of levels in the blockchain match the number of levels in the JSON file.
/// </summary>
public class CreateLevels
{
    readonly Wallet wallet = new ("<Mnemonic string for wallet used to deploy SUI Package>");
	readonly SUIRPCClient faucet = new("https://faucet.devnet.sui.io/gas");
	bool ready = false;

	IEnumerator Setup()
	{
		if(ready) yield break;
		Debug.Log($"Wallet: {wallet.publicKey}");
		GameObject go = new();
		WebsocketController wc = go.AddComponent<WebsocketController>();
		wc.SetupConnection(SUIConstantVars.devNetNode);
		var response = faucet.SendRequestAsync<object>(new {
			FixedAmountRequest = new {
				recipient = wallet.publicKey,
			},
		});
		ready = true;
		yield return new WaitUntil(() => response.IsCompleted);
	}
	
	[UnityTest]
	public IEnumerator CreateLevelsTest()
	{
		yield return Setup();
		var gameResponse = SUINetworking.GetGame();
		yield return new WaitUntil(() => gameResponse.IsCompleted);
		Assert.IsTrue(gameResponse.IsCompletedSuccessfully);

		TextAsset jsonFile = Resources.Load<TextAsset>("levels");
		LevelContainer levelData = JsonUtility.FromJson<LevelContainer>(jsonFile.text);
		Assert.IsNotNull(levelData);
		for(int i = 0; i < levelData.levels.Length; i++)
		{
			var level = levelData.levels[i];
			var createLevel = SUINetworking.CreateLevel(wallet, level.levelNumber, level.costToPlay, level.coinsToEarn, level.bonusCoins);
			yield return new WaitUntil(() => createLevel.IsCompleted);
			var levelsResponse = SUINetworking.GetUnboundedLevels(wallet, SUINetworking.LEVEL_OBJECT);
			while (!levelsResponse.IsCompleted) yield return null;
			Assert.IsNotNull(levelsResponse);
			Assert.IsTrue(levelsResponse.Result.Count == i + 1);
		}

		var allLevelsResponse = SUINetworking.GetUnboundedLevels(wallet, SUINetworking.LEVEL_OBJECT);
		while (!allLevelsResponse.IsCompleted) yield return null;
		Assert.IsTrue(allLevelsResponse.Result.Count == levelData.levels.Length);

		for(int i = 0; i < levelData.levels.Length; i++)
		{
			var localLevel = levelData.levels[i];
			var chainLevel = allLevelsResponse.Result.Find(l => l.data.content.fields.level_number == localLevel.levelNumber);
			Assert.IsNotNull(chainLevel);
			for(int e = 0; e < localLevel.enemies.Length; e++)
			{
				var localEnemy = localLevel.enemies[e];
				var addEnemy = SUINetworking.AddEnemy(wallet, chainLevel.data.content.fields.id.id, localEnemy.type, localEnemy.count, localEnemy.speed);
				while (!addEnemy.IsCompleted) yield return null;
			}
			for(int e = 0; e < localLevel.spies.Length; e++)
			{
				var localSpy = localLevel.enemies[e];
				var addSpy = SUINetworking.AddSpy(wallet, chainLevel.data.content.fields.id.id, localSpy.count, localSpy.speed);
				while (!addSpy.IsCompleted) yield return null;
			}
			var addLevel = SUINetworking.AddLevel(
				wallet, 
				SUINetworking.ADMIN_CAP,
				chainLevel.data.content.fields.id.id
			);
			while (!addLevel.IsCompleted) yield return null;
		}
	}
}
