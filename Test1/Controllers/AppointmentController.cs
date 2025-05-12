using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[Route("/api/appointments")]
[ApiController]
public class AppointmentController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    public AppointmentController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var appointment = await _appointmentService.GetAppointment(id);

        if (appointment is null)
        {
            return NotFound();
        }
        
        return Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewAppointment([FromBody] AppointmentRequestDTO appointmentRequestDto)
    {
        if (await _appointmentService.AppointmentExists(appointmentRequestDto.appointmentId))
        {
            return Conflict("Appointment already exists");
        }

        if (!await _appointmentService.PatientExists(appointmentRequestDto.patientId))
        {
            return NotFound("Patient not found");
        }

        var doctorId = await _appointmentService.GetDoctorIdWithPwz(appointmentRequestDto.pwz);
        if (doctorId == null)
        {
            return NotFound("Doctor not found");
        }
        
        var serviceIds = await Task
            .WhenAll(appointmentRequestDto.services
                .Select(async service => await _appointmentService.GetServiceIdByNames(service.serviceName)));
        if (serviceIds.ToList().Any(a => a == null))
        {
            return BadRequest("One of the services wasn't not found");
        }
        
        await _appointmentService.CreateAppointment(appointmentRequestDto, (int)doctorId, serviceIds
            .ToList()
            .Select(id => (int)id!)
            .ToList());
        
        return Ok();
    }
}