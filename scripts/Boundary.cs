using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Boundary : StaticBody2D
{
	private List<CollisionShape2D> boundarySegmentList;

	public override void _Ready()
	{
		boundarySegmentList = GetChildren().OfType<CollisionShape2D>().ToList();
		// Optional: Debug boundary setup
		foreach (CollisionShape2D shape in boundarySegmentList)
		{
			SegmentShape2D segment = shape.Shape as SegmentShape2D;
			//GD.Print($"Boundary Segment: {shape.Name}, Position: {segment.A}, {segment.B}");
		}
	}

	public override void _Process(double delta)
	{
		// Optional: Monitor boundary interactions if needed
	}

	public Vector2 GetBoundaryExtents()
	{
		// Return boundary dimensions for SpawnManager or other systems
		float minX = float.MaxValue, maxX = float.MinValue;
		float minY = float.MaxValue, maxY = float.MinValue;

		foreach (var shape in boundarySegmentList)
		{
			var segment = (SegmentShape2D)shape.Shape;
			var start = shape.Position + segment.A;
			var end = shape.Position + segment.B;
			minX = Mathf.Min(minX, Mathf.Min(start.X, end.X));
			maxX = Mathf.Max(maxX, Mathf.Max(start.X, end.X));
			minY = Mathf.Min(minY, Mathf.Min(start.Y, end.Y));
			maxY = Mathf.Max(maxY, Mathf.Max(start.Y, end.Y));
		}

		return new Vector2(maxX - minX, maxY - minY); // e.g., (38400, 21600)
	}

	public bool IsWithinBounds(Vector2 position)
	{
		// Check if a position is within the boundary for SpawnManager
		foreach (var shape in boundarySegmentList)
		{
			var segment = (SegmentShape2D)shape.Shape;
			var start = shape.Position + segment.A;
			if (shape.Name == "Top" && position.Y < start.Y) return false;
			if (shape.Name == "Bottom" && position.Y > start.Y) return false;
			if (shape.Name == "Left" && position.X < start.X) return false;
			if (shape.Name == "Right" && position.X > start.X) return false;
		}
		return true;
	}
}
