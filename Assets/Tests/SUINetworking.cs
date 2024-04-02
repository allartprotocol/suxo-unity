using System.Collections;
using NUnit.Framework;
using UnityEngine;
using AllArt.SUI.Wallets;
using UnityEngine.TestTools;
using AllArt.SUI.RPC;

public class SUINetworkingTest
{
	readonly Wallet wallet = new (Mnemonics.GenerateNewMnemonic());
	readonly SUIRPCClient faucet = new("https://faucet.devnet.sui.io/gas");
	bool ready = false;
	
	IEnumerator Setup()
	{
		if(ready) yield break;
		GameObject go = new();
		WebsocketController wc = go.AddComponent<WebsocketController>();
		wc.SetupConnection(SUIConstantVars.devNetNode);
		var response = faucet.SendRequestAsync<object>(new {
			FixedAmountRequest = new {
				recipient = wallet.publicKey,
			},
		});
		yield return new WaitUntil(() => response.IsCompleted);
		yield return new WaitForSeconds(10);
		ready = true;
	}
	
	[UnityTest]
	public IEnumerator Test_1_GetGame()
	{
		yield return Setup();
		var response = SUINetworking.GetGame();
		while (!response.IsCompleted) yield return null;
		Assert.IsNotNull(response);
		Assert.IsNotNull(response.Result.data);
		Assert.IsNotNull(response.Result.data.content);
		Assert.IsNotNull(response.Result.data.content.fields);
		Assert.IsNotNull(response.Result.data.content.fields.levels);
		Assert.IsTrue(response.Result.data.content.fields.levels.Length > 0);
	}

	[UnityTest]
	public IEnumerator Test_2_CreatePlayer()
	{
		yield return Setup();

		var createPlayerResponse = SUINetworking.CreatePlayer(wallet);
		while (!createPlayerResponse.IsCompleted) yield return null;
		
		yield return new WaitForSeconds(1);
		if(createPlayerResponse.IsFaulted) {
			var getPlayerResponse = SUINetworking.GetPlayer(wallet);
			while (!getPlayerResponse.IsCompleted) yield return null;
			Assert.IsNotNull(getPlayerResponse);
			Assert.IsNotNull(getPlayerResponse.Result.data);
			Assert.IsNotNull(getPlayerResponse.Result.data.content);
			Assert.IsNotNull(getPlayerResponse.Result.data.content.fields);
		}
		else Assert.IsTrue(createPlayerResponse.IsCompletedSuccessfully);
	}

	[UnityTest]
	public IEnumerator Test_3_GetPlayer()
	{
		yield return Setup();
		var response = SUINetworking.GetPlayer(wallet);
		while (!response.IsCompleted) yield return null;
		Assert.IsNotNull(response);
		Assert.IsNotNull(response.Result.data);
		Assert.IsNotNull(response.Result.data.content);
		Assert.IsNotNull(response.Result.data.content.fields);
	}

	[UnityTest]
	public IEnumerator Test_4_IncreaseScore()
	{
		yield return Setup();
		
		var playerResponse = SUINetworking.GetPlayer(wallet);
		while (!playerResponse.IsCompleted) yield return null;
		Debug.Log($"Player Received: {playerResponse.Result.data.content.fields.id.id}");

		var scoreBeforeResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreBeforeResponse.IsCompleted) yield return null;
		Debug.Log($"Player Total Score Before: {scoreBeforeResponse.Result.Count}");
		
		var increaseScore = SUINetworking.IncreasePlayerScore(10, wallet, playerResponse.Result.data.content.fields.id.id);
		while (!increaseScore.IsCompleted) yield return null;
		
		var scoreAfterResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreAfterResponse.IsCompleted) yield return null;
		Debug.Log($"Player Total Score After: {scoreAfterResponse.Result.Count}");

		Assert.IsTrue(scoreAfterResponse.Result.Count > scoreBeforeResponse.Result.Count);
	}
	
	[UnityTest]
	public IEnumerator Test_5_PlayLevel()
	{
		yield return Setup();
		
		var gameResponse = SUINetworking.GetGame();
		while (!gameResponse.IsCompleted) yield return null;
		Assert.IsTrue(gameResponse.Result.data.content.fields.levels.Length > 0);
		
		var levelNumber = gameResponse.Result.data.content.fields.levels[0].fields.level_number;
		var cost = gameResponse.Result.data.content.fields.levels[0].fields.cost;

		var playerResponse = SUINetworking.GetPlayer(wallet);
		while (!playerResponse.IsCompleted) yield return null;
		Debug.Log($"Player Received: {playerResponse.Result.data.content.fields.id.id}");

		var scoreBeforeResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreBeforeResponse.IsCompleted) yield return null;

		ulong balanceBeforeResponse = 0;
		ulong balanceAfterResponse = 0;
		
		scoreBeforeResponse.Result.ForEach((coin) => {
			balanceBeforeResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		Debug.Log($"Player Total Score Before: {scoreBeforeResponse.Result.Count}");
		
		Debug.Log("PLAYING LEVEL !!!!!!");
		var playLevelResponse = SUINetworking.PlayLevel(wallet, levelNumber, playerResponse.Result.data.content.fields.id.id);
		while (!playLevelResponse.IsCompleted) yield return null;
		if(playLevelResponse.IsFaulted) {
			Debug.LogError($"Error: {playLevelResponse.Exception.Message}");
		}
		Assert.IsTrue(playLevelResponse.IsCompletedSuccessfully);
		
		var scoreAfterResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreAfterResponse.IsCompleted) yield return null;
		scoreAfterResponse.Result.ForEach((coin) => {
			balanceAfterResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		Debug.Log($"Player Total Score After: {scoreAfterResponse.Result.Count}");

		if(cost > 0)
			Assert.IsTrue(balanceAfterResponse < balanceBeforeResponse);
		else
			Assert.IsTrue(balanceAfterResponse == balanceBeforeResponse);
	}

	[UnityTest]
	public IEnumerator Test_6_CompleteLevel()
	{
		yield return Setup();
		
		var gameResponse = SUINetworking.GetGame();
		while (!gameResponse.IsCompleted) yield return null;
		Assert.IsTrue(gameResponse.Result.data.content.fields.levels.Length > 0);
		var levelNumber = gameResponse.Result.data.content.fields.levels[1].fields.level_number;
		var cost = gameResponse.Result.data.content.fields.levels[1].fields.cost;

		var playerResponse = SUINetworking.GetPlayer(wallet);
		while (!playerResponse.IsCompleted) yield return null;
		Debug.Log($"Player Received: {playerResponse.Result.data.content.fields.id.id}");

		ulong balanceBeforeResponse = 0;
		ulong balanceAfterResponse = 0;

		var scoreBeforeResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreBeforeResponse.IsCompleted) yield return null;
		scoreBeforeResponse.Result.ForEach((coin) => {
			balanceBeforeResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		Debug.Log($"Player Total Score Before: {scoreBeforeResponse.Result.Count}");
		
		var completeLevelResponse = SUINetworking.CompleteLevel(
			wallet, 
			playerResponse.Result.data.content.fields.id.id, 
			levelNumber,
			100,
			1
		);
		while (!completeLevelResponse.IsCompleted) yield return null;
		
		var scoreAfterResponse = SUINetworking.GetTokens(wallet, SUINetworking.SCORE_COIN);
		while (!scoreAfterResponse.IsCompleted) yield return null;
		scoreAfterResponse.Result.ForEach((coin) => {
			balanceAfterResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		Debug.Log($"Player Total Score After: {scoreAfterResponse.Result.Count}");

		Assert.IsTrue(balanceAfterResponse > balanceBeforeResponse);
	}

	[UnityTest]
	public IEnumerator Test_7_RedeemLife()
	{
		yield return Setup();
		ulong balanceBeforeResponse = 0;
		ulong balanceAfterResponse = 0;

		var lifeBeforeResponse = SUINetworking.GetTokens(wallet, SUINetworking.LIFE_COIN);
		while (!lifeBeforeResponse.IsCompleted) yield return null;
		lifeBeforeResponse.Result.ForEach((coin) => {
			balanceBeforeResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		
		var redeemLife = SUINetworking.RedeemLife(wallet);
		while (!redeemLife.IsCompleted) yield return null;
		
		var lifeAfterResponse = SUINetworking.GetTokens(wallet, SUINetworking.LIFE_COIN);
		while (!lifeAfterResponse.IsCompleted) yield return null;
		lifeAfterResponse.Result.ForEach((coin) => {
			balanceAfterResponse += ulong.Parse(coin.data.content.fields.balance);
		});

		Assert.IsTrue(balanceAfterResponse > balanceBeforeResponse);
	}

	[UnityTest]
	public IEnumerator Test_8_UseLife()
	{
		yield return Setup();
		ulong balanceBeforeResponse = 0;
		ulong balanceAfterResponse = 0;

		var lifeBeforeResponse = SUINetworking.GetTokens(wallet, SUINetworking.LIFE_COIN);
		while (!lifeBeforeResponse.IsCompleted) yield return null;
		lifeBeforeResponse.Result.ForEach((coin) => {
			balanceBeforeResponse += ulong.Parse(coin.data.content.fields.balance);
		});
		
		var useLife = SUINetworking.UseLife(wallet);
		while (!useLife.IsCompleted) yield return null;
		
		var lifeAfterResponse = SUINetworking.GetTokens(wallet, SUINetworking.LIFE_COIN);
		while (!lifeAfterResponse.IsCompleted) yield return null;
		lifeAfterResponse.Result.ForEach((coin) => {
			balanceAfterResponse += ulong.Parse(coin.data.content.fields.balance);
		});

		Assert.IsTrue(balanceAfterResponse < balanceBeforeResponse);
	}

}
