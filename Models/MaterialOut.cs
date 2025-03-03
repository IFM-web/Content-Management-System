using System;
using ContentManagementSystem.Models;

namespace ContentManagementSystem.Models
{
    public class MaterialOut
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime IssuanceDate { get; set; }
        public string Remarks { get; set; }

        public virtual Company Company { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Employee Employee { get; set; }
    }
} 