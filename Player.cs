using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float acceleration { get; set; } = 150.0f;
	[Export]
	public float rotation_acceleration { get; set; } = 2.5f;
	[Export]
	public float max_speed { get; set; } = 250.0f;
	[Export]
	public float mass { get; set; } = 0.1f;
	[Export]
	public float initial_rotation = 0.0f;
	[Export]
	public float friction_coefficient = 10.0f;  // Adjust as needed
	[Export]
	public float relative_friction_val = 0.1f;

	// Map boundaries
	private static readonly Vector2 MapSize = new Vector2(19200, 10800);
	private static readonly Vector2 MapMin = new Vector2(600, 400);
	private static readonly Vector2 MapMax = MapSize - MapMin;

	// Energy Transfer properties
	private static float BounceRestitution = 0.1f; // 0.0 = no bounce, 1.0 = full bounce
	private static float RandomCaromAngle = 30.0f; // Max random angle deviation in degrees
	private const float DegressToRadians = 0.0174533f; // standard accepted value
	private const float EnergyTransferFactor = 0.5f; // Fraction of momentum transferred

	private Camera2D _camera;
	private AnimationPlayer _animationPlayer;
	private AudioStreamPlayer2D _thrustAudio;
	private AudioStreamPlayer2D _boundaryAudio;
	private AudioStreamPlayer2D _rockCollisionAudio;

	Vector2 input_vector;
	float push_force;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_camera = GetNode<Camera2D>("Camera2D");
		_thrustAudio = GetNode<AudioStreamPlayer2D>("ThrustAudio");
		_boundaryAudio = GetNode<AudioStreamPlayer2D>("BoundaryAudio");
		_rockCollisionAudio = GetNode<AudioStreamPlayer2D>("RockCollisionAudio");
	}

	public override void _Process(double delta)
	{
		input_vector.X = Input.GetAxis("ui_left", "ui_right");
		input_vector.Y = Input.IsActionPressed("ui_up") ? 1 : 0;

		initial_rotation = input_vector.X != 0 ? (int)input_vector.X : 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		Rotation += initial_rotation * rotation_acceleration * (float)delta;
		if (input_vector.Y > 0)
		{
			_animationPlayer.Play("thrust");
			if (!_thrustAudio.Playing) _thrustAudio.Play();
			thrust(delta);
		}
		else
		{
			_animationPlayer.Play("stop");
			_thrustAudio.Stop();
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
				RigidBody2D body = c.GetCollider() as RigidBody2D;
				ApplyImpulseToRock(body, c);
			}

		}
	}

	private void ApplyImpulseToRock(RigidBody2D rock, KinematicCollision2D collision)
	{
		// Get rock's mass
		float rockMass = rock.Mass;
		float rockBounciness = rock.PhysicsMaterialOverride != null ? rock.PhysicsMaterialOverride.Bounce : 0.0f;

		// Save initial velocity
		Vector2 initialVelocity = Velocity;

		// Calculate relative velocity
		Vector2 relativeVelocity = Velocity - rock.LinearVelocity;

		// Get collision direction of impact, from rock toward player
		Vector2 normal = collision.GetNormal();

		// Project relative velocity onto the normalized collision direction
		float velocityAlongNormal = relativeVelocity.Dot(normal);

		// Skip if objects are moving apart
		if (velocityAlongNormal > 0)
			return;

		// Calculate restitution based on player(default 0.1) and rock(0.0) bounciness
		float restitution = (BounceRestitution + rockBounciness) / 2.0f;

		// Calculate impulse scalar using 1D collision formula
		float impulseScalar = -(1 + restitution) * velocityAlongNormal;
		impulseScalar /= (1 / mass + 1 / rockMass);
		// GD.Print("relativeVelocity: ", relativeVelocity);
		// GD.Print("velocityAlongNormal: ", velocityAlongNormal);
		// GD.Print("impulseScalar: ", impulseScalar);
		// GD.Print("normal: ", normal);

		// Calculate impulse vector
		Vector2 impulse = impulseScalar * normal;
		// GD.Print("impulse: ", impulse);

		// Apply friction to reduce sliding
		Vector2 tangentialVelocity = relativeVelocity - (velocityAlongNormal * normal);
		Vector2 frictionImpulse = -tangentialVelocity * friction_coefficient * mass;
		// GD.Print("frictionImpulse.Length: ", frictionImpulse.Length());
		// GD.Print("impulse.Length: ", impulse.Length());

		if (frictionImpulse.Length() > impulse.Length() * relative_friction_val) {
			frictionImpulse = frictionImpulse.Normalized() * impulse.Length();

			float collisionAudioDbLevel = (relativeVelocity.Length() * (rockMass - mass) / 100);
			float normalizedDb = Mathf.Lerp(-60.0f, 0.0f, Mathf.Clamp((collisionAudioDbLevel - 0.1f) / (15.0f - 0.1f), 0.0f, 1.0f));
			float dbVolume = Mathf.DbToLinear(normalizedDb);
			_rockCollisionAudio.SetVolumeLinear(dbVolume);
			if (!_rockCollisionAudio.Playing) _rockCollisionAudio.Play();
			GD.Print("dbVolume : ", dbVolume);
			GD.Print("_rockCollisionAudio.GetVolumeLinear : ", _rockCollisionAudio.GetVolumeLinear() );

			// Apply impulses
			rock.ApplyImpulse(-impulse - frictionImpulse, collision.GetPosition() - rock.GlobalPosition);
			Vector2 playerImpulse = (impulse + frictionImpulse) * EnergyTransferFactor;
			Velocity += playerImpulse / mass;
			Velocity = Velocity.LimitLength(max_speed); // cap at max_speed
			
			if (frictionImpulse.Length() < 0.01f) {
				Velocity += normal * 5.0f; // Small nudge away from rock
			}

			// Trigger impact effects
			TriggerImpactEffects(1, 1);
		}

		// Apply opposite impulse to player (Newton's third law)
		// Vector2 playerImpulse = impulse / EnergyTransferFactor;
		// Velocity += playerImpulse / mass; // Update velocity (F = ma, so v += impulse/m)
		// Velocity = Velocity.LimitLength(max_speed); // cap at max_speed

		// Optional: Add random carom to player velocity
		// float randomAngle = (float)GD.RandRange(-RandomCaromAngle, RandomCaromAngle) * DegressToRadians;
		// Velocity = Velocity.Rotated(randomAngle);
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
				TriggerImpactEffects(15, 5);
				if (!_boundaryAudio.Playing) _boundaryAudio.Play();
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

	private void TriggerImpactEffects(int duration, int offset)
	{
		// Screen shake
		var tween = CreateTween();
		for (int i = 0; i < duration; i++) // shake camera for {duration} number of frames
		{
			tween.TweenProperty(_camera, "offset", new Vector2(
				(float)GD.RandRange(-offset, offset), // Random offset of ±{offset} pixels
				(float)GD.RandRange(-offset, offset)
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
