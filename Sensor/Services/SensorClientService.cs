using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Sensor.Clients;
using Sensor.Models;

namespace Sensor.Services
{
    public class SensorClientService : BackgroundService
    {
        private readonly ILogger<SensorClientService> _logger;
        private readonly RestApiClient _restClient;
        private readonly IConfiguration _configuration;
        private readonly IServer _server;
        
        private SensorInfo? _sensorInfo;
        private SensorInfo? _nearestNeighbor;
        private ReadingData _currentReading;
        private List<string[]> _csvData;
        private int _activeSeconds = 0;
        private readonly Random _random = new();

        public SensorClientService(
            ILogger<SensorClientService> logger,
            RestApiClient restClient,
            IConfiguration configuration,
            IServer server)
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            _server = server;
            _currentReading = new ReadingData();
            _csvData = [];
        }

        public ReadingData GetCurrentReading() => _currentReading;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Pokrećem senzor...");

                await InitializeAsync();
                
                LoadCsvData();

                _logger.LogInformation("Senzor inicijaliziran...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await RunSensorCycleAsync();
                        
                        _activeSeconds++;
                        await Task.Delay(1000, stoppingToken); // spava 1 sec
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Greška u ciklusu");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pokretanju senzora");
            }
        }

        private async Task RunSensorCycleAsync()
        {
            var ownReading = GenerateReadingFromCsv();
            _currentReading = ownReading; 
            
            _logger.LogInformation("[Iteracija {Seconds}] Generirano: Temp={Temp}°C, Press={Press}hPa, Hum={Hum}%, CO={CO}, NO2={NO2}, SO2={SO2}",
                _activeSeconds, ownReading.Temperature, ownReading.Pressure, ownReading.Humidity,
                ownReading.CO, ownReading.NO2, ownReading.SO2);

            ReadingData? neighborReading = null;
            if (_nearestNeighbor != null)
            {
                neighborReading = await GetNeighborReadingViaGrpcAsync();
            }
            else
            {
                if (_sensorInfo != null)
                {
                    _nearestNeighbor = await _restClient.GetNearestNeighborAsync(_sensorInfo.Id);
                }
            }

            var calibratedReading = CalibrateReading(ownReading, neighborReading);

            _logger.LogInformation("[Iteracija {Seconds}] Kalibrirano: Temp={Temp}°C, Press={Press}hPa, Hum={Hum}%, CO={CO}, NO2={NO2}, SO2={SO2}",
                _activeSeconds, calibratedReading.Temperature, calibratedReading.Pressure, calibratedReading.Humidity,
                calibratedReading.CO, calibratedReading.NO2, calibratedReading.SO2);

            if (_sensorInfo != null)
            {
                await _restClient.StoreReadingAsync(_sensorInfo.Id, calibratedReading);
            }
        }

        private async Task InitializeAsync()
        {
            var longitude = 15.87 + _random.NextDouble() * (16.0 - 15.87);
            var latitude = 45.75 + _random.NextDouble() * (45.85 - 45.75);
            
            int port = _configuration.GetValue<int>("SensorPort");

            var sensor = new SensorInfo
            {
                Longitude = longitude,
                Latitude = latitude,
                Ip = "127.0.0.1",
                Port = port
            };

            _logger.LogInformation("Registriram senzor s lokacijskim podacima ({Lat}, {Lon}) na portu {Port}", 
                latitude, longitude, port);

            _sensorInfo = await _restClient.RegisterSensorAsync(sensor);
            
            if (_sensorInfo == null)
            {
                _logger.LogError("Neuspješna registracija senzora");
                throw new Exception("Neuspješna registracija senzora");
            }

            _logger.LogInformation("Uspješno registriran senzor s ID: {Id}", _sensorInfo.Id);

            _nearestNeighbor = await _restClient.GetNearestNeighborAsync(_sensorInfo.Id);
            
            if (_nearestNeighbor != null)
            {
                _logger.LogInformation("Pronađen najbliži susjed: Sensor {Id} at {Ip}:{Port}", 
                    _nearestNeighbor.Id, _nearestNeighbor.Ip, _nearestNeighbor.Port);
            }
            else
            {
                _logger.LogInformation("Nema najbližeg susjeda, koristi se samo vlastito očitanje");
            }
        }

        private void LoadCsvData()
        {
            try
            {
                var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readings.csv");
                
                if (!File.Exists(csvPath))
                {
                    _logger.LogError("CSV file nije pronađen na: {Path}", csvPath);
                    throw new FileNotFoundException($"readings.csv nije pronađen na {csvPath}");
                }

                var lines = File.ReadAllLines(csvPath);
                
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    _csvData.Add(values);
                }

                _logger.LogInformation("Učitano {Count} očitanja iz CSV datoteke", _csvData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju CSV podataka");
                throw;
            }
        }

        private ReadingData GenerateReadingFromCsv()
        {
            int row = _activeSeconds % 100;
            
            if (row >= _csvData.Count)
                row = _csvData.Count - 1;

            var data = _csvData[row];

            return new ReadingData
            {
                Temperature = ParseDouble(data[0]),
                Pressure = ParseDouble(data[1]),
                Humidity = ParseDouble(data[2]),
                CO = ParseDouble(data[3]),
                NO2 = ParseDouble(data[4]),
                SO2 = ParseDouble(data[5])
            };
        }

        private async Task<ReadingData?> GetNeighborReadingViaGrpcAsync()
        {
            if (_nearestNeighbor == null) return null;

            try
            {
                var address = $"http://{_nearestNeighbor.Ip}:{_nearestNeighbor.Port}";
                
                using var channel = GrpcChannel.ForAddress(address);
                var client = new SensorService.SensorServiceClient(channel);

                var request = new ReadingRequest { SensorId = _sensorInfo?.Id ?? 0 };
                var response = await client.GetReadingAsync(request, deadline: DateTime.UtcNow.AddSeconds(2));

                _logger.LogInformation("Primljeno od susjeda: Temp={Temp}°C, Press={Press}hPa", 
                    response.Temperature, response.Pressure);

                return new ReadingData
                {
                    Temperature = response.Temperature,
                    Pressure = response.Pressure,
                    Humidity = response.Humidity,
                    CO = response.Co,
                    NO2 = response.No2,
                    SO2 = response.So2
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Greška pri dohvaćanju očitanja od susjeda");
                return null;
            }
        }

        private ReadingData CalibrateReading(ReadingData own, ReadingData? neighbor)
        {
            if (neighbor == null)
            {
                _logger.LogInformation("Nema susjednog očitanja. Koristim vlastito očitanje");
                return own;
            }

            return new ReadingData
            {
                Temperature = Average(own.Temperature, neighbor.Temperature),
                Pressure = Average(own.Pressure, neighbor.Pressure),
                Humidity = Average(own.Humidity, neighbor.Humidity),
                CO = Average(own.CO, neighbor.CO),
                NO2 = Average(own.NO2, neighbor.NO2),
                SO2 = Average(own.SO2, neighbor.SO2)
            };
        }

        private static double? Average(double? value1, double? value2)
        {
            if ((!value1.HasValue || value1 == 0) && (!value2.HasValue || value2 == 0))
                return null;
            
            if (!value1.HasValue || value1 == 0) return value2;
            if (!value2.HasValue || value2 == 0) return value1;
            
            return (value1.Value + value2.Value) / 2.0;
        }

        private static double ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (double.TryParse(value, out double result))
                return result;
            return 0;
        }
    }
}
