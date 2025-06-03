using Godot;
using System.Linq;
using System.Collections.Generic;


public partial class Boundary : StaticBody2D
{
	private CanvasLayer _cl;

	public override void _Ready()
	{
		List<CollisionShape2D> boundarySegmentList = GetChildren().OfType<CollisionShape2D>().ToList();

		_cl = GetNode<CanvasLayer>("CL");
		TextureRect rect = _cl.GetChildren().First() as TextureRect;

		// SetRectPointsToWorldBoundaries(rect, boundarySegmentList);
	}

	public override void _Process(double delta) { }

	private void SetRectPointsToWorldBoundaries // This is for now done in the editor
		(TextureRect rect, List<CollisionShape2D> boundarySegmentList)
	{
		// GD.Print($"First List: {boundarySegmentList.ElementAt(0).Name}");
		// GD.Print($"Second List: {boundarySegmentList.ElementAt(1).Name}");
		// GD.Print($"Third List: {boundarySegmentList.ElementAt(2).Name}");
		// GD.Print($"Fourth List: {boundarySegmentList.ElementAt(3).Name}");
		List<(StringName name, Vector2 start, Vector2 end)> segmentPoints = boundarySegmentList
			.Select(shape =>
				{
					SegmentShape2D segment = (SegmentShape2D)shape.Shape;
					Vector2 shapeOriginPosition = shape.Position;
					Vector2 start = shapeOriginPosition + segment.A;
					Vector2 end = shapeOriginPosition + segment.B;
					if (shape.Name == "Top")
					{
						rect.Position = new Vector2(start.X, start.Y);
						rect.Size = new Vector2(end.X - start.X, rect.Size.Y);
					}
					else if (shape.Name == "Bottom")
					{
						rect.Size = new Vector2(rect.Size.X, start.Y - rect.Position.Y);
					}
					return (shape.Name, Start: start, End: end);
				})
			.ToList();
		// GD.Print($"Segment Points: ");
		// GD.Print($"First List: {segmentPoints.ElementAt(0)}");
		// GD.Print($"Second List: {segmentPoints.ElementAt(1)}");
		// GD.Print($"Third List: {segmentPoints.ElementAt(2)}");
		// GD.Print($"Fourth List: {segmentPoints.ElementAt(3)}");

		// foreach (CollisionShape2D shape in boundarySegmentList)
		// {
		// 	GD.Print($@"
		// 			Shape name: {shape.Name}
		// 			Shape shape: {shape.Shape}
		// 			Shape position: {shape.Position}");
		// }
	}
}
