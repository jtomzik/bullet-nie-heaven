using Godot;
using System.Threading.Tasks;

public partial class NetworkTest : Control
{
	private LineEdit _loginInput;
	private LineEdit _passwordInput;
	private Label _statusLabel;
	private ApiClient _apiClient;

	public override async void _Ready()
	{
		_loginInput = GetNode<LineEdit>("VBoxContainer/LoginInput");
		_passwordInput = GetNode<LineEdit>("VBoxContainer/PasswordInput");
		_statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");

		_apiClient = GetNode<ApiClient>("/root/ApiClient");

		GetNode<Button>("VBoxContainer/RegisterButton").Pressed += OnRegisterPressed;
		GetNode<Button>("VBoxContainer/LoginButton").Pressed += OnLoginPressed;
		GetNode<Button>("VBoxContainer/SubmitScoreButton").Pressed += OnSubmitScorePressed;
		GetNode<Button>("VBoxContainer/GetScoresButton").Pressed += OnGetScoresPressed;

		// Automatyczny test połączenia przy starcie sceny
		_statusLabel.Text = "Status: Łączenie z serwerem...";
		bool online = await _apiClient.Ping();
		_statusLabel.Text = online ? "Status: Serwer ONLINE" : "Status: Serwer OFFLINE";
	}

	private async void OnSubmitScorePressed()
	{
		_statusLabel.Text = "Status: Wysyłanie wyniku...";
		bool sentOnline = await _apiClient.SubmitScore(120, 45);
		_statusLabel.Text = sentOnline
			? "Status: Wynik zapisany ONLINE (i lokalnie)!"
			: "Status: Tryb OFFLINE - wynik zapisany lokalnie.";
	}

	private async void OnGetScoresPressed()
	{
		_statusLabel.Text = "Status: Pobieranie wyników...";
		var scores = await _apiClient.GetTop10Scores();

		if (scores.Count == 0)
		{
			_statusLabel.Text = "Status: Brak wyników.";
			return;
		}

		string header = ApiClient.IsServerOnline ? "TOP 10 (ONLINE):\n" : "TOP 10 (OFFLINE - LOKALNE):\n";
		for (int i = 0; i < scores.Count; i++)
		{
			header += $"{i + 1}. {scores[i].Username} - {scores[i].SurvivalTimeInSeconds}s ({scores[i].EnemiesKilled} zabitych)\n";
		}
		_statusLabel.Text = header;
	}

	private async void OnRegisterPressed()
	{
		if (string.IsNullOrWhiteSpace(_loginInput.Text) || string.IsNullOrWhiteSpace(_passwordInput.Text))
		{
			_statusLabel.Text = "Status: Pola nie mogą być puste!";
			return;
		}

		_statusLabel.Text = "Status: Rejestracja w toku...";
		bool success = await _apiClient.Register(_loginInput.Text, _passwordInput.Text);

		_statusLabel.Text = success ? "Status: Rejestracja pomyślna!" : "Status: Błąd rejestracji.";
	}

	private async void OnLoginPressed()
	{
		if (string.IsNullOrWhiteSpace(_loginInput.Text) || string.IsNullOrWhiteSpace(_passwordInput.Text))
		{
			_statusLabel.Text = "Status: Pola nie mogą być puste!";
			return;
		}

		_statusLabel.Text = "Status: Logowanie w toku...";
		bool success = await _apiClient.Login(_loginInput.Text, _passwordInput.Text);

		_statusLabel.Text = success ? $"Status: Zalogowano jako ID {ApiClient.CurrentUserId}" : "Status: Błędne dane logowania.";
	}
}
