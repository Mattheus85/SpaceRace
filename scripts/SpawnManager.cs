using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnManager : Node
{
	[Export] private float spawnRadius = 1.5f; // Spawn within 1.5x viewport
	[Export] private float cullRadius = 1.25f; // Cull beyond 2x viewport
	private float initialSpawnTimer = 2f;
	private int firstFewObjects = 25;

	public static SpawnManager Instance { get; private set; }

	private Camera2D camera;
	private Vector2 viewportSize;
	private Dictionary<string,
		(
			List<Node> objects,
			int maxCount,
			PackedScene prefab,
			Node parent,
			Action<Node2D> configure
		)
			> spawnedObjects = new();

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			viewportSize = GetViewport().GetVisibleRect().Size;
			camera = GetViewport().GetCamera2D();
		}
		else
		{
			QueueFree();
		}
	}

	public override void _Process(double delta)
	{
		if (camera == null) return;
		var player = GetNode<CharacterBody2D>("/root/Main/Player");
		initialSpawnTimer -= (float)delta;
		if (player.Velocity != Vector2.Zero && initialSpawnTimer <= 0)
		{
			foreach (var kvp in spawnedObjects)
			{
				var (objects, maxCount, prefab, parent, configure) = kvp.Value;
				InternalSpawnObjects(player, objects, maxCount, prefab, parent, configure);
				RemoveOutOfSightObjects(objects);
			}
		}
	}

	private void RemoveOutOfSightObjects(List<Node> objects)
	{
		objects.RemoveAll(node =>
		{
			if (node is Node2D node2D && node2D.GlobalPosition.DistanceTo(camera.GlobalPosition) > viewportSize.Length() * cullRadius)
			{
				GD.Print($"DEAD {node2D.GetChild(0).Name} at {node2D.GlobalPosition}");
				node2D.QueueFree();
				return true;
			}
			// if (node is Control control && control.GlobalPosition.DistanceTo(camera.GlobalPosition) > viewportSize.Length())
			// {
			//     GD.Print($"Culled Control at {control.GlobalPosition}");
			//     control.QueueFree();
			//     return true;
			// }
			return false;
		});
	}

	private void InternalSpawnObjects(CharacterBody2D player, List<Node> objects, int maxCount, PackedScene prefab, Node parent, Action<Node2D> configure)
	{
		while (firstFewObjects > 0 || objects.Count < maxCount)
		{
			var obj = prefab.Instantiate<Node2D>();
			Vector2 velocity = player.Velocity.Normalized();
			float angle = velocity.Angle() + Mathf.DegToRad(GD.Randf() * 90f - 45f);
			float distance = Mathf.Lerp(1200f, 2000f, Mathf.Abs(Mathf.Cos(angle - velocity.Angle())));
			Vector2 spawnPos = camera.GlobalPosition + Vector2.FromAngle(angle) * distance;
			obj.GlobalPosition = spawnPos;
			configure(obj);
			objects.Add(obj);
			parent.AddChild(obj);
			firstFewObjects--;
		}
	}

	public void SpawnObjects<T>(string typeKey, int maxCount, PackedScene prefab, bool isOrdered, Node parent, Action<T> configure) where T : Node2D
	{
		if (!spawnedObjects.ContainsKey(typeKey))
		{
			spawnedObjects[typeKey] = (new List<Node>(), maxCount, prefab, parent, x => configure((T)x));
		}
		var (objects, _, _, _, _) = spawnedObjects[typeKey];
		while (objects.Count < maxCount)
		{
			var obj = prefab.Instantiate<T>();
			Vector2 spawnOffset = new Vector2(GD.Randf() * viewportSize.X * spawnRadius, (GD.Randf() - 0.5f) * viewportSize.Y * spawnRadius);
			obj.GlobalPosition = camera.GlobalPosition + spawnOffset;
			configure(obj);
			objects.Add(obj);
			if (isOrdered)
				objects.Sort((a, b) => ((Node2D)a).Scale.Length().CompareTo(((Node2D)b).Scale.Length()));
			parent.AddChild(obj);
		}
	}

	// public void SpawnUIObjects<T>( string typeKey, PackedScene prefab, bool isOrdered, int maxCount, Node parent, Action<T> configure) where T : Control
	// {
	//     if (!spawnedObjects.ContainsKey(typeKey))
	//     {
	//         spawnedObjects[typeKey] = (new List<Node>(), maxCount, prefab, parent, x => configure((T)x));
	//     }
	//
	// var (objects, _, _, _, _) = spawnedObjects[typeKey];
	//
	//     while (objects.Count < maxCount)
	//     {
	//         var obj = prefab.Instantiate<T>();
	//         Vector2 spawnOffset = new Vector2(
	//             GD.Randf() * viewportSize.X * spawnRadius,
	//             GD.Randf() * viewportSize.Y * spawnRadius
	//         );
	//         obj.Position = spawnOffset;
	//         configure(obj);
	//         spawnedObjects[typeKey].Add(obj);
	//         parent.AddChild(obj);
	//     }
	// }
}
