namespace Server.DTOs
{
    public class RegisterSensorDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Ip { get; set; } = string.Empty;
        public int? Port { get; set; }
    }
}