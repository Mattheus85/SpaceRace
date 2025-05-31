using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float acceleration { get; set; } = 90.0f;
	[Export]
	public float rotation_acceleration { get; set; } = 2.5f;
	[Export]
	public float max_speed { get; set; } = 150.0f;

	// Map boundaries
	private static readonly Vector2 MapSize = new Vector2(19200, 10800);
	private static readonly Vector2 MapMin = new Vector2(600, 400);
	private static readonly Vector2 MapMax = MapSize - MapMin;

	// Bounce properties
	private static float BounceRestitution = 0.7f; // 0.0 = no bounce, 1.0 = full bounce
	private static float RandomCaromAngle = 30.0f; // Max random angle deviation in degrees

	private const float DegressToRadians = 0.0174533f;

	private Camera2D _camera;
	private AnimationPlayer _animationPlayer;

	Vector2 input_vector;
	int rotation_direction = 0;
	float mass = 0.01f;
	float push_force;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_camera = GetNode<Camera2D>("Camera2D");
	}

	public override void _Process(double delta)
	{
		input_vector.X = Input.GetAxis("ui_left", "ui_right");
		input_vector.Y = Input.IsActionPressed("ui_up") ? 1 : 0;

		rotation_direction = input_vector.X != 0 ? (int)input_vector.X : 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		Rotation += rotation_direction * rotation_acceleration * (float)delta;
		if (input_vector.Y > 0)
		{
			_animationPlayer.Play("thrust");
			thrust(delta);
		}
		else
		{
			_animationPlayer.Play("stop");
		}

		MoveAndSlide();

		// Handle boundary collision with bounce
		HandleBoundaryCollision();

		// Handle collisions with world objects
		HandleObjectCollisions();

		// Update camera position
		_camera.GlobalPosition = GlobalPosition;
	}

	private void HandleObjectCollisions()
	{
		float ship_speed = Velocity.LengthSquared();
		push_force = mass * ship_speed;
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D c = GetSlideCollision(i);
			if (c.GetCollider() is RigidBody2D)
			{
				RigidBody2D rock = c.GetCollider() as RigidBody2D;
				rock.ApplyCentralImpulse(-c.GetNormal() * push_force);
			}

		}
	}
	private void HandleBoundaryCollision()
	{
		Vector2 pos = GlobalPosition;
		Vector2 newPos = pos;
		Vector2 newVelocity = Velocity;
		bool hitBoundary = false;

		// Check each boundary and apply bounce
		if (pos.X <= MapMin.X)
		{
			newPos.X = MapMin.X;
			newVelocity = BounceVelocity(newVelocity, new Vector2(1, 0)); // Normal vector: right
			hitBoundary = true;
		}
		else if (pos.X >= MapMax.X)
		{
			newPos.X = MapMax.X;
			newVelocity = BounceVelocity(newVelocity, new Vector2(-1, 0)); // Normal vector: left
			hitBoundary = true;
		}

		if (pos.Y <= MapMin.Y)
		{
			newPos.Y = MapMin.Y;
			newVelocity = BounceVelocity(newVelocity, new Vector2(0, 1)); // Normal vector: down
			hitBoundary = true;
		}
		else if (pos.Y >= MapMax.Y)
		{
			newPos.Y = MapMax.Y;
			newVelocity = BounceVelocity(newVelocity, new Vector2(0, -1)); // Normal vector: up
			hitBoundary = true;
		}

		// Apply new position and velocity if changed
		if (newPos != pos)
		{
			GlobalPosition = newPos;
			Velocity = newVelocity;
			if (hitBoundary)
			{
				TriggerImpactEffects();
			}
		}
	}

	private Vector2 BounceVelocity(Vector2 velocity, Vector2 normal)
	{
		// Reflect velocity: v' = v - 2 * (v · n) * n
		Vector2 reflected = velocity - 2 * velocity.Dot(normal) * normal;

		// Apply restitution (energy conservation)
		reflected *= BounceRestitution;

		// Add random carom effect by rotating the reflected velocity
		float randomAngle = (float)GD.RandRange(-RandomCaromAngle, RandomCaromAngle) * DegressToRadians;
		return reflected.Rotated(randomAngle);
	}

	private void TriggerImpactEffects()
	{
		// Screen shake
		var tween = CreateTween();
		for (int i = 0; i < 5; i++)
		{
			tween.TweenProperty(_camera, "offset", new Vector2(
				(float)GD.RandRange(-5, 5), // Random offset ±5 pixels
				(float)GD.RandRange(-5, 5)
			), 0.05);
		}
		tween.TweenProperty(_camera, "offset", Vector2.Zero, 0.05);

		// Optional: Play crash sound
		// GetNode<AudioStreamPlayer2D>("CrashSound")?.Play();
	}

	private void thrust(double delta)
	{
		// Accelerate in the facing direction (Rotation = 0 is up)
		Vector2 thrustDirection = new Vector2(0, -1).Rotated(Rotation); // Upward vector rotated by ship's angle
		Velocity += thrustDirection * acceleration * (float)delta; // Add acceleration
		Velocity = Velocity.LimitLength(max_speed); // Cap at max_speed
	}
}
