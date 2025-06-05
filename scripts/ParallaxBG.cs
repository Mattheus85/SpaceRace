using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ParallaxBG : ParallaxBackground
{
	[Export] private BackgroundItemConfig[] _itemConfigs = new BackgroundItemConfig[0];

	public override void _Ready()
	{
		foreach (var config in _itemConfigs)
		{
			if (config.ProvidedPackedScene == null)
			{
				GD.Print($"Error: ProvidedPackedScene for {config.TypeKey} is null");
				continue;
			}
			GD.Print($@"
					---->ParallaxBG.cs entered
					---->Spawning {config.MaxCount} {config.TypeKey}s
					");
			if (config.TypeKey == "PxStation_1")
			{

				var layer = config.ProvidedPackedScene.Instantiate<ParallaxLayer>();
				var textureRect = layer.GetChild<TextureRect>(0);
				Vector2 motionScaleValue = new Vector2
					(
						.00001f,
						.00001f
					);
				layer.GlobalPosition = new Vector2(300, 200);
				layer.MotionScale = motionScaleValue;
				this.AddChild(layer);
			}
			else
			{

				for (int i = 0; i < config.MaxCount; i++)
				{
					var layer = config.ProvidedPackedScene.Instantiate<ParallaxLayer>();

					// Existing configuration for size and motion scale
					float sizeFactor = (float)(Math.Pow(GD.Randf(), 2) * (config.MaxSize - config.MinSize) + config.MinSize);
					var textureRect = layer.GetChild<TextureRect>(0);
					textureRect.Scale = new Vector2(sizeFactor, sizeFactor);
					Vector2 motionScaleValue = new Vector2
						(
							sizeFactor * config.ParallaxSpeed,
							sizeFactor * config.ParallaxSpeed
						);

					// Simplest positioning: Random in a large fixed area around origin
					float randomX = (float)GD.RandRange(0, 3900);
					float randomY = (float)GD.RandRange(0, 2200);
					layer.GlobalPosition = new Vector2(randomX * (0 + sizeFactor), randomY * (10 + sizeFactor));

					layer.MotionScale = motionScaleValue.X < 0.6f ? motionScaleValue * new Vector2(.025f, .025f) : motionScaleValue;

					// Add to this ParallaxBackground node
					this.AddChild(layer);

					// Simple approach for 'IsOrdered': set ZIndex based on size
					// Larger (closer) objects will have a higher ZIndex.
					if (config.IsOrdered)
					{
						layer.ZIndex = (int)(sizeFactor * 100); // Scale ZIndex to avoid too many overlaps for small size differences
					}
					// 		GD.Print($@"        Position: {layer.GlobalPosition}
					// Size: {sizeFactor}
					// MotionScale: {layer.MotionScale}
					// ZIndex: {layer.ZIndex}
					// ");
				}
			}
		}
	}

	public override void _Process(double delta) { }
}
