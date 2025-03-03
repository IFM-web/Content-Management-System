using System;
using System.Collections.Generic;

namespace ContentManagementSystem.Models
{
    public class AvailableStockViewModel
    {
        public string SearchTerm { get; set; }
        public DateTime? ReceivedDateFrom { get; set; }
        public DateTime? ReceivedDateTo { get; set; }
        public List<Material> Materials { get; set; }

        public AvailableStockViewModel()
        {
            Materials = new List<Material>();
        }
    }
} 