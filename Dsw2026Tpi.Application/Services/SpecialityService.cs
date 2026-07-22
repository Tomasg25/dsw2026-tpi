using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class SpecialityService : ISpecialityService
    {
        private readonly IPersistence _persistence;

        public SpecialityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
        {
            var result = await _persistence.Paginate<Speciality, string>(
                pageSize, pageIndex,
                s => s.IsActive && (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)),
                s => s.Name);

            return result.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
        }

        public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
        {
            Validate(request);

            var speciality = new Speciality(request.Name, request.Description);
            await _persistence.Add(speciality);

            return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
        }

        public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
        {
            Validate(request);

            var speciality = await _persistence.GetById<Speciality>(id)
             ?? throw new EntityNotFoundException(nameof(Speciality));

            speciality.UpdateDetails(request.Name, request.Description);
            await _persistence.Update(speciality);

            return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);

            // Name/Description son init-only -> hay que reemplazar la instancia o cambiar a set
            //var updated = new Speciality(request.Name, request.Description, speciality.Id);
            //await _persistence.Update(updated);

            //return new SpecialityModel.Response(updated.Id, updated.Name, updated.Description);
        }

        public async Task Delete(Guid id)
        {
            var speciality = await _persistence.GetById<Speciality>(id)
                ?? throw new EntityNotFoundException(nameof(Speciality));

            speciality.Deactivate();
            await _persistence.Update(speciality);
        }

        private static void Validate(SpecialityModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length is < 3 or > 100)
                throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres", "SPECIALITY_NAME_INVALID");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length is < 10 or > 100)
                throw new ValidationException("La descripción debe tener entre 10 y 100 caracteres", "SPECIALITY_DESCRIPTION_INVALID");
        }
    }
}
