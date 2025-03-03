using System;
using System.Collections.Generic;

namespace ContentManagementSystem.Models
{
    public class AvailableItemViewModel
    {
        public string SearchTerm { get; set; }
        public DateTime? BillDateFrom { get; set; }
        public DateTime? BillDateTo { get; set; }
        public DateTime? WarrantyDateFrom { get; set; }
        public DateTime? WarrantyDateTo { get; set; }
        public List<MaterialItem> AvailableItems { get; set; }

        public AvailableItemViewModel()
        {
            AvailableItems = new List<MaterialItem>();
        }
    }
} 