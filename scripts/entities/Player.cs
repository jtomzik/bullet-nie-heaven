using Godot;
using BulletNieHeaven.Entities;

namespace BulletNieHeaven;

/// <summary>
/// Reprezentuje postać gracza sterowaną przez użytkownika.
/// Obsługuje poruszanie się (WSAD/strzałki), kolizje ze ścianami
/// oraz automatyczny atak na najbliższego wroga w zasięgu.
/// </summary>
public partial class Player : CharacterBody2D
{
	[ExportGroup("Ruch")]
	/// <summary>
	/// Prędkość poruszania się gracza w pikselach na sekundę.
	/// </summary>
	[Export]
	public float Speed { get; set; } = 300.0f;

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

	public override void _Ready()
	{
		AddToGroup("player");

		// Domyślne załadowanie sceny pocisku, jeśli nie ustawiono w Inspektorze
		if (BulletScene == null)
		{
			BulletScene = GD.Load<PackedScene>("res://scenes/entities/bullet.tscn");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement();
		HandleAutoAttack((float)delta);
	}

	/// <summary>
	/// Obsługuje płynne poruszanie się postaci (WSAD / strzałki) bez przyspieszenia po skosie.
	/// </summary>
	private void HandleMovement()
	{
		// GetVector pobiera kierunki WSAD/strzałek i automatycznie normalizuje ruch po skosie
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (direction != Vector2.Zero)
		{
			Velocity = direction * Speed;
		}
		else
		{
			Velocity = Vector2.Zero;
		}

		// move_and_slide obsługuje fizykę kolizji ze ścianami areny
		MoveAndSlide();
	}

	/// <summary>
	/// Odlicza czas odnowienia ataku i wykonuje wystrzał do najbliższego wroga w zasięgu.
	/// </summary>
	/// <param name="delta">Czas od ostatniej klatki fizyki.</param>
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
	/// Weryfikuje węzły należące do grup 'enemies' oraz 'enemy'.
	/// </summary>
	/// <returns>Węzeł Node2D najbliższego wroga lub null, gdy żaden nie znajduje się w zasięgu.</returns>
	public Node2D FindNearestEnemy()
	{
		Node2D nearestEnemy = null;
		float minDistanceSquared = AttackRange * AttackRange;

		// Sprawdzenie wrogów z grupy "enemies"
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

		// Sprawdzenie alternatywnej grupy "enemy" dla pełnej kompatybilności
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
	/// Pocisk dodawany jest do sceny nadrzędnej, by poruszał się niezależnie od gracza.
	/// </summary>
	/// <param name="targetPosition">Współrzędne celu w przestrzeni globalnej.</param>
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

		// Dodajemy pocisk do rodzica gracza (np. sceny areny/świata),
		// aby nie poruszał się wraz z ruchem gracza
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
