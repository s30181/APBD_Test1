namespace Tutorial9.Model;

public class AppointmentRequestDTO
{
    public int appointmentId { get; set; }
    public int patientId { get; set; }
    public string pwz { get; set; }
    public List<AppointmentServiceRequestDTO> services { get; set; }
}

public class AppointmentServiceRequestDTO
{
    public string serviceName { get; set; }
    public decimal serviceFee { get; set; }
}