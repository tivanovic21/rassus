using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class RegisterSensorDto
    {
        [Required(ErrorMessage = "Latitude je obavezan")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude je obavezan")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "IP adresa je obavezna")]
        public string Ip { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Port je obavezan")]
        public int Port { get; set; }
    }
}