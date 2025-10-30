using Grpc.Core;

namespace Sensor.Services
{
    public class SensorGrpcService : SensorService.SensorServiceBase
    {
        private readonly ILogger<SensorGrpcService> _logger;
        private readonly SensorClientService _clientService;

        public SensorGrpcService(ILogger<SensorGrpcService> logger, SensorClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;
        }

        public override Task<ReadingResponse> GetReading(ReadingRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Dobio gRPC zahtjev za očitanje od senzora {SensorId}", request.SensorId);

            var reading = _clientService.GetCurrentReading();

            var response = new ReadingResponse
            {
                Temperature = reading.Temperature.GetValueOrDefault(),
                Pressure = reading.Pressure.GetValueOrDefault(),
                Humidity = reading.Humidity.GetValueOrDefault(),
                Co = reading.CO.GetValueOrDefault(),
                No2 = reading.NO2.GetValueOrDefault(),
                So2 = reading.SO2.GetValueOrDefault()
            };

            _logger.LogInformation("Odgovaram s: Temp={Temp}°C, Pressure={Pressure}hPa, Humidity={Humidity}%, CO={CO}, NO2={NO2}, SO2={SO2}", 
                reading.Temperature, reading.Pressure, reading.Humidity, reading.CO, reading.NO2, reading.SO2);

            return Task.FromResult(response);
        }
    }
}
