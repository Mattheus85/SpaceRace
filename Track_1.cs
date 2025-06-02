using Godot;

public partial class Track_1 : StaticBody2D
{
	[Export]
	private Polygon2D _polygon2D;
	[Export]
	private CollisionPolygon2D _collisionPolygon2D;
	[Export]
	private Color _vertexColor = new Color(0, 255, 100, 1.0f);

	public override void _Ready()
	{
		if (_polygon2D != null && _collisionPolygon2D != null)
		{
			_polygon2D.Polygon = _polygon2D.Polygon; // Ensure points are set
			_collisionPolygon2D.Polygon = _polygon2D.Polygon; // Sync points
		}
	}

	public override void _Draw()
	{
		if (_polygon2D != null && _polygon2D.Polygon.Length > 0)
		{
			Vector2[] points = new Vector2[_polygon2D.Polygon.Length + 1];
			_polygon2D.Polygon.CopyTo(points, 0);
			points[points.Length - 1] = points[0];

			DrawPolyline(points, _vertexColor, 4.0f, true);
		}
	}
}
