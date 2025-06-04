using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ParallaxBG : ParallaxBackground
{
	[Export] private BackgroundItemConfig[] _itemConfigs = new BackgroundItemConfig[0];
	private List<ParallaxLayer> _parallaxLayerList = new List<ParallaxLayer>();

	public override void _Ready()
	{
		CharacterBody2D player = GetNode<CharacterBody2D>("/root/Main/Player");

		foreach (var config in _itemConfigs)
		{
			if (config.Prefab == null)
			{
				GD.Print($"Error: Prefab for {config.TypeKey} is null");
				continue;
			}
			GD.Print($@"---->ParallaxBG _Ready called, Configs: {_itemConfigs.Length}
---->Spawning {config.TypeKey}s, Count: {config.MaxCount}, Prefab: {config.Prefab.ResourcePath}");

			for (int i = 0; i < config.MaxCount; i++)
			{
				var layer = config.Prefab.Instantiate<ParallaxLayer>();

				// Simplest positioning: Random in a large fixed area around origin
				float randomX = (float)GD.RandRange(0, 39000);
				float randomY = (float)GD.RandRange(0, 22000);
				layer.GlobalPosition = new Vector2(randomX, randomY);

				// Existing configuration for size and motion scale
				float sizeFactor = (float)(Math.Pow(GD.Randf(), 2) * (config.MaxSize - config.MinSize) + config.MinSize);
				var textureRect = layer.GetChild<TextureRect>(0);
				textureRect.Scale = new Vector2(sizeFactor, sizeFactor);
				Vector2 motionScaleValue = new Vector2(sizeFactor * config.ParallaxSpeed, sizeFactor * config.ParallaxSpeed);
				layer.MotionScale = motionScaleValue.X < 0.6f ? motionScaleValue * new Vector2(.025f,.025f) : motionScaleValue;

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

	public override void _Process(double delta)
	{
		_parallaxLayerList = GetChildren().OfType<ParallaxLayer>().ToList();
	}
}
