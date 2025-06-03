using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ParallaxBG : ParallaxBackground
{
	[Export]
	private int _rockCount = 5000;
	private List<ParallaxLayer> _parallaxLayerList = new List<ParallaxLayer>();
	public override void _Ready()
	{
		var rockTexture = ResourceLoader.Load<Texture2D>("res://assets/background/rock.png");
		Random random = new Random();

		for (int i = 0; i < _rockCount; i++)
		{
			float sizeFactor = (float)(Math.Pow(random.NextDouble(), 2) * (0.05 - 0.001) + 0.001);
			var layer = new ParallaxLayer();
			var textureRect = new TextureRect();
			textureRect.Texture = rockTexture;
			textureRect.Scale = new Vector2(sizeFactor, sizeFactor);
			textureRect.Position = new Vector2((float)random.NextDouble() * 12000, (float)random.NextDouble() * 8000);
			layer.MotionScale = new Vector2(0.5f, 0.5f); // Fixed scale for testing
			layer.MotionScale = new Vector2(sizeFactor * 45.0f, sizeFactor * 45.0f);
			layer.AddChild(textureRect);
			_parallaxLayerList.Add(layer);
		}
		_parallaxLayerList = _parallaxLayerList
			.OrderBy(layer => layer.GetChild<TextureRect>(0).Scale.X)
			.ToList();
		foreach (var layer in _parallaxLayerList)
		{
			AddChild(layer);
		}

		//_parallaxLayerList.ForEach(get the texture, get it's scale, and sort based on that)
		// GD.Print("Camera Position: ", GetViewport().GetCamera2D().Position);
	}
}
