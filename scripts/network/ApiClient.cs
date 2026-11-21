using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using HttpClient = System.Net.Http.HttpClient;

// Model pojedynczego rekordu rankingu (gotowy do użycia przez skrypty UI)
public class ScoreEntry
{
	public string Username { get; set; } = string.Empty;
	public int SurvivalTimeInSeconds { get; set; }
	public int EnemiesKilled { get; set; }
}

public partial class ApiClient : Node
{
	private static readonly HttpClient client = new HttpClient(new HttpClientHandler
	{
		ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
	});

	// Opcje deserializacji ignorujące wielkość liter (ASP.NET zwraca camelCase, C# używa PascalCase)
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	// Jeden wspólny adres bazowy zgodny z run_server.bat
	private const string BaseUrl = "http://localhost:5157/api";
	
	private const string LocalScoresPath = "user://local_scores.json";

	public static bool IsServerOnline { get; private set; } = false;
	public static string SessionToken { get; private set; } = string.Empty;
	public static int CurrentUserId { get; private set; } = 0;
	public static string CurrentUsername { get; private set; } = string.Empty;

	public static bool IsLoggedIn => CurrentUserId > 0 && !string.IsNullOrEmpty(SessionToken);

	// Szybkie sprawdzenie statusu serwera z limitem czasu (domyślnie 3 sekundy)
	public async Task<bool> Ping(int timeoutSeconds = 3)
	{
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
		try
		{
			HttpResponseMessage response = await client.GetAsync($"{BaseUrl}/health/ping", cts.Token);
			IsServerOnline = response.IsSuccessStatusCode;
			return IsServerOnline;
		}
		catch (Exception)
		{
			IsServerOnline = false;
			return false;
		}
	}

	public async Task<bool> Register(string username, string password)
	{
		var requestData = new { Username = username, Password = password };
		string json = JsonSerializer.Serialize(requestData);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		try
		{
			HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/auth/register", content);
			if (response.IsSuccessStatusCode)
			{
				GD.Print("Rejestracja udana!");
				return true;
			}

			string error = await response.Content.ReadAsStringAsync();
			GD.PrintErr($"Błąd rejestracji: {error}");
			return false;
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd połączenia (Register): {e.Message}");
			IsServerOnline = false;
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
			HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/auth/login", content);
			if (response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				using JsonDocument doc = JsonDocument.Parse(responseBody);
				JsonElement root = doc.RootElement;

				SessionToken = root.GetProperty("token").GetString() ?? string.Empty;
				CurrentUserId = root.GetProperty("userId").GetInt32();
				CurrentUsername = username;
				IsServerOnline = true;

				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionToken);

				GD.Print($"Zalogowano jako {CurrentUsername} (ID: {CurrentUserId})");
				return true;
			}

			GD.PrintErr("Błędny login lub hasło.");
			return false;
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd połączenia (Login): {e.Message}");
			IsServerOnline = false;
			return false;
		}
	}

	public void Logout()
	{
		SessionToken = string.Empty;
		CurrentUserId = 0;
		CurrentUsername = string.Empty;
		client.DefaultRequestHeaders.Authorization = null;
		GD.Print("Wylogowano.");
	}

	public async Task<bool> SubmitScore(int survivalTime, int enemiesKilled)
	{
		// Zawsze zapisujemy wynik lokalnie jako kopię zapasową i rekord offline
		SaveLocalScore(survivalTime, enemiesKilled);

		if (!IsLoggedIn)
		{
			GD.Print("Tryb offline / brak logowania: wynik zapisany wyłącznie lokalnie.");
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
			HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/score", content);
			if (response.IsSuccessStatusCode)
			{
				GD.Print("Wynik pomyślnie wysłany na serwer oraz zapisany lokalnie!");
				return true;
			}

			GD.PrintErr("Błąd serwera przy zapisie wyniku (zapisano lokalnie).");
			return false;
		}
		catch (Exception e)
		{
			GD.PrintErr($"Brak połączenia (SubmitScore): {e.Message}. Wynik zapisano lokalnie.");
			IsServerOnline = false;
			return false;
		}
	}

	// Pobiera TOP 10 z serwera; w razie braku połączenia automatycznie zwraca lokalne TOP 10
	public async Task<List<ScoreEntry>> GetTop10Scores()
	{
		try
		{
			HttpResponseMessage response = await client.GetAsync($"{BaseUrl}/score/top10");
			if (response.IsSuccessStatusCode)
			{
				IsServerOnline = true;
				string jsonResponse = await response.Content.ReadAsStringAsync();
				var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(jsonResponse, JsonOptions);
				return scores ?? new List<ScoreEntry>();
			}

			GD.PrintErr("Błąd pobierania rankingu online. Przełączanie na wyniki lokalne.");
			return GetLocalTop10Scores();
		}
		catch (Exception e)
		{
			GD.PrintErr($"Brak połączenia (GetTop10Scores): {e.Message}. Ładowanie wyników lokalnych.");
			IsServerOnline = false;
			return GetLocalTop10Scores();
		}
	}

	// --- TRYB OFFLINE (LOKALNY PLIK JSON) ---

	public void SaveLocalScore(int survivalTime, int enemiesKilled)
	{
		var scores = GetLocalTop10Scores();
		scores.Add(new ScoreEntry
		{
			Username = IsLoggedIn ? CurrentUsername : "Gracz (Offline)",
			SurvivalTimeInSeconds = survivalTime,
			EnemiesKilled = enemiesKilled
		});

		// Sortowanie identyczne jak na serwerze: czas przetrwania, potem pokonani wrogowie
		var top10 = scores
			.OrderByDescending(s => s.SurvivalTimeInSeconds)
			.ThenByDescending(s => s.EnemiesKilled)
			.Take(10)
			.ToList();

		try
		{
			string json = JsonSerializer.Serialize(top10, new JsonSerializerOptions { WriteIndented = true });
			using var file = FileAccess.Open(LocalScoresPath, FileAccess.ModeFlags.Write);
			file.StoreString(json);
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd zapisu lokalnego pliku wyników: {e.Message}");
		}
	}

	public List<ScoreEntry> GetLocalTop10Scores()
	{
		if (!FileAccess.FileExists(LocalScoresPath))
		{
			return new List<ScoreEntry>();
		}

		try
		{
			using var file = FileAccess.Open(LocalScoresPath, FileAccess.ModeFlags.Read);
			string json = file.GetAsText();
			var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(json, JsonOptions);
			return scores ?? new List<ScoreEntry>();
		}
		catch (Exception e)
		{
			GD.PrintErr($"Błąd odczytu lokalnego pliku wyników: {e.Message}");
			return new List<ScoreEntry>();
		}
	}

	public ScoreEntry GetLocalBestScore()
	{
		return GetLocalTop10Scores().FirstOrDefault();
	}
}
