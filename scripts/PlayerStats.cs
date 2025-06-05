using Godot;

public partial class PlayerStats : CanvasLayer
{
	private Label positionLabel;
	private Label speedLabel;
	private Label rotationLabel;
	private Label directionFacingLabel;
	private Label directionTravelingLabel;
	private LineEdit spawnRadiusInput;
	private SpawnManager spawnManager;

	// private float debugCooldown = 0f;
	// private const float DEBUG_INTERVAL = 1.3f;

	public override void _Ready()
	{
		positionLabel = GetNode<Label>("DebugPanel/VBoxContainer/PositionLabel");
		speedLabel = GetNode<Label>("DebugPanel/VBoxContainer/SpeedLabel");
		rotationLabel = GetNode<Label>("DebugPanel/VBoxContainer/RotationLabel");
		directionFacingLabel = GetNode<Label>("DebugPanel/VBoxContainer/DirectionFacingLabel");
		directionTravelingLabel = GetNode<Label>("DebugPanel/VBoxContainer/DirectionTravelingLabel");
		//spawnRadiusInput = GetNode<LineEdit>("DebugPanel/VBoxContainer/SpawnRadiusInput");
		//spawnManager = SpawnManager.Instance;
		//spawnRadiusInput.TextSubmitted += OnSpawnRadiusSubmitted;
	}

	public override void _Process(double delta)
	{
		// debugCooldown -= (float)delta;
		CharacterBody2D player = GetNode<CharacterBody2D>("/root/Main/Player");
		int velo = (int)player.Velocity.Length();
		speedLabel.Text = $"Speed: {velo}";
		int vecx = (int)player.Position.X;
		int vecy = (int)player.Position.Y;
		positionLabel.Text = $"Position: {vecx}, {vecy}";
		int rot = (int)player.RotationDegrees;
		int rotation = rot < 0 ? rot + 360 : rot;
		rotationLabel.Text = $"Rotation: {rotation}";
		string directionFacing = GetCardinalDirectionFacing(rotation);
		string directionTraveling = GetCardinalDirectionTraveling(rotation, Mathf.RadToDeg(player.Velocity.Angle()));
		directionFacingLabel.Text = $"Direction Facing: {directionFacing}";
		directionTravelingLabel.Text = $"Direction Traveling: {directionTraveling}";
	}

	private string GetCardinalDirectionFacing(float angleDegrees)
	{
		int segment = (int)((angleDegrees % 360 + 11.25f) / 22.5f) % 16;
		string[] directions = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
		return directions[segment];
	}

	private string GetCardinalDirectionTraveling(float rotation, float angleDegrees)
	{
		if (angleDegrees < -91)
		{
			angleDegrees += 450;
			// if (debugCooldown <= 0)
			// {
			// 	GD.Print($"angleDegrees after conversion : {angleDegrees}");
			// 	debugCooldown = DEBUG_INTERVAL;
			// }
		}
		else
		{
			angleDegrees += 90;
		}
		int segment = (int)((angleDegrees % 360 + 11.25f) / 22.5f) % 16;
		string[] directions = { "↑", "↗", "↗", "↗", "→", "↘", "↘", "↘", "↓", "↙", "↙", "↙", "←", "↖", "↖", "↖" };
		// Safety bounds check
		if (segment < 0 || segment >= 16)
		{
			GD.PrintErr($"Invalid segment: {segment} from converted angle: {angleDegrees}");
			segment = 0; // Default to up arrow
		}
		return directions[segment];
	}

	private void OnSpawnRadiusSubmitted(string text)
	{
		if (float.TryParse(text, out float value))
			spawnManager.Set("spawnRadius", value);
	}
}
