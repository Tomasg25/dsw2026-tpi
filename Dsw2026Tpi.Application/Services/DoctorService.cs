using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<DoctorService> _logger;
    public DoctorService(IPersistence persistence, ILogger<DoctorService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                   d.Name.Contains(name))&& !d.Speciality.Deleted && !d.Deleted && d.IsActive, x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }


   

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        Validate(request);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId)
            ?? throw new ValidationException("La especialidad indicada no existe", "DOCTOR_SPECIALITY_NOT_FOUND");

        var doctor = new Doctor(request.Name, request.LicenseNumber, speciality);
        await _persistence.Add(doctor);

        return ToResponse(doctor);
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        Validate(request);

        var doctor = await _persistence.GetById<Doctor>(id, nameof(Doctor.Speciality))
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId)
            ?? throw new ValidationException("La especialidad indicada no existe", "DOCTOR_SPECIALITY_NOT_FOUND");

        doctor.UpdateDetails(speciality, request.Name, request.LicenseNumber);
        await _persistence.Update(doctor);

        return ToResponse(doctor);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        doctor.IsDelete();
        await _persistence.Update(doctor);
    }

    private static DoctorModel.Response ToResponse(Doctor d) =>
        new(d.Id, d.Name, d.LicenseNumber, new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name));

    private static void Validate(DoctorModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length is < 3 or > 100)
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres", "DOCTOR_NAME_INVALID");

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ValidationException("La matrícula es obligatoria", "DOCTOR_LICENSE_INVALID");
    }

}


