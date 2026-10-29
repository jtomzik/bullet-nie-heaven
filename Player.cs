using Godot;

namespace BulletNieHeaven;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 300.0f;

	public override void _PhysicsProcess(double delta)
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
}
