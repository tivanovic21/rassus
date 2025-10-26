using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class StoreReadingDto
    {
        [Required(ErrorMessage = "Temperatura je obavezna")]
        public double Temperature { get; set; }

        [Required(ErrorMessage = "Pritisak je obavezan")]
        public double Pressure { get; set; }

        [Required(ErrorMessage = "Vlažnost je obavezna")]
        public double Humidity { get; set; }

        [Required(ErrorMessage = "CO je obavezan")]
        public double CO { get; set; }

        [Required(ErrorMessage = "SO2 je obavezan")]
        public double SO2 { get; set; }

        public int SensorId { get; set; }
    }
}