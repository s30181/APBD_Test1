using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class AppointmentRequestDTO
{
    [Required]
    public int appointmentId { get; set; }
    
    [Required]
    public int patientId { get; set; }
    
    [Required]
    public string pwz { get; set; }
    
    [Required]
    public List<AppointmentServiceRequestDTO> services { get; set; }
}

public class AppointmentServiceRequestDTO
{
    [Required]
    public string serviceName { get; set; }
    
    [Required]
    public decimal serviceFee { get; set; }
}