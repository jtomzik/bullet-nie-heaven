using HttpClient = System.Net.Http.HttpClient;
using Godot;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class ApiClient : Node
{
	// Konfiguracja ignorująca błędy lokalnego certyfikatu SSL na czas testów w Visual Studio
	private static readonly HttpClient client = new HttpClient(new HttpClientHandler 
	{
		ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
	});
	
	// Upewnij się, że port (tutaj 7274) zgadza się z tym, na którym uruchamia się Twój serwer w VS
	private const string BaseUrl = "https://localhost:7274/api/auth"; 
	
	public static string SessionToken { get; private set; } = string.Empty;
	public static int CurrentUserId { get; private set; } = 0;

	public async Task<bool> Register(string username, string password)
	{
		var requestData = new { Username = username, Password = password };
		string json = JsonSerializer.Serialize(requestData);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/register", content);
			if (response.IsSuccessStatusCode)
			{
				GD.Print("Rejestracja udana!");
				return true;
			}
			else
			{
				string error = await response.Content.ReadAsStringAsync();
				GD.PrintErr($"Błąd rejestracji: {error}");
				return false;
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd połączenia: {e.Message}");
			return false;
		}
	}

	public async Task<bool> Login(string username, string password)
	{
		var requestData = new { Username = username, Password = password };
		string json = JsonSerializer.Serialize(requestData);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/login", content);
			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				
				using JsonDocument doc = JsonDocument.Parse(responseBody);
				JsonElement root = doc.RootElement;
				
				SessionToken = root.GetProperty("token").GetString();
				CurrentUserId = root.GetProperty("userId").GetInt32();
				
				// Automatyczne dodanie tokenu do nagłówków dla przyszłych zapytań (np. wysyłanie wyniku)
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SessionToken);

				GD.Print($"Zalogowano udanie! Przypisano token sesji dla ID: {CurrentUserId}");
				return true;
			}
			
			GD.PrintErr("Błędny login lub hasło.");
			return false;
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd połączenia: {e.Message}");
			return false;
		}
	}
	
	// Metoda wysyłająca wynik zalogowanego gracza na serwer
	public async Task<bool> SubmitScore(int survivalTime, int enemiesKilled)
	{
		if (CurrentUserId == 0)
		{
			GD.PrintErr("Błąd: Gracz nie jest zalogowany.");
			return false;
		}

		var requestData = new 
		{ 
			UserId = CurrentUserId, 
			SurvivalTimeInSeconds = survivalTime, 
			EnemiesKilled = enemiesKilled 
		};
		
		string json = JsonSerializer.Serialize(requestData);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			// Zwróć uwagę na prawidłowy adres endpointa wyników
			HttpResponseMessage response = await client.PostAsync("https://localhost:7274/api/score", content);
			if (response.IsSuccessStatusCode)
			{
				GD.Print("Wynik pomyślnie wysłany na serwer!");
				return true;
			}
			
			GD.PrintErr("Błąd podczas wysyłania wyniku.");
			return false;
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd połączenia: {e.Message}");
			return false;
		}
	}

	// Metoda pobierająca TOP 10 wyników
	public async Task<string> GetTop10Scores()
	{
		try
		{
			HttpResponseMessage response = await client.GetAsync("https://localhost:7274/api/score/top10");
			if (response.IsSuccessStatusCode)
			{
				// Zwraca surowy JSON, który później można sparsować do wyświetlenia w tabeli UI
				string jsonResponse = await response.Content.ReadAsStringAsync();
				return jsonResponse;
			}
			return "Błąd pobierania rankingu.";
		}
		catch (Exception e)
		{
			return $"Błąd połączenia: {e.Message}";
		}
	}
}
