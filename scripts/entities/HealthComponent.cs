using Godot;

namespace BulletNieHeaven.Entities;

/// <summary>
/// Komponent odpowiedzialny za zarządzanie punktami życia (HP), otrzymywanie obrażeń,
/// leczenie oraz obsługę chwilowej nietykalności (I-Frames).
/// Może być dołączony do dowolnej encji w grze (gracz, wrogowie, obiekty).
/// </summary>
public partial class HealthComponent : Node
{
	/// <summary>
	/// Sygnał emitowany przy zmianie aktualnych punktów życia.
	/// </summary>
	[Signal]
	public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

	/// <summary>
	/// Sygnał emitowany w momencie otrzymania obrażeń.
	/// </summary>
	[Signal]
	public delegate void DamagedEventHandler(float amount);

	/// <summary>
	/// Sygnał emitowany przy uleczeniu.
	/// </summary>
	[Signal]
	public delegate void HealedEventHandler(float amount);

	/// <summary>
	/// Sygnał emitowany, gdy punkty życia spadną do zera.
	/// </summary>
	[Signal]
	public delegate void DiedEventHandler();

	/// <summary>
	/// Sygnał emitowany w momencie aktywacji okresu nietykalności.
	/// </summary>
	[Signal]
	public delegate void InvulnerabilityStartedEventHandler();

	/// <summary>
	/// Sygnał emitowany po zakończeniu okresu nietykalności.
	/// </summary>
	[Signal]
	public delegate void InvulnerabilityEndedEventHandler();

	/// <summary>
	/// Maksymalna pula punktów życia.
	/// </summary>
	[Export]
	public float MaxHealth { get; set; } = 100.0f;

	/// <summary>
	/// Czas trwania chwilowej nietykalności po otrzymaniu trafienia (w sekundach).
	/// Domyślnie 0 (brak nietykalności dla standardowych przeciwników).
	/// </summary>
	[Export]
	public float InvulnerabilityDuration { get; set; } = 0.0f;

	/// <summary>
	/// Aktualny poziom punktów życia encji.
	/// </summary>
	public float CurrentHealth { get; private set; }

	/// <summary>
	/// Określa, czy encja jest obecnie w stanie chwilowej nietykalności.
	/// </summary>
	public bool IsInvulnerable { get; private set; } = false;

	/// <summary>
	/// Zwraca true, jeśli punkty życia zostały wyzerowane.
	/// </summary>
	public bool IsDead => CurrentHealth <= 0.0f;

	private float _invulnerabilityTimer = 0.0f;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
	}

	public override void _Process(double delta)
	{
		if (IsInvulnerable)
		{
			_invulnerabilityTimer -= (float)delta;
			if (_invulnerabilityTimer <= 0.0f)
			{
				IsInvulnerable = false;
				EmitSignal(SignalName.InvulnerabilityEnded);
			}
		}
	}

	/// <summary>
	/// Inicjalizuje lub resetuje parametry punktów życia.
	/// </summary>
	/// <param name="maxHealth">Nowa maksymalna pula życia.</param>
	/// <param name="currentHealth">Opcjonalna początkowa wartość życia.</param>
	public void Initialize(float maxHealth, float? currentHealth = null)
	{
		MaxHealth = maxHealth;
		CurrentHealth = currentHealth ?? maxHealth;
		IsInvulnerable = false;
		_invulnerabilityTimer = 0.0f;

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	/// <summary>
	/// Zadaje obrażenia encji. Jeśli encja jest martwa lub nietykalna, obrażenia są ignorowane.
	/// </summary>
	/// <param name="amount">Wartość obrażeń do odjęcia.</param>
	/// <returns>True jeśli obrażenia zostały pomyślnie zadane, w przeciwnym razie false.</returns>
	public bool Damage(float amount)
	{
		if (IsDead || IsInvulnerable || amount <= 0.0f)
		{
			return false;
		}

		CurrentHealth = Mathf.Max(0.0f, CurrentHealth - amount);

		EmitSignal(SignalName.Damaged, amount);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

		// Aktywacja nietykalności, jeśli jest skonfigurowana
		if (InvulnerabilityDuration > 0.0f && CurrentHealth > 0.0f)
		{
			IsInvulnerable = true;
			_invulnerabilityTimer = InvulnerabilityDuration;
			EmitSignal(SignalName.InvulnerabilityStarted);
		}

		// Sprawdzenie stanu śmierci
		if (CurrentHealth <= 0.0f)
		{
			EmitSignal(SignalName.Died);
		}

		return true;
	}

	/// <summary>
	/// Przywraca określoną liczbę punktów życia (nie przekraczając MaxHealth).
	/// </summary>
	/// <param name="amount">Liczba punktów do uleczenia.</param>
	public void Heal(float amount)
	{
		if (IsDead || amount <= 0.0f)
		{
			return;
		}

		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

		EmitSignal(SignalName.Healed, amount);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}
}
