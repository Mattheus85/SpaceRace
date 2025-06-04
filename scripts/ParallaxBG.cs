using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ParallaxBG : CanvasLayer
{
	[Export] private BackgroundItemConfig[] _itemConfigs = new BackgroundItemConfig[0];
	private List<ParallaxLayer> _parallaxLayerList = new List<ParallaxLayer>();

	public override void _Ready()
	{
		foreach (var config in _itemConfigs)
		{
			if (config.Prefab == null)
			{
				GD.Print($"Error: Prefab for {config.TypeKey} is null");
				continue;
			}
			GD.Print($@"---->ParallaxBG _Ready called, Configs: {_itemConfigs.Length}
---->Spawning {config.TypeKey}s, Count: {config.MaxCount}, Prefab: {config.Prefab.ResourcePath}");
			SpawnManager.Instance.SpawnObjects<ParallaxLayer>(
				config.TypeKey,
				config.MaxCount,
				config.Prefab,
				config.IsOrdered,
				this,
				(ParallaxLayer layer) =>
				{
					float sizeFactor = (float)(Math.Pow(GD.Randf(), 2) * (config.MaxSize - config.MinSize) + config.MinSize);
					var textureRect = layer.GetChild<TextureRect>(0);
					textureRect.Scale = new Vector2(sizeFactor, sizeFactor);
					Vector2 motionScaleValue = new Vector2(sizeFactor * config.ParallaxSpeed, sizeFactor * config.ParallaxSpeed);
					layer.MotionScale = motionScaleValue;
					GD.Print($"{textureRect.Name} born at {layer.GlobalPosition}. This big: {sizeFactor}, This fast: {motionScaleValue.Length()}");
				}
			);
		}
	}

	public override void _Process(double delta)
	{
		_parallaxLayerList = GetChildren().OfType<ParallaxLayer>().ToList();
	}
}
