using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class AppointmentResponseDTO
{
    public DateTime date { get; set; }
    public PatientDTO patient { get; set; }
    public DoctorDTO doctor { get; set; }
    public List<AppointmentServiceDTO> appointmentServices { get; set; }
}

public class PatientDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

public class DoctorDTO
{
    public int doctorId { get; set; }
    public string pwz { get; set; }
}

public class AppointmentServiceDTO
{
    public string name { get; set; }
    public decimal serviceFree { get; set; }
}