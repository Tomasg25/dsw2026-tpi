using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient: EntityBase
    {
        public long Dni { get; private set; }
        public string? Name { get; private set; }
        public string? Phone { get; private set; }


        #region Constructor for EF
#pragma warning disable CS8618
        private Patient() { }
#pragma warning restore CS8618
        #endregion
        //lo dejamos null hasta que exista algun flujo que lo complete

        public Patient(long dni, string? name =null, string? phone= null, Guid? id=null): base(id) 
        {
            Dni= dni;
            Name= name;
            Phone= phone;

        }
    }

}
