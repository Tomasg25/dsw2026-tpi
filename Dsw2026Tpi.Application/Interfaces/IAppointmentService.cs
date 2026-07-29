using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> Book(AppointmentModel.Request request);
        Task Cancel(Guid id);
        Task<IEnumerable<AppointmentModel.PatientAppointmentDto>> GetByPatient(long dni);
        Task<Pagination<AppointmentModel.SearchResultDto>> GetByDate(DateOnly date, int pageSize, int pageIndex);
        Task<Pagination<AppointmentModel.SearchResultDto>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date, int pageSize, int pageIndex);
    }
}
