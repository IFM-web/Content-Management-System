using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace ContentManagementSystem.Models
{
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
        public int? CPUCapacity { get; set; }
        public int? HardDisk { get; set; }
        public int? RAMCapacity { get; set; }
        public int? SSDCapacity { get; set; }
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