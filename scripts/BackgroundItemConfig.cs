using Godot;

[GlobalClass]
public partial class BackgroundItemConfig : Resource
{
	[Export] public string TypeKey { get; set; } = "Unnamed";
	[Export] public PackedScene Prefab { get; set; }
	[Export] public int MaxCount { get; set; } = 25;
	[Export] public bool IsOrdered { get; set; } = true;
	[Export] public float MinSize { get; set; } = 0.1f;
	[Export] public float MaxSize { get; set; } = 0.9f;
	[Export] public float ParallaxSpeed { get; set; } = 0.5f;
}
