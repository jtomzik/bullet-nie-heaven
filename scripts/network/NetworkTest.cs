using Godot;
using System.Threading.Tasks;

public partial class NetworkTest : Control
{
	private LineEdit _loginInput;
	private LineEdit _passwordInput;
	private Label _statusLabel;
	private ApiClient _apiClient;

	public override void _Ready()
	{
		_loginInput = GetNode<LineEdit>("VBoxContainer/LoginInput");
		_passwordInput = GetNode<LineEdit>("VBoxContainer/PasswordInput");
		_statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
		
		_apiClient = GetNode<ApiClient>("/root/ApiClient");

		GetNode<Button>("VBoxContainer/RegisterButton").Pressed += OnRegisterPressed;
		GetNode<Button>("VBoxContainer/LoginButton").Pressed += OnLoginPressed;
		
		// Nowe przyciski do obsługi wyników
		GetNode<Button>("VBoxContainer/SubmitScoreButton").Pressed += OnSubmitScorePressed;
		GetNode<Button>("VBoxContainer/GetScoresButton").Pressed += OnGetScoresPressed;
	}

	private async void OnSubmitScorePressed()
	{
		_statusLabel.Text = "Status: Wysyłanie wyniku...";
		// Symulujemy wynik: 120 sekund przetrwania, 45 pokonanych wrogów
		bool success = await _apiClient.SubmitScore(120, 45); 
		_statusLabel.Text = success ? "Status: Wynik zapisany na serwerze!" : "Status: Błąd zapisu wyniku (niezalogowany?).";
	}

	private async void OnGetScoresPressed()
	{
		_statusLabel.Text = "Status: Pobieranie wyników...";
		string jsonScores = await _apiClient.GetTop10Scores();
		_statusLabel.Text = $"Status: Pobrano ranking.\n{jsonScores}";
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
