using System;

namespace ContentManagementSystem.Models
{
    public class MaterialAssignment
    {
        public int Id { get; set; }
        public int MaterialOutId { get; set; }
        public int MaterialItemId { get; set; }
        public string EmployeeNumber { get; set; }
        public DateTime AssignmentDate { get; set; }
        
        // Navigation properties
        public virtual MaterialOut MaterialOut { get; set; }
        public virtual MaterialItem MaterialItem { get; set; }
        public virtual Employee Employee { get; set; }
    }
} 