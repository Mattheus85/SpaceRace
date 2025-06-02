using Godot;

public partial class Main : Node2D
{
	[Export] private int _objectsPerType = 50;
	[Export] private string[] _scenePaths = new[] { "res://scenes/rock_0.tscn" };
	[Export] private NodePath _boundaryPath; // Assign boundary node in editor

	public override void _Ready()
	{
		// Get playable area from boundary
		Vector2 minBound = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 maxBound = new Vector2(float.MinValue, float.MinValue);

		if (_boundaryPath != null)
		{
			StaticBody2D boundary = GetNode<StaticBody2D>(_boundaryPath);
			if (boundary.GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D") is CollisionPolygon2D collisionPolygon)
			{
				Vector2[] points = collisionPolygon.Polygon;
				foreach (Vector2 point in points)
				{
					Vector2 globalPoint = collisionPolygon.ToGlobal(point);
					minBound.X = Mathf.Min(minBound.X, globalPoint.X);
					minBound.Y = Mathf.Min(minBound.Y, globalPoint.Y);
					maxBound.X = Mathf.Max(maxBound.X, globalPoint.X);
					maxBound.Y = Mathf.Max(maxBound.Y, globalPoint.Y);
				}
			}
		}
		else
		{
			// Fallback bounds if boundary not set
			minBound = new Vector2(-2500, -2500);
			maxBound = new Vector2(2500, 2500);
		}

		// Spawn objects
		foreach (string path in _scenePaths)
		{
			PackedScene scene = GD.Load<PackedScene>(path);
			for (int i = 0; i < _objectsPerType; i++)
			{
				Node2D obj = scene.Instantiate<Node2D>();
				obj.Set("_isForeground", GD.Randf() > 0.5f);
				obj.Set("_playableAreaMin", minBound);
				obj.Set("_playableAreaMax", maxBound);
				AddChild(obj);
			}
		}
	}
}
