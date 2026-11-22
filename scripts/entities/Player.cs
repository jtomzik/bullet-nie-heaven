using Godot;
using BulletNieHeaven.Entities;

namespace BulletNieHeaven;

/// <summary>
/// Reprezentuje postać gracza sterowaną przez użytkownika.
/// Obsługuje poruszanie się (WSAD/strzałki), kolizje ze ścianami,
/// automatyczny atak na najbliższego wroga, zarządzanie zdrowiem (HealthComponent),
/// chwilową nietykalność (I-Frames) oraz zgłaszanie zdarzeń do GameEvents.
/// </summary>
public partial class Player : CharacterBody2D, IDamageable
{
	[ExportGroup("Ruch")]
	/// <summary>
	/// Prędkość poruszania się gracza w pikselach na sekundę.
	/// </summary>
	[Export]
	public float Speed { get; set; } = 300.0f;

	[ExportGroup("Życie i Obrona")]
	/// <summary>
	/// Maksymalna pula punktów życia gracza.
	/// </summary>
	[Export]
	public float MaxHealth { get; set; } = 100.0f;

	/// <summary>
	/// Czas trwania chwilowej nietykalności po otrzymaniu obrażeń (w sekundach).
	/// </summary>
	[Export]
	public float InvulnerabilityDuration { get; set; } = 0.8f;

	/// <summary>
	/// Komponent punktów życia gracza.
	/// </summary>
	[Export]
	public HealthComponent Health { get; set; }

	[ExportGroup("Automatyczny Atak")]
	/// <summary>
	/// Scena pocisku wystrzeliwanego podczas ataku.
	/// Jeśli nie zostanie przypisana w Inspektorze, załaduje się automatycznie res://scenes/entities/bullet.tscn.
	/// </summary>
	[Export]
	public PackedScene BulletScene { get; set; }

	/// <summary>
	/// Zasięg wykrywania przeciwników w pikselach.
	/// </summary>
	[Export]
	public float AttackRange { get; set; } = 400.0f;

	/// <summary>
	/// Odstęp czasu między kolejnymi strzałami (w sekundach).
	/// </summary>
	[Export]
	public float AttackCooldown { get; set; } = 0.8f;

	/// <summary>
	/// Prędkość lotu wystrzeliwanych pocisków.
	/// </summary>
	[Export]
	public float BulletSpeed { get; set; } = 600.0f;

	/// <summary>
	/// Wartość obrażeń zadawanych przez każdy pocisk.
	/// </summary>
	[Export]
	public float BulletDamage { get; set; } = 25.0f;

	/// <summary>
	/// Czas życia pocisku w sekundach zanim zniknie.
	/// </summary>
	[Export]
	public float BulletLifetime { get; set; } = 2.0f;

	private float _attackTimer = 0.0f;
	private Tween _invulnerabilityTween;

	public override void _Ready()
	{
		AddToGroup("player");

		// Inicjalizacja HealthComponent
		if (Health == null)
		{
			Health = GetNodeOrNull<HealthComponent>("HealthComponent");
			if (Health == null)
			{
				Health = new HealthComponent();
				Health.Name = "HealthComponent";
				AddChild(Health);
			}
		}

		Health.InvulnerabilityDuration = InvulnerabilityDuration;
		Health.Initialize(MaxHealth);
		Health.Damaged += OnPlayerDamaged;
		Health.Died += OnPlayerDied;
		Health.InvulnerabilityStarted += OnInvulnerabilityStarted;
		Health.InvulnerabilityEnded += OnInvulnerabilityEnded;

		// Wysłanie początkowego stanu zdrowia do UI / szyny zdarzeń
		GameEvents.EmitPlayerHealthChanged(Health.CurrentHealth, Health.MaxHealth);

		// Domyślne załadowanie sceny pocisku, jeśli nie ustawiono w Inspektorze
		if (BulletScene == null)
		{
			BulletScene = GD.Load<PackedScene>("res://scenes/entities/bullet.tscn");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Health != null && Health.IsDead)
		{
			return;
		}

		HandleMovement();
		CheckEnemyContactCollisions();
		HandleAutoAttack((float)delta);
	}

	/// <summary>
	/// Obsługuje płynne poruszanie się postaci (WSAD / strzałki) bez przyspieszenia po skosie.
	/// </summary>
	private void HandleMovement()
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (direction != Vector2.Zero)
		{
			Velocity = direction * Speed;
		}
		else
		{
			Velocity = Vector2.Zero;
		}

		MoveAndSlide();
	}

	/// <summary>
	/// Sprawdza kolizje kontaktowe z przeciwnikami i zadaje graczowi obrażenia kontaktowe.
	/// </summary>
	private void CheckEnemyContactCollisions()
	{
		int collisionCount = GetSlideCollisionCount();
		for (int i = 0; i < collisionCount; i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);
			if (collision.GetCollider() is Node collider)
			{
				if (collider.IsInGroup("enemies") || collider.IsInGroup("enemy"))
				{
					float contactDamage = 15.0f;
					if (collider is DummyEnemy dummy)
					{
						contactDamage = dummy.ContactDamage;
					}

					TakeDamage(contactDamage);
					break;
				}
			}
		}
	}

	/// <summary>
	/// Implementacja IDamageable - przekazuje obrażenia do HealthComponent.
	/// </summary>
	/// <param name="amount">Wartość obrażeń.</param>
	public void TakeDamage(float amount)
	{
		Health?.Damage(amount);
	}

	/// <summary>
	/// Obsługa otrzymania obrażeń przez gracza - emisja do GameEvents.
	/// </summary>
	private void OnPlayerDamaged(float amount)
	{
		GD.Print($"[Player] Otrzymano {amount} obrażeń! Pozostałe HP: {Health.CurrentHealth}/{Health.MaxHealth}");
		GameEvents.EmitPlayerDamaged(Health.CurrentHealth, Health.MaxHealth);
	}

	/// <summary>
	/// Efekt wizualny nietykalności - migotanie postaci.
	/// </summary>
	private void OnInvulnerabilityStarted()
	{
		_invulnerabilityTween?.Kill();
		_invulnerabilityTween = CreateTween().SetLoops();
		_invulnerabilityTween.TweenProperty(this, "modulate:a", 0.3f, 0.1f);
		_invulnerabilityTween.TweenProperty(this, "modulate:a", 1.0f, 0.1f);
	}

	/// <summary>
	/// Przywrócenie standardowego wyglądu po zakończeniu nietykalności.
	/// </summary>
	private void OnInvulnerabilityEnded()
	{
		_invulnerabilityTween?.Kill();
		_invulnerabilityTween = null;
		Modulate = Colors.White;
	}

	/// <summary>
	/// Obsługa śmierci gracza.
	/// </summary>
	private void OnPlayerDied()
	{
		GD.Print("[Player] Gracz zginął! Koniec gry.");
		_invulnerabilityTween?.Kill();
		Modulate = new Color(0.4f, 0.4f, 0.4f, 0.8f);
		GameEvents.EmitPlayerDied();
	}

	/// <summary>
	/// Odlicza czas odnowienia ataku i wykonuje wystrzał do najbliższego wroga w zasięgu.
	/// </summary>
	private void HandleAutoAttack(float delta)
	{
		_attackTimer += delta;

		if (_attackTimer >= AttackCooldown)
		{
			Node2D nearestEnemy = FindNearestEnemy();

			if (nearestEnemy != null)
			{
				ShootAt(nearestEnemy.GlobalPosition);
				_attackTimer = 0.0f;
			}
		}
	}

	/// <summary>
	/// Wyszukuje najbliższego przeciwnika w obrębie zasięgu ataku (AttackRange).
	/// </summary>
	public Node2D FindNearestEnemy()
	{
		Node2D nearestEnemy = null;
		float minDistanceSquared = AttackRange * AttackRange;

		var enemies = GetTree().GetNodesInGroup("enemies");
		foreach (Node node in enemies)
		{
			if (node is Node2D enemy2D && IsInstanceValid(enemy2D))
			{
				float distSq = GlobalPosition.DistanceSquaredTo(enemy2D.GlobalPosition);
				if (distSq <= minDistanceSquared)
				{
					minDistanceSquared = distSq;
					nearestEnemy = enemy2D;
				}
			}
		}

		if (nearestEnemy == null)
		{
			var altEnemies = GetTree().GetNodesInGroup("enemy");
			foreach (Node node in altEnemies)
			{
				if (node is Node2D enemy2D && IsInstanceValid(enemy2D))
				{
					float distSq = GlobalPosition.DistanceSquaredTo(enemy2D.GlobalPosition);
					if (distSq <= minDistanceSquared)
					{
						minDistanceSquared = distSq;
						nearestEnemy = enemy2D;
					}
				}
			}
		}

		return nearestEnemy;
	}

	/// <summary>
	/// Tworzy pocisk i wystrzeliwuje go w stronę pozycji wskazanego celu.
	/// </summary>
	public void ShootAt(Vector2 targetPosition)
	{
		if (BulletScene == null)
		{
			GD.PrintErr("[Player] BulletScene nie jest przypisana!");
			return;
		}

		Bullet bullet = BulletScene.Instantiate<Bullet>();
		if (bullet == null)
		{
			GD.PrintErr("[Player] Nie udało się zinstancjonować sceny Bullet!");
			return;
		}

		Vector2 direction = (targetPosition - GlobalPosition).Normalized();
		bullet.GlobalPosition = GlobalPosition;
		bullet.Initialize(direction, BulletSpeed, BulletDamage, BulletLifetime);

		Node parent = GetParent();
		if (parent != null)
		{
			parent.AddChild(bullet);
		}
		else
		{
			GetTree().CurrentScene?.AddChild(bullet);
		}
	}
}
