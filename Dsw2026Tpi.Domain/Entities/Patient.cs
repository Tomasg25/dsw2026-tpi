using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient: EntityDeletable
    {
        public Guid? UserId { get; set; }
        public long Dni { get; private set; }
        public string? FullName { get; private set; }


        #region Constructor for EF
#pragma warning disable CS8618
        private Patient() { }
#pragma warning restore CS8618
        #endregion
        

        public Patient(long dni, string? fullName =null , Guid? id=null): base(id) 
        {
            Dni= dni;
            FullName= fullName;

        }
    }

}
