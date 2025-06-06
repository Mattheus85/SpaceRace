using Godot;

[GlobalClass]
public partial class BackgroundItemConfig : Resource
{
	[Export] public string SpaceJunkName { get; set; }
	[Export] public PackedScene ProvidedPackedScene { get; set; }
	[Export] public int Count { get; set; }
	[Export] public float Distance { get; set; }
	[Export] public float MinRelativeSize { get; set; } = 0.1f;
	[Export] public float MaxRelativeSize { get; set; } = 1.0f;
	[Export] public float ParallaxSpeed { get; set; } = 1.0f;
}
