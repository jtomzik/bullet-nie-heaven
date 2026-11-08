using Godot;

namespace BulletNieHeaven.Entities;

/// <summary>
/// Obiekt manekina treningowego przeciwnika do testowania namierzania,
/// kolizji pocisków i mechaniki zadawania obrażeń.
/// </summary>
public partial class DummyEnemy : CharacterBody2D, IDamageable
{
	/// <summary>
	/// Maksymalne punkty życia manekina.
	/// </summary>
	[Export]
	public float MaxHealth { get; set; } = 50.0f;

	/// <summary>
	/// Aktualne punkty życia manekina.
	/// </summary>
	public float CurrentHealth { get; private set; }

	public override void _Ready()
	{
		AddToGroup("enemies");
		CurrentHealth = MaxHealth;
	}

	/// <summary>
	/// Przyjmuje obrażenia zadane przez pocisk i weryfikuje stan życia.
	/// </summary>
	/// <param name="amount">Wartość otrzymanych obrażeń.</param>
	public void TakeDamage(float amount)
	{
		CurrentHealth -= amount;
		GD.Print($"[DummyEnemy {Name}] Trafienie! Obrażenia: {amount}. Pozostałe HP: {CurrentHealth}/{MaxHealth}");

		// Efekt wizualny trafienia - krótkie mignięcie na czerwono
		Modulate = Colors.Crimson;
		GetTree().CreateTimer(0.1).Timeout += () =>
		{
			if (IsInstanceValid(this))
			{
				Modulate = Colors.White;
			}
		};

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	/// <summary>
	/// Likwiduje obiekt przeciwnika po wyczerpaniu punktów życia.
	/// </summary>
	private void Die()
	{
		GD.Print($"[DummyEnemy {Name}] Został pokonany!");
		QueueFree();
	}
}
