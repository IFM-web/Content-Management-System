namespace ContentManagementSystem.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
} 