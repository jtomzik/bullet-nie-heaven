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
		// Odnalezienie węzłów UI w obrębie VBoxContainer
		_loginInput = GetNode<LineEdit>("VBoxContainer/LoginInput");
		_passwordInput = GetNode<LineEdit>("VBoxContainer/PasswordInput");
		_statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
		
		// Pobranie Singletona
		_apiClient = GetNode<ApiClient>("/root/ApiClient");

		// Przypisanie zdarzeń kliknięcia
		GetNode<Button>("VBoxContainer/RegisterButton").Pressed += OnRegisterPressed;
		GetNode<Button>("VBoxContainer/LoginButton").Pressed += OnLoginPressed;
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
