using System.Collections.Generic;

namespace Erkon.Models
{
    public class AccessMaintenanceModel
    {
        public AccessModel Access { get; set; }
        public List<UnitModel> Units { get; set; }
    }
}
