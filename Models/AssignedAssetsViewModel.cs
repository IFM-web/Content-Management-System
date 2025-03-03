using System;
using System.Collections.Generic;
using ContentManagementSystem.Models;

namespace ContentManagementSystem.Models
{
    public class AssignedAssetsViewModel
    {
        public List<AssignedAssetItem> AssignedAssets { get; set; } = new List<AssignedAssetItem>();
    }

    public class AssignedAssetItem
    {
        public int MaterialOutId { get; set; }
        public DateTime IssuanceDate { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string AssetName { get; set; }
        public string SerialNo { get; set; }
        public string ModelNo { get; set; }
        public DateTime? WarrantyDate { get; set; }
        public string Status { get; set; }
    }
} 