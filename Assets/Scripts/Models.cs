using AllArt.SUI.RPC.Response;

public class PlayerFields: Fields {
	public ulong score;
	public ulong version;
}

public class GameFields {
	public ObjectId id;
	public ulong grid_width;
	public ulong grid_height;
	public Content<GameLevel>[] levels;
	public ulong version;
}

public class GameLevel {
	public ObjectId id;
	public ushort level_number;
	public ulong cost;
	public ulong reward;
	public ulong bonus;
	public Content<LevelEnemy>[] enemies;
	public Content<LevelSpy>[] spies;
}

public class LevelEnemy {
	public ObjectId id;
	public int[] type;
	public int count;
	public ulong speed;
}

public class LevelSpy {
	public ObjectId id;
	public int count;
	public ulong speed;
}