using UnityEngine;
using System.Collections.Generic;
using AllArt.SUI.RPC.Filter.Types;
using AllArt.SUI.RPC.Response;
using AllArt.SUI.Wallets;
using System.Threading.Tasks;
using Chaos.NaCl;
using AllArt.SUI.RPC;

public static class SUINetworking {
	/// <summary>
	/// Once you deploy your SUI package, you will get all object identifiers from the SUI blockchain.
	/// Replace the following values with the object identifiers you get from the SUI blockchain.
	/// All object identifiers except for game package will be under "Created Objects" section of the package publish response.
	/// Game package will be under "Published Objects" section of the package publish response.
	/// </summary>
	public const string GAME_PACKAGE = "0x33aef485dfd56d5bdae9ef79fe8ce1b24d0301b6447726a848203354a3417802";
	public const string GAME_ID = "0x6df43d60e98f17247898ace5cbbfe409b767bd58d54725d2edc065d02963f046";
	public const string ADMIN_CAP = "0x65eaf5ea9d5463859fb606a2e48e4912f058ff62d700d7f4efe3ed961f94d69f";
	public const string LEVEL_CAP = "0xa3a4c90b047e4b40a74a7a88adc4178b78b75a58964ac4112b3b68999ffa9333";
	public const string SCORE_CAP = "0xbe9c23c0c6e094ed912b7996f623c302cc30e0c6500f5c4766cfe6f2c9045d97";
	public const string LIFE_CAP = "0x7364365f468c68b077e2170d223a7f45e14c2a27b87a11c1df9a54a2eec0e49a";
	public const string SCORE_COIN = "0x2::coin::Coin<" + GAME_PACKAGE + "::score::SCORE>";
	public const string LIFE_COIN = "0x2::coin::Coin<" + GAME_PACKAGE + "::life::LIFE>";
	public const string SUI_COIN_ADDRESS = "0x2::sui::SUI";
	public const string SUI_COIN = "0x2::coin::Coin<" + SUI_COIN_ADDRESS + ">";
	public const string LEVEL_OBJECT = GAME_PACKAGE + "::level::Level";
	
	
	static readonly SUIRPCClient faucet = new("https://faucet.devnet.sui.io/gas");
	static readonly SUIRPCClient client = new(SUIConstantVars.devNetNode);
	static SUIRPCClient getClient(SUIRPCClient client = null) => client ?? SUINetworking.client;
	
	public static async Task CreatePlayer(Wallet wallet, string gameID = GAME_ID, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"create_player",
				new string[0],
				new object[]{gameID},
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}
	
	public static async Task IncreasePlayerScore(ulong amount, Wallet wallet, string playerID, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"increase_score",
				new string[0],
				new object[]{ playerID, amount.ToString(), SCORE_CAP },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task RedeemLife(Wallet wallet, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var score = await GetTokens(wallet, SCORE_COIN);
			var scoreList = score.ConvertAll(s => s.data.content.fields.id.id);
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"redeem_life",
				new string[0],
				new object[]{ scoreList, LIFE_CAP, SCORE_CAP },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}
	
	public static async Task UseLife(Wallet wallet, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var life = await GetTokens(wallet, LIFE_COIN);
			var lifeList = life.ConvertAll(s => s.data.content.fields.id.id);
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"use_life",
				new string[0],
				new object[]{ lifeList.ToArray(), LIFE_CAP },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}
	
	public static async Task CreateLevel(Wallet wallet, ushort levelNumber, ulong cost, ulong reward, ulong bonus, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"level",
				"create_level",
				new string[0],
				new object[]{ levelNumber, cost.ToString(), reward.ToString(), bonus.ToString() },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task AddEnemy(Wallet wallet, string levelID, string type, uint count, ulong speed, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"level",
				"add_enemy",
				new string[0],
				new object[]{ levelID, type, count, speed.ToString() },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task AddSpy(Wallet wallet, string levelID, uint count, ulong speed, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"level",
				"add_spy",
				new string[0],
				new object[]{ levelID, count, speed.ToString() },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task AddLevel(Wallet wallet, string adminCap, string levelID, string gameID = GAME_ID, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"add_level",
				new string[0],
				new object[]{ adminCap, gameID, levelID },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task PlayLevel(Wallet wallet, ushort levelNumber, string playerID, string gameID = GAME_ID, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var score = await GetTokens(wallet, SCORE_COIN);
			var scoreList = score.ConvertAll(s => s.data.content.fields.id.id).ToArray();
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"play_level",
				new string[0],
				new object[]{ gameID, playerID, levelNumber, scoreList, SCORE_CAP },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}
	
	public static async Task CompleteLevel(Wallet wallet, string player, ushort levelNumber, ulong playerScore, ulong percent, string gameID = GAME_ID, string package = GAME_PACKAGE, SUIRPCClient rpcClient = null) {
		try{
			var ptb = await getClient(rpcClient).MoveCall(
				wallet, 
				package,
				"game",
				"complete_level",
				new string[0],
				new object[]{ gameID, player, levelNumber, playerScore.ToString(), percent.ToString(), SCORE_CAP },
				"10000000"
			);
			ValidatePTBRequest(ptb);
			
			var signature = wallet.SignData(Wallet.GetMessageWithIntent(CryptoBytes.FromBase64String(ptb.result.txBytes)));
			var tx = await getClient(rpcClient).ExecuteTransactionBlock(
				ptb.result.txBytes,
				new string[] { signature }, 
				new TransactionBlockResponseOptions(), 
				ExecuteTransactionRequestType.WaitForLocalExecution
			);
			ValidatePTBResponse(tx);
		}
		catch(System.Exception e){
			throw e;
		}
	}

	public static async Task<List<SUIObjectResponse<GameLevel>>> GetUnboundedLevels(Wallet wallet, string type, SUIRPCClient rpcClient = null) {
		try{
			ObjectResponseQuery query = new() {
				filter = new MatchAllDataFilter{
					MatchAll = new List<ObjectDataFilter>{
						new StructTypeDataFilter() { StructType = type},
						new OwnerDataFilter() { AddressOwner = wallet.publicKey }
					}
				},
				options = new ObjectDataOptions()
			};
			List<SUIObjectResponse<GameLevel>> objects = new List<SUIObjectResponse<GameLevel>>();
			string nextCursor = null;
			while(true){
				var response = await getClient(rpcClient).GetOwnedObjects<GameLevel>(wallet.publicKey, query, nextCursor, 3);
				if(response.data != null){
					for(int i = 0; i < response.data.Count; i++){
						if(response.data[i].error == null)
							objects.Add(response.data[i]);
					}
					if(!response.hasNextPage) break;
					nextCursor = response.nextCursor;
					continue;
				}
				break;
			}
			return objects;
		}
		catch(System.Exception e){
			Debug.LogError(e);
			return null;
		}
	}

	public static async Task<SUIObjectResponse<PlayerFields>> GetPlayer(Wallet wallet, SUIRPCClient rpcClient = null) {
		try{
			ObjectResponseQuery query = new() {
				filter = new MatchAllDataFilter{
					MatchAll = new List<ObjectDataFilter>{
						new StructTypeDataFilter() { StructType =  $"{GAME_PACKAGE}::player::Player"},
						new OwnerDataFilter() { AddressOwner = wallet.publicKey }
					}
				},
				options = new ObjectDataOptions()
			};
			var playerRequest = await getClient(rpcClient).GetOwnedObjects<PlayerFields>(wallet.publicKey, query, null, 3);
			return playerRequest.data[0];
		}
		catch(System.Exception e){
			Debug.Log($"GetPlayer Error: {e.Message}");
			return null;
		}
	}

	public static async Task<SUIObjectResponse<GameFields>> GetGame(string gameID = GAME_ID, SUIRPCClient rpcClient = null) {
		try{
			return await getClient(rpcClient).GetObject<GameFields>(gameID);
		}
		catch(System.Exception e){
			Debug.LogError(e);
			return null;
		}
	}

	public static async Task<List<SUIObjectResponse<Fields>>> GetTokens(Wallet wallet, string type, SUIRPCClient rpcClient = null) {
		List<SUIObjectResponse<Fields>> objects = new();
		try{
			ObjectResponseQuery query = new() {
				filter = new MatchAllDataFilter{
					MatchAll = new List<ObjectDataFilter>{
						new StructTypeDataFilter() { StructType = type},
						new OwnerDataFilter() { AddressOwner = wallet.publicKey }
					}
				},
				options = new ObjectDataOptions()
			};
			string nextCursor = null;
			while(true){
				var response = await getClient(rpcClient).GetOwnedObjects<Fields>(wallet.publicKey, query, nextCursor, 3);
				if(response.data != null){
					for(int i = 0; i < response.data.Count; i++){
						if(response.data[i].error == null)
							objects.Add(response.data[i]);
					}
					if(!response.hasNextPage) break;
					nextCursor = response.nextCursor;
					continue;
				}
				break;
			}
		}
		catch(System.Exception e){
			Debug.LogError(e);
		}
		return objects;
	}

	private static void ValidatePTBRequest(JsonRpcResponse<TransactionBlockBytes> request){
		if(request.error != null) throw new System.Exception($"Failed to create PTB for {GAME_PACKAGE} package: {request.error.message}");
	}

	private static void ValidatePTBResponse(JsonRpcResponse<SuiTransactionBlockResponse> response){
		if(response.error != null) throw new System.Exception($"Failed to process PTB for {GAME_PACKAGE} package: {response.error.message}");
		if(response.result.error != null) throw new System.Exception($"Failed to process PTB for {GAME_PACKAGE} package: {response.result.error.message}");
		if(response.result.effects != null && response.result.effects.status.status == "failure") throw new System.Exception($"Failed to process PTB for {GAME_PACKAGE} package: {response.result.effects.status.error}");
	}
}