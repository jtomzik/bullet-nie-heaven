using Godot;
using System;
using BulletNieHeaven.Entities;

namespace RogueGame.UI;

/// <summary>
/// Kontroler interfejsu użytkownika (HUD) podczas rozgrywki.
/// Wyświetla pasek zdrowia gracza, licznik czasu oraz reaguje na zdarzenia z GameEvents.
/// </summary>
public partial class GameUI : CanvasLayer
{
	[ExportGroup("UI References")]
	[Export] private TextureProgressBar _healthBar;
	[Export] private Label _healthLabel;
	[Export] private Label _timerLabel;

	[ExportGroup("Settings")]
	[Export] private float _maxHealth = 100f;
	[Export] private float _currentHealth = 100f;

	private float _elapsedTime = 0f;

	public override void _Ready()
	{
		_healthBar ??= GetNodeOrNull<TextureProgressBar>("HUD/MarginContainer/TopBarContainer/HealthContainer/HealthBar");
		_healthLabel ??= GetNodeOrNull<Label>("HUD/MarginContainer/TopBarContainer/HealthContainer/HealthLabel");
		_timerLabel ??= GetNodeOrNull<Label>("HUD/MarginContainer/TopBarContainer/TimerContainer/TimerLabel");

		// Inicjalizacja paska zdrowia i tekstu
		if (_healthBar != null)
		{
			_healthBar.MaxValue = _maxHealth;
			_healthBar.Value = _currentHealth;
		}
		UpdateHealthText();

		// Subskrypcja na globalne zdarzenie zmiany punktów życia gracza
		GameEvents.OnPlayerHealthChanged += OnPlayerHealthChanged;
	}

	public override void _ExitTree()
	{
		// Odpięcie subskrypcji przy usuwaniu węzła, zapobiega wyciekom pamięci
		GameEvents.OnPlayerHealthChanged -= OnPlayerHealthChanged;
	}

	private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
	{
		_maxHealth = maxHealth;
		UpdateHealth(currentHealth);
	}

	public override void _Process(double delta)
	{
		// Aktualizacja timera
		_elapsedTime += (float)delta;
		UpdateTimerDisplay(_elapsedTime);
	}

	private void UpdateTimerDisplay(float totalSeconds)
	{
		if (_timerLabel == null) return;

		int minutes = Mathf.FloorToInt(totalSeconds / 60f);
		int seconds = Mathf.FloorToInt(totalSeconds % 60f);

		_timerLabel.Text = $"Czas: {minutes:D2}:{seconds:D2}";
	}

	public void UpdateHealth(float newHealth)
	{
		_currentHealth = Mathf.Clamp(newHealth, 0f, _maxHealth);
		if (_healthBar != null)
		{
			_healthBar.MaxValue = _maxHealth;
			_healthBar.Value = _currentHealth;
		}
		UpdateHealthText();
	}

	private void UpdateHealthText()
	{
		if (_healthLabel != null)
		{
			_healthLabel.Text = $"HP: {Mathf.RoundToInt(_currentHealth)}/{Mathf.RoundToInt(_maxHealth)}";
		}
	}
}
