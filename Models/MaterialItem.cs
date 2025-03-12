using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // Add this for Index attribute

namespace ContentManagementSystem.Models
{
    [Index(nameof(SerialNo), IsUnique = true)]  // Use the correct attribute
    public class MaterialItem
    {
        private DateTime? _warrantyDate;

        public MaterialItem()
        {
            Status = "UnAssigned";  // Set default value
        }

        public int Id { get; set; }
        public int MaterialId { get; set; }
        public int AssetItemId { get; set; }  // Add this property
        public string Status { get; set; }
        
        // Common fields
        [Required]
        public string SerialNo { get; set; }
        [Required]
        public string ModelNo { get; set; }
        
        // Fields for Desktop/Laptop
        public string Generation { get; set; }
        
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Processor can only contain letters, numbers, spaces and hyphens")]
        [StringLength(100)]
        public string Processor { get; set; }
        [RegularExpression(@"^\d+(\.\d{1,2})?\s*(GB|TB)$", ErrorMessage = "Please enter a valid size with GB or TB (e.g., 500 GB, 1 TB, 2.5 TB)")]
        [StringLength(50)]
        public string HardDisk { get; set; }
        public int? RAMCapacity { get; set; }
        [RegularExpression(@"^\d+(\.\d{1,2})?\s*(GB|TB)$", ErrorMessage = "Please enter a valid size with GB or TB (e.g., 500 GB, 1 TB, 2.5 TB)")]
        [StringLength(50)]
        public string SSDCapacity { get; set; }
        public string Other { get; set; }
        
        // Fields for Other items
        public string ItemName { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [ModelBinder(BinderType = typeof(CustomDateTimeModelBinder))]
        public DateTime? WarrantyDate 
        { 
            get => _warrantyDate;
            set => _warrantyDate = value;
        }

        [NotMapped]
        public string WarrantyDateString
        {
            get => _warrantyDate?.ToString("dd/MM/yyyy");
            set
            {
                if (DateTime.TryParseExact(value, "dd/MM/yyyy", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, 
                    out DateTime date))
                {
                    _warrantyDate = date;
                }
            }
        }

        public string WindowsKey { get; set; }
        public string MSOfficeKey { get; set; }

        // Navigation properties
        public virtual Material Material { get; set; }
        public virtual AssetItem AssetItem { get; set; }  // Add this navigation property
    }
} 