using Godot;
using System;

public partial class Autoload : Node
{
	public static Autoload Instance { get; set; }
	private Control _debug;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
