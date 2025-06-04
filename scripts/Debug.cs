using Godot;

public partial class Debug : CanvasLayer
{
	private Label positionLabel;
	private Label speedLabel;
	private Label rotationLabel;
	private Label directionLabel;
	private LineEdit spawnRadiusInput;
	private SpawnManager spawnManager;


	public override void _Ready()
	{
		positionLabel = GetNode<Label>("DebugPanel/VBoxContainer/PositionLabel");
		speedLabel = GetNode<Label>("DebugPanel/VBoxContainer/SpeedLabel");
		rotationLabel = GetNode<Label>("DebugPanel/VBoxContainer/RotationLabel");
		directionLabel = GetNode<Label>("DebugPanel/VBoxContainer/DirectionLabel");
		//spawnRadiusInput = GetNode<LineEdit>("DebugPanel/VBoxContainer/SpawnRadiusInput");
		//spawnManager = SpawnManager.Instance;
		//spawnRadiusInput.TextSubmitted += OnSpawnRadiusSubmitted;
	}

	public override void _Process(double delta)
	{
		CharacterBody2D player = GetNode<CharacterBody2D>("/root/Main/Player");
		int velo = (int)player.Velocity.Length();
		speedLabel.Text = $"Speed: {velo}";
		int vecx = (int)player.Position.X;
		int vecy = (int)player.Position.Y;
		positionLabel.Text = $"Position: {vecx}, {vecy}";
		int rotation = (int)player.RotationDegrees;
		if (rotation < 0)
		{
			rotation = 360 + rotation;
		}
		rotationLabel.Text = $"Rotation: {rotation}";
		string direction = GetCardinalDirection(rotation);
		directionLabel.Text = $"Direction: {direction}";
	}

	private string GetCardinalDirection(float angleDegrees)
	{
		int segment = (int)((angleDegrees % 360 + 11.25f) / 22.5f) % 16;
		string[] directions = { "↑ N", "↗ NNE", "↗ NE", "↗ ENE", "→ E", "↘ ESE", "↘ SE", "↘ SSE", "↓ S", "↙ SSW", "↙ SW", "↙ WSW", "← W", "↖ WNW", "↖ NW", "↖ NNW" };
		return directions[segment];
	}

	private void OnSpawnRadiusSubmitted(string text)
	{
		if (float.TryParse(text, out float value))
			spawnManager.Set("spawnRadius", value);
	}
}
