using Godot;
using BulletNieHeaven.Entities;
using RogueGame.UI;

namespace BulletNieHeaven.Tests;

/// <summary>
/// Kontroler sceny testowej walki (tests/test_combat.tscn).
/// Odpowiada za integrację szyny zdarzeń GameEvents z instancją HUD w środowisku testowym,
/// bez konieczności modyfikowania oryginalnych plików UI.
/// </summary>
public partial class TestCombat : Node2D
{
	private GameUI _gameUI;
	private ProgressBar _healthBar;
	private Label _healthLabel;

	public override void _Ready()
	{
		_gameUI = GetNodeOrNull<GameUI>("GameUI");

		if (_gameUI != null)
		{
			_healthBar = _gameUI.FindChild("HealthBar", true, false) as ProgressBar;
			_healthLabel = _gameUI.FindChild("HealthLabel", true, false) as Label;

			// Usunięcie jednolitego czerwonego modulate w scenie testowej, aby pasek był czytelny
			if (_healthBar != null)
			{
				_healthBar.Modulate = Colors.White;
			}
		}

		// Podpięcie szyny zdarzeń mechaniki do widoku testowego
		GameEvents.OnPlayerHealthChanged += OnPlayerHealthChanged;
	}

	public override void _ExitTree()
	{
		GameEvents.OnPlayerHealthChanged -= OnPlayerHealthChanged;
	}

	/// <summary>
	/// Aktualizuje widok HUD w teście po otrzymaniu obrażeń przez gracza.
	/// </summary>
	private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
	{
		if (_gameUI != null)
		{
			// Wywołanie publicznej metody z UI Kirila
			_gameUI.UpdateHealth(currentHealth);
		}

		if (_healthLabel != null)
		{
			_healthLabel.Text = $"HP: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
		}

		GD.Print($"[TestCombat] Zdarzenie HP -> HUD zaktualizowany na: {currentHealth}/{maxHealth}");
	}
}
