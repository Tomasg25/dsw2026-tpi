using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class EntityDeletable : EntityBase
    {

        public bool Deleted { get; private set; }

        public EntityDeletable(Guid? id = null ):base(id) { } 
        public void IsDelete()
        {
            Deleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
