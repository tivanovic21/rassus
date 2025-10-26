namespace Server.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}