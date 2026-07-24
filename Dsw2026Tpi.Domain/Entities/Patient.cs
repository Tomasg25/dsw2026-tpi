using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient: EntityBase
    {
        public long Dni { get; private set; }
        public string? Nombre { get; private set; }
        public string? Telefono { get; private set; }
      

        private Patient() { 
        
        }
        //lo dejamos null hasta que exista algun flujo que lo complete

        public Patient(long dni, string? nombre =null, string? telefono= null, Guid? id=null): base(id) 
        {
            Dni= dni;
            Nombre= nombre;
            Telefono= telefono;

        }
    }

}
