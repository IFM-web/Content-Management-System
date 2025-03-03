using System.ComponentModel.DataAnnotations;

namespace ContentManagementSystem.Models
{
    public class AssetItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
} 