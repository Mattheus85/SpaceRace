using Godot;
using System;

public partial class PxAsteroidTexture1 : TextureRect
{
	private bool ShouldRotate { get; set; }
	private bool ShouldRotateClockwise { get; set; }
	[Export] public float BaseRotationSpeed { get; set; } = 0.5f;
	[Export] public float SpeedExaggeration { get; set; } = 2.0f;
	[Export] public float MinRand { get; set; } = 0.3f;
	[Export] public float MaxRand { get; set; } = 0.7f;

	public override void _Ready()
	{
		Rotation = (float)GD.RandRange(0, 360);
		ShouldRotate = (float)GD.Randf() > 0.2f ? true : false;
		ShouldRotateClockwise = (float)GD.Randf() > 0.5f ? true : false;
	}

	public override void _Process(double delta)
	{
		if (ShouldRotate)
		{
			if (ShouldRotateClockwise)
			{
				Rotation += CalculateRotationSpeed(Scale.X) * (float)delta;
			}
			else
			{
				Rotation -= CalculateRotationSpeed(Scale.X) * (float)delta;
			}
		}
	}

	public float CalculateRotationSpeed(float scale)
	{
		float inverse = 1.0f / Mathf.Pow(scale, SpeedExaggeration);
		float randomFactor = (float)GD.RandRange(MinRand, MaxRand);
		return BaseRotationSpeed * inverse * randomFactor;
	}
}
