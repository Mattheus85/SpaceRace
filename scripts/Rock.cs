using Godot;

public partial class Rock : RigidBody2D
{
	[Export] private Vector2 _minSize = new Vector2(16, 16);
	[Export] private Vector2 _maxSize = new Vector2(64, 64);
	[Export] private float _minAlpha = 0.2f;
	[Export] private float _maxAlpha = 0.8f;
	[Export] private Vector2 _minLinearVelocity = new Vector2(-50, -50);
	[Export] private Vector2 _maxLinearVelocity = new Vector2(50, 50);
	[Export] private Color _color = Colors.White;
	[Export] private bool _flipH;
	[Export] private float _minAngularVelocity = 0.0f;
	[Export] private float _maxAngularVelocity = 5.0f;
	[Export] private bool _isForeground = true;
	[Export] private float _parallaxFactor = 1.0f;
	[Export] private float _minBaseDensity = 0.8f;
	[Export] private float _maxBaseDensity = 1.2f;
	[Export] private string _animationName = "default";
	[Export] private Vector2 _playableAreaMin = new Vector2(-2500, -2500); // Map bounds
	[Export] private Vector2 _playableAreaMax = new Vector2(2500, 2500);
	[Export] private float _spawnMargin = 100.0f;

	private AnimatedSprite2D _animatedSprite;
	private CollisionPolygon2D _collisionPolygon;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite");
		_collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon");

		// Randomize position within playable area
		Position = new Vector2(
			(float)GD.RandRange(_playableAreaMin.X + _spawnMargin, _playableAreaMax.X - _spawnMargin),
			(float)GD.RandRange(_playableAreaMin.Y + _spawnMargin, _playableAreaMax.Y - _spawnMargin)
		);

		// Randomize size
		// if rock is > 1.2x larger than spaceship, then use modulo spaceship.size
		float size_calc = (float)GD.RandRange(_minSize.X, _maxSize.X);
		Vector2 size = new Vector2(size_calc, size_calc);

		// Calculate mass based on density(0.8-1.2) and area
		float density = (float)GD.RandRange(_minBaseDensity, _maxBaseDensity);
		float area = Mathf.Pi * Mathf.Pow((size.X / 2), 2);
		float mass = density * Mathf.Pow(area, 1.5f);

		// Randomize background alpha and velocity
		float alpha = _isForeground ? 1.0f : (float)GD.RandRange(_minAlpha, _maxAlpha);
		float angularVelocity = (float)GD.RandRange(_minAngularVelocity, _maxAngularVelocity);
		Vector2 linearVelocity = new Vector2(
			(float)GD.RandRange(_minLinearVelocity.X, _maxLinearVelocity.X),
			(float)GD.RandRange(_minLinearVelocity.Y, _maxLinearVelocity.Y)
		);

		// Apply properties
		_animatedSprite.Scale = size / _animatedSprite.SpriteFrames.GetFrameTexture(
				_animationName, 0).GetSize();
		_collisionPolygon.Scale = _animatedSprite.Scale;
		Mass = mass;
		_animatedSprite.Modulate = new Color(_color, alpha);
		_animatedSprite.FlipH = _flipH;
		_animationName = _animatedSprite.FlipH ? "reversed" : "default";
		AngularVelocity = angularVelocity;
		LinearVelocity = linearVelocity * (_isForeground ? 1.0f : _parallaxFactor);
		ZIndex = _isForeground ? 1 : -1;
		_collisionPolygon.Disabled = !_isForeground;

		// Play animation if available
		if (_animatedSprite.SpriteFrames.HasAnimation(_animationName))
		{
			_animatedSprite.Play(_animationName);
		}
	}
}
