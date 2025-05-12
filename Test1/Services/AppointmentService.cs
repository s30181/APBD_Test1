using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class AppointmentService
{
    private readonly DbService _dbService;

    public AppointmentService(DbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<AppointmentResponseDTO?> GetAppointment(int id)
    {
        var query = @"SELECT 
                Appointment.appointment_id as AppointmentId,
                [date] as AppointmentDate, 
                Patient.first_name as PatientFirstName, 
                Patient.last_name as PatientLastName,
                Patient.date_of_birth as PatientDateOfBirth,
                Doctor.doctor_id as DoctorId,
                Doctor.PWZ as DoctorPwz,
                Service.name as ServiceName,
                Appointment_Service.service_fee as ServiceFee
            FROM Appointment 
            JOIN Doctor ON Appointment.doctor_id = Doctor.doctor_id 
            JOIN Patient ON Patient.patient_id = Appointment.patient_id 
            JOIN Appointment_Service ON Appointment_Service.appointment_id = Appointment.appointment_id
            JOIN Service ON Service.service_id = Appointment_Service.service_id
            WHERE Appointment.appointment_id = @id";

        var all = await _dbService.FetchList(query, (reader) => Task.FromResult(new
        {
            Id = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
            Date = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
            PatientFirstName = reader.GetString(reader.GetOrdinal("PatientFirstName")),
            PatientLastName = reader.GetString(reader.GetOrdinal("PatientLastName")),
            PatientDateOfBirth = reader.GetDateTime(reader.GetOrdinal("PatientDateOfBirth")),
            DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
            DoctorPwz = reader.GetString(reader.GetOrdinal("DoctorPwz")),
            ServiceName = reader.GetString(reader.GetOrdinal("ServiceName")),
            ServiceFee = reader.GetDecimal(reader.GetOrdinal("ServiceFee")),
        }), new Dictionary<string, object>()
        {
            { "@id", id }
        });

        return all
            .GroupBy(entry => new { Id = entry.Id, Date = entry.Date })
            .Select((grouping) => new AppointmentResponseDTO()
            {
                date = grouping.Key.Date,
                patient = grouping
                    .Select(entry => new PatientDTO()
                    {
                        firstName = entry.PatientFirstName,
                        lastName = entry.PatientLastName,
                        dateOfBirth = entry.PatientDateOfBirth,
                    })
                    .First(),
                doctor = grouping
                    .Select(entry => new DoctorDTO()
                    {
                        doctorId = entry.DoctorId,
                        pwz = entry.DoctorPwz,
                    })
                    .First(),
                appointmentServices = grouping
                    .Select(entry => new AppointmentServiceDTO()
                    {
                        name = entry.ServiceName,
                        serviceFree = entry.ServiceFee
                    })
                    .ToList()
            })
            .ToList()
            .FirstOrDefault();
    }

    public async Task<bool> AppointmentExists(int appointmentId)
    {
        return await _dbService.FetchScalar<int>(
            "SELECT 1 FROM Appointment WHERE appointment_id = @id",
            new Dictionary<string, object>() {  { "@id", appointmentId } }) == 1;
    }

    public async Task<bool> PatientExists(int patientId)
    {
        return await _dbService.FetchScalar<int>(
            "SELECT 1 FROM Patient WHERE patient_id = @id",
            new Dictionary<string, object>() {  { "@id", patientId } }) == 1;
    }

    public async Task<int?> GetDoctorIdWithPwz(string pwz)
    {
        return await _dbService.FetchScalar<int>(
            "SELECT Doctor.doctor_id FROM Doctor WHERE PWZ = @pwz",
            new Dictionary<string, object>() {  { "@pwz", pwz } });
    }

    public async Task<int?> GetServiceIdByNames(string serviceName)
    {
        return await _dbService.FetchScalar<int>(
            "SELECT Service.service_id as ServiceId FROM Service WHERE name = @name", 
            new Dictionary<string, object>()
            {
                { "@name", serviceName }
            });
    }

    public async Task CreateAppointmentService(int appointmentId, int serviceId, decimal serviceFee)
    {
        await _dbService.ExecuteNonQuery(
            "INSERT INTO Appointment_Service (appointment_id, service_id, service_fee) VALUES (@appointmentId, @serviceId, @serviceFee)",
            new Dictionary<string, object>()
            {
                ["@appointmentId"] = appointmentId,
                ["@serviceId"] = serviceId,
                ["@serviceFee"] = serviceFee
            });
    }
    
    public async Task CreateAppointment(AppointmentRequestDTO appointmentRequestDto, int doctorId, List<int> serviceIds)
    {
        await _dbService.ExecuteNonQuery(
            "INSERT INTO Appointment (appointment_id, patient_id, doctor_id, date) VALUES (@appointmentId, @patientId, @doctorId, @date)",
            new Dictionary<string, object>()
            {
                ["@appointmentId"] = appointmentRequestDto.appointmentId,
                ["@patientId"] = appointmentRequestDto.patientId,
                ["@doctorId"] = doctorId,
                ["@date"] = DateTime.Now
            });

        foreach (var service in appointmentRequestDto.services.Select((service, index) => new { service, index }))
        {
            await CreateAppointmentService(
                appointmentRequestDto.appointmentId, 
                serviceIds[service.index],
                service.service.serviceFee);
        }
    }
}