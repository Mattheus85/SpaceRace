using Godot;

[GlobalClass]
public partial class BackgroundItemConfig : Resource
{
	[Export] public string TypeKey { get; set; } = "Unnamed";
	[Export] public PackedScene ProvidedPackedScene { get; set; }
	[Export] public int MaxCount { get; set; } = 10;
	[Export] public bool IsOrdered { get; set; } = true;
	[Export] public float MinSize { get; set; } = 0.1f;
	[Export] public float MaxSize { get; set; } = 1.0f;
	[Export] public float ParallaxSpeed { get; set; } = 1.0f;
}
