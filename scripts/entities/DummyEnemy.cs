using Godot;

namespace BulletNieHeaven.Entities;

/// <summary>
/// Obiekt przeciwnika do testowania namierzania, kolizji pocisków,
/// systemu punktów życia (HealthComponent), reakcji na obrażenia i likwidacji (QueueFree).
/// </summary>
public partial class DummyEnemy : CharacterBody2D, IDamageable
{
	[ExportGroup("Statystyki")]
	/// <summary>
	/// Maksymalne punkty życia manekina.
	/// </summary>
	[Export]
	public float MaxHealth { get; set; } = 50.0f;

	/// <summary>
	/// Obrażenia zadawane graczowi przy bezpośrednim kontakcie.
	/// </summary>
	[Export]
	public float ContactDamage { get; set; } = 15.0f;

	[ExportGroup("Komponenty")]
	/// <summary>
	/// Komponent zarządzający punktami życia przeciwnika.
	/// </summary>
	[Export]
	public HealthComponent Health { get; set; }

	/// <summary>
	/// Zwraca aktualne punkty życia z komponentu zdrowia.
	/// </summary>
	public float CurrentHealth => Health?.CurrentHealth ?? 0.0f;

	public override void _Ready()
	{
		AddToGroup("enemies");

		// Zapewnienie obecności HealthComponent
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

		Health.Initialize(MaxHealth);
		Health.Damaged += OnDamaged;
		Health.Died += OnDied;
	}

	/// <summary>
	/// Implementacja interfejsu IDamageable - przekazuje obrażenia do HealthComponent.
	/// </summary>
	/// <param name="amount">Wartość otrzymanych obrażeń.</param>
	public void TakeDamage(float amount)
	{
		Health?.Damage(amount);
	}

	/// <summary>
	/// Reakcja wizualna na otrzymanie obrażeń (mignięcie na czerwono).
	/// </summary>
	/// <param name="amount">Wartość zadanych obrażeń.</param>
	private void OnDamaged(float amount)
	{
		GD.Print($"[DummyEnemy {Name}] Trafienie! Obrażenia: {amount}. Pozostałe HP: {Health.CurrentHealth}/{Health.MaxHealth}");

		// Efekt wizualny trafienia - krótkie mignięcie na karmazynowy
		Modulate = Colors.Crimson;
		GetTree().CreateTimer(0.1).Timeout += () =>
		{
			if (IsInstanceValid(this))
			{
				Modulate = Colors.White;
			}
		};
	}

	/// <summary>
	/// Likwiduje obiekt przeciwnika po wyczerpaniu punktów życia (QueueFree)
	/// i emituje globalne zdarzenie OnEnemyDied.
	/// </summary>
	private void OnDied()
	{
		GD.Print($"[DummyEnemy {Name}] Został pokonany! Wywołanie QueueFree().");
		GameEvents.EmitEnemyDied(GlobalPosition);
		QueueFree();
	}
}
