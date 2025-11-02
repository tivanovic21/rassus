using System.Text;
using System.Text.Json;
using Sensor.Models;

namespace Sensor.Clients
{
    public class RestApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestApiClient> _logger;
        private readonly string _serverUrl;

        private const string RegisterEndpoint = "api/sensor/RegisterSensor";
        private const string NearestNeighborEndpoint = "api/sensor/GetNearestSensor";
        private const string StoreReadingEndpoint = "api/reading/StoreReading";

        public RestApiClient(HttpClient httpClient, ILogger<RestApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serverUrl = configuration["ServerUrl"] ??
                throw new ArgumentNullException("ServerUrl nije pronađen");
        }

        public async Task<SensorInfo?> RegisterSensorAsync(SensorInfo sensor)
        {
            try
            {
                var json = JsonSerializer.Serialize(sensor);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_serverUrl}/{RegisterEndpoint}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var registered = JsonSerializer.Deserialize<SensorInfo>(responseJson, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation("Senzor registriran uspješno s ID: {Id}", registered?.Id);
                    return registered;
                }
                else
                {
                    _logger.LogError("Greška pri registraciji senzora. Status: {Status}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri registraciji senzora");
                return null;
            }
        }

        public async Task<SensorInfo?> GetNearestNeighborAsync(int sensorId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}/{NearestNeighborEndpoint}/{sensorId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var neighbor = JsonSerializer.Deserialize<SensorInfo>(json, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    _logger.LogInformation("Najbliži susjed: {Ip}:{Port}", neighbor?.Ip, neighbor?.Port);
                    return neighbor;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Nema najbližeg susjeda");
                    return null;
                }
                else
                {
                    _logger.LogError("Greška pri dohvaćanju najbližeg susjeda. Status: {Status}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvaćanju najbližeg susjeda");
                return null;
            }
        }

        public async Task<bool> StoreReadingAsync(int sensorId, ReadingData reading)
        {
            try
            {
                var json = JsonSerializer.Serialize(reading);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_serverUrl}/{StoreReadingEndpoint}/{sensorId}", content);
                
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation("Očitanje uspješno spremljeno");
                    return true;
                }
                else
                {
                    _logger.LogError("Greška pri spremanju očitanja. Status: {Status}", response.StatusCode);
                    _logger.LogError("Response: {Response}", await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri spremanju očitanja: {Message}", ex.Message);
                return false;
            }
        }
    }
}