using Godot;

namespace RogueGame.UI;

public partial class MainMenu : Control
{
	[Export] private Button _startButton;
	[Export] private Button _exitButton;

	public override void _Ready()
	{
		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;

		if (_exitButton != null)
			_exitButton.Pressed += OnExitPressed;
	}

	private void OnStartPressed()
	{
		GD.Print("Start Game Pressed");
		// GetTree().ChangeSceneToFile("res://Scenes/Levels/MainLevel.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
