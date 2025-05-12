using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class AppointmentResponseDTO
{
    [Required]
    public DateTime date { get; set; }
    [Required]
    public PatientDTO patient { get; set; }
    [Required]
    public DoctorDTO doctor { get; set; }
    [Required]
    public List<AppointmentServiceDTO> appointmentServices { get; set; }
}

public class PatientDTO
{
    [Required]
    public string firstName { get; set; }
    [Required]
    public string lastName { get; set; }
    [Required]
    public DateTime dateOfBirth { get; set; }
}

public class DoctorDTO
{
    [Required]
    public int doctorId { get; set; }
    [Required]
    public string pwz { get; set; }
}

public class AppointmentServiceDTO
{
    [Required]
    public string name { get; set; }
    
    [Required]
    public decimal serviceFree { get; set; }
}