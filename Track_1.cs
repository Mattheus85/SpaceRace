using Godot;

public partial class Track_1 : StaticBody2D
{
	[Export] private Polygon2D _polygon2D;
	[Export] private CollisionPolygon2D _collisionPolygon2D;

	public override void _Ready()
	{
		if (_polygon2D != null && _collisionPolygon2D != null)
		{
			_polygon2D.Polygon = _polygon2D.Polygon; // Ensure points are set
			_collisionPolygon2D.Polygon = _polygon2D.Polygon; // Sync points
		}
	}
}
