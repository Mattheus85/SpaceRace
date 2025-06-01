using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float acceleration { get; set; } = 150.0f;
	[Export]
	public float rotationAcceleration { get; set; } = 2.5f;
	[Export]
	public float maxSpeed { get; set; } = 250.0f;
	[Export]
	public float playerMass { get; set; } = 10.0f;
	[Export]
	public float initialRotation = 0.0f;
	[Export]
	public float frictionCoefficient = 10.0f;  // Adjust as needed
	[Export]
	public float relativeFrictionVal = 0.1f;

	// Game inputs
	private Vector2 _inputVector;

	// Map boundaries
	private static readonly Vector2 _mapSize = new Vector2(19200, 10800);
	private static readonly Vector2 _mapMin = new Vector2(600, 400);
	private static readonly Vector2 _mapMax = _mapSize - _mapMin;

	// Energy Transfer properties
	private static float _bounceRestitution = 0.4f; // 0.0 = no bounce, 1.0 = full bounce
	private const float DEGREES_TO_RADIANS = 0.0174533f; // standard accepted value
	private const float ENERGY_TRANSFER_FACTOR = 0.4f; // Fraction of momentum transferred

	// Audio delay properties
	private double _lastCollisionTime = 0.0;
	private const double COLLISION_COOLDOWN = 0.8;

	// Player movement
	private Vector2 movementDirection;

	// Nodes
	private Camera2D _camera;
	private AnimationPlayer _animationPlayer;
	private AudioStreamPlayer2D _thrustAudio;
	private AudioStreamPlayer2D _boundaryAudio;
	private AudioStreamPlayer2D _bodyCollisionAudio;

	// Rate limits for collisions
	private float _collisionTimer = 0.0f;
	private int _collisionCount = 0;
	private const float COLLISION_WINDOW = 1.0f; // 1 second
	private const int MAX_COLLISIONS_PER_SECOND = 5; // Limit to 5 collisions per second

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_camera = GetNode<Camera2D>("Camera2D");
		_thrustAudio = GetNode<AudioStreamPlayer2D>("ThrustAudio");
		_boundaryAudio = GetNode<AudioStreamPlayer2D>("BoundaryAudio");
		_bodyCollisionAudio = GetNode<AudioStreamPlayer2D>("BodyCollisionAudio");
	}

	public override void _Process(double delta)
	{
		_inputVector.X = Input.GetAxis("ui_left", "ui_right");
		_inputVector.Y = Input.IsActionPressed("ui_up") ? 1 : 0;

		initialRotation = _inputVector.X != 0 ? (int)_inputVector.X : 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Update collision timer
		_collisionTimer += (float)delta;
		if (_collisionTimer >= COLLISION_WINDOW)
		{
			_collisionTimer -= COLLISION_WINDOW; // Reset timer
			_collisionCount = 0; // Reset count
		}

		Rotation += initialRotation * rotationAcceleration * (float)delta;
		if (_inputVector.Y > 0)
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

		KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta);

		if (ShouldHandleCollision(collision))
		{
			if (collision.GetCollider() is Node node)
			{
				if (node.IsInGroup("Boundary"))
				{
					HandleBoundaryCollision(collision);
				}
				else if (node.IsInGroup("Asteroid"))
				{
					HandleRigidBody2DCollision(node as RigidBody2D, collision);
				}
				else return;
			}
		}

		UpdateCameraPosition();
	}

	private bool ShouldHandleCollision(KinematicCollision2D collision)
	{
		return collision != null && _collisionCount < MAX_COLLISIONS_PER_SECOND;
	}

	private void HandleRigidBody2DCollision(RigidBody2D body, KinematicCollision2D collision)
	{
		body.ApplyImpulse(Velocity, collision.GetPosition() - body.GlobalPosition);
		ApplyImpulseToPlayer(body, collision);
		ShakeCamera(1, 1);
	}

	private void HandleBoundaryCollision(KinematicCollision2D collision)
	{
		if (collision == null || !(collision.GetCollider() is Node node && node.IsInGroup("Boundary")))
		{
			return; // No collision or not a boundary
		}

		// Get collision details
		Vector2 normal = collision.GetNormal(); // Points from boundary to ship
		Vector2 remainder = collision.GetRemainder();

		// Bounce velocity
		Vector2 newVelocity = BounceVelocity(Velocity, normal, 0.5f);

		// Update position to resolve collision
		GlobalPosition += remainder;

		// Apply new velocity
		Velocity = newVelocity;

		// Trigger camera shake and audio
		ShakeCamera(15, 5);
		if (!_boundaryAudio.Playing)
		{
			_boundaryAudio.Play();
		}
	}

	private void UpdateCameraPosition()
	{
		_camera.GlobalPosition = GlobalPosition;
	}

	private void ApplyImpulseToPlayer(RigidBody2D body, KinematicCollision2D collision)
	{
		// Get collision details
		Vector2 normal = collision.GetNormal(); // Points from body to character
		Vector2 collisionPoint = collision.GetPosition();

		// Get velocities
		Vector2 characterVelocity = Velocity; // CharacterBody2D velocity
		Vector2 bodyVelocity = body.LinearVelocity; // RigidBody2D linear velocity

		// Account for angular velocity of RigidBody2D
		Vector2 r = collisionPoint - body.GlobalPosition; // Vector from body's center to collision point
		Vector2 bodyPointVelocity = bodyVelocity + new Vector2(-body.AngularVelocity * r.Y, body.AngularVelocity * r.X);

		// Relative velocity at collision point
		Vector2 relativeVelocity = characterVelocity - bodyPointVelocity;

		// Project relative velocity onto the normal
		float velocityAlongNormal = relativeVelocity.Dot(normal);

		// Skip if bodies are moving apart
		if (velocityAlongNormal > 0)
		{
			return;
		}

		// Masses
		float bodyMass = body.Mass;

		// Impulse scalar
		float impulseMagnitude = -(1.0f + _bounceRestitution) * velocityAlongNormal;
		impulseMagnitude /= (1.0f / playerMass + 1.0f / bodyMass);

		// Impulse vector
		Vector2 impulse = impulseMagnitude * normal;

		// Update CharacterBody2D velocity
		Velocity += impulse / playerMass;

		// Optional: Move to resolve collision (if not using MoveAndSlide)
		Position += collision.GetRemainder();
		PlayCollisionAudio(body.Mass, Velocity - body.LinearVelocity, normal);
	}

	private bool ShouldRegisterCollision(Vector2 bodyFrictionImpulse, Vector2 shipFrictionImpulse)
	{
		return (bodyFrictionImpulse.Length() > shipFrictionImpulse.Length() * relativeFrictionVal);
	}

	private void PlayCollisionAudio(float bodyMass, Vector2 relativeVelocity, Vector2 normal)
	{
		double currentTime = Time.GetUnixTimeFromSystem();
		if (currentTime - _lastCollisionTime < COLLISION_COOLDOWN)
		{
			return; // Skip this collision audio
		}
		_lastCollisionTime = currentTime;

		// Use velocity along the normal for impact force
		float velocityAlongNormal = relativeVelocity.Dot(normal);
		float impactForce = Mathf.Abs(velocityAlongNormal) * MathF.Log10(bodyMass);

		// Define energy ranges (adjust based on your game)
		float minImpactForce = 0.0f;
		float maxImpactForc = 700.0f;

		float normalizedEnergy = Mathf.Clamp((impactForce - minImpactForce) / (maxImpactForc - minImpactForce), 0.0f, 1.0f);
		float dbValue = Mathf.Lerp(-20.0f, 0.0f, normalizedEnergy);

		if (_bodyCollisionAudio.Playing)
		{
			_bodyCollisionAudio.Stop();
		}
		_bodyCollisionAudio.VolumeDb = dbValue;
		_bodyCollisionAudio.Play();
		GD.Print("body mass : ", bodyMass);
		GD.Print("impactForce: ", impactForce);
		GD.Print("normalizedEnergy: ", normalizedEnergy);
		GD.Print("dbValue : ", dbValue);
	}

	private Vector2 BounceVelocity(Vector2 velocity, Vector2 normal)
	{
		// Reflect velocity: v' = v - 2 * (v · n) * n
		Vector2 reflected = velocity - 2 * velocity.Dot(normal) * normal;

		// Apply restitution (energy conservation)
		reflected *= _bounceRestitution;

		return reflected;
	}

	private Vector2 BounceVelocity(Vector2 velocity, Vector2 normal, float additionalBounce)
	{
		// Reflect velocity: v' = v - 2 * (v · n) * n
		Vector2 reflected = velocity - 2 * velocity.Dot(normal) * normal;

		// Apply restitution (energy conservation)
		reflected *= _bounceRestitution + additionalBounce;

		return reflected;
	}

	private void ShakeCamera(int duration, int offset)
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
		Velocity = Velocity.LimitLength(maxSpeed); // Cap at max_speed
	}
}
