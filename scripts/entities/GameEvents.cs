using System;
using Godot;

namespace BulletNieHeaven.Entities;

/// <summary>
/// Statyczna szyna zdarzeń (Event Bus) dla gry.
/// Odpowiada za luźne powiązanie (decoupling) pomiędzy mechanikami rozgrywki (obrażenia, śmierć)
/// a elementami interfejsu użytkownika (HUD, ekran Game Over) czy systemem statystyk.
/// </summary>
public static class GameEvents
{
	/// <summary>
	/// Zdarzenie wywoływane w momencie otrzymania obrażeń przez gracza.
	/// Parametry: aktualne punkty życia (currentHealth), maksymalne punkty życia (maxHealth).
	/// </summary>
	public static event Action<float, float> OnPlayerDamaged;

	/// <summary>
	/// Zdarzenie wywoływane przy jakiejkolwiek zmianie poziomu zdrowia gracza (obrażenia, leczenie).
	/// Parametry: aktualne punkty życia (currentHealth), maksymalne punkty życia (maxHealth).
	/// </summary>
	public static event Action<float, float> OnPlayerHealthChanged;

	/// <summary>
	/// Zdarzenie wywoływane w momencie śmierci gracza (wyzerowanie punktów życia).
	/// </summary>
	public static event Action OnPlayerDied;

	/// <summary>
	/// Zdarzenie wywoływane w momencie śmierci przeciwnika.
	/// Parametr: pozycja na scenie (Vector2), w której przeciwnik został pokonany.
	/// </summary>
	public static event Action<Vector2> OnEnemyDied;

	/// <summary>
	/// Emituje zdarzenie otrzymania obrażeń przez gracza.
	/// </summary>
	public static void EmitPlayerDamaged(float currentHealth, float maxHealth)
	{
		OnPlayerDamaged?.Invoke(currentHealth, maxHealth);
		OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
	}

	/// <summary>
	/// Emituje zdarzenie zmiany punktów życia gracza (np. leczenie).
	/// </summary>
	public static void EmitPlayerHealthChanged(float currentHealth, float maxHealth)
	{
		OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
	}

	/// <summary>
	/// Emituje zdarzenie śmierci gracza.
	/// </summary>
	public static void EmitPlayerDied()
	{
		OnPlayerDied?.Invoke();
	}

	/// <summary>
	/// Emituje zdarzenie śmierci przeciwnika (np. pod kątem wypadania kryształów XP).
	/// </summary>
	public static void EmitEnemyDied(Vector2 deathPosition)
	{
		OnEnemyDied?.Invoke(deathPosition);
	}
}
