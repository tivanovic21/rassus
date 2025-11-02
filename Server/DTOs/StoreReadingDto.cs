namespace Server.DTOs
{
    public class StoreReadingDto
    {
        public double? Temperature { get; set; }
        public double? Pressure { get; set; }
        public double? Humidity { get; set; }
        public double? CO { get; set; }
        public double? NO2 { get; set; }
        public double? SO2 { get; set; }

        public int SensorId { get; set; }
    }
}