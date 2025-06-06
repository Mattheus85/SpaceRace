using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ParallaxBG : ParallaxBackground
{
    [Export] private BackgroundItemConfig[] _itemConfigs = new BackgroundItemConfig[0];

    public override void _Ready()
    {
        PackedScene px_asteroid_layer_1 = ResourceLoader.Load<PackedScene>("res://scenes/background_scenes/px_asteroid_layer_1.tscn");
        BackgroundItemConfig pxAsteroidLayer1BIC = CreateSpaceJunk("example junk", px_asteroid_layer_1, 1500, 1, 5, 0.1f, 1.0f, 20);
        _itemConfigs[0] = pxAsteroidLayer1BIC;

        foreach (var config in _itemConfigs)
        {
            if (config.ProvidedPackedScene == null)
            {
                GD.Print($"Error: ProvidedPackedScene for {config.SpaceJunkName} is null");
                continue;
            }
            GD.Print($@"
					---->ParallaxBG.cs entered
					---->Spawning {config.Count} {config.SpaceJunkName}s
					");

            for (int i = 0; i < config.Count; i++)
            {
                var layer = config.ProvidedPackedScene.Instantiate<ParallaxLayer>();



                float randomScale = (float)GD.RandRange(config.MinRelativeSize, config.MaxRelativeSize);
                float distance = (float)GD.RandRange(.5f, 1.0f);

                float sizeFactor = 1f / distance * randomScale;
                var textureRect = layer.GetChild<TextureRect>(0);
                textureRect.Scale = new Vector2(sizeFactor, sizeFactor);

                // Parallax factor = 1 / D where D is distance to object. Here we have a variable parallax speed in order to fine tune how fast everything across the background. 
                float parallax_factor = config.ParallaxSpeed / distance;
                Vector2 motionScaleValue = new Vector2(parallax_factor, parallax_factor);

                // Simplest positioning: Random in a large fixed area around origin
                float randomX = (float)GD.RandRange(0, 3900);
                float randomY = (float)GD.RandRange(0, 2200);
                layer.GlobalPosition = new Vector2(randomX * (0 + sizeFactor), randomY * (10 + sizeFactor));

                layer.MotionScale = motionScaleValue;
                // Add to this ParallaxBackground node
                this.AddChild(layer);

                layer.ZIndex = (int)(distance * -99); // Scale ZIndex to avoid too many overlaps for small size differences
            }
        }
    }
    private BackgroundItemConfig CreateSpaceJunk(String spaceJunkName, PackedScene providedPackedScene, int count, float distance, float size, float minRelativeSize, float maxRelativeSize, float parallaxSpeed)
    {
        return new BackgroundItemConfig
        {
            SpaceJunkName = spaceJunkName,
            ProvidedPackedScene = providedPackedScene,
            Count = count,
            Distance = distance,
            MinRelativeSize = minRelativeSize,
            MaxRelativeSize = maxRelativeSize,
            ParallaxSpeed = parallaxSpeed,
        };
    }

    public override void _Process(double delta)
    {
    //     foreach (var config in _itemConfigs)
    //     {
    //         if (config.ProvidedPackedScene == null)
    //         {
    //             GD.Print($"Error: ProvidedPackedScene for {config.SpaceJunkName} is null");
    //             continue;
    //         }
    //         for (int i = 0; i < config.Count; i++)
    //         {
    //             var layer = config.ProvidedPackedScene.Instantiate<ParallaxLayer>();
    //             var textureRect = layer.GetChild<TextureRect>(0);
    //             float scale = textureRect.Scale.X;
    //             textureRect.Rotation += scale * (float)delta * GD.Randf();
    //         }
    //     }
    }
}
