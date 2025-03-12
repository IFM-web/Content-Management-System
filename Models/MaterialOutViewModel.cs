using System;
using System.Collections.Generic;
namespace ContentManagementSystem.Models
{
public class MaterialOutViewModel
{
    public MaterialOutViewModel()
    {
        // Initialize the Items collection in constructor
        Items = new List<MaterialOutItemViewModel>();
        IssuanceDate = DateTime.Today;
    }

    public int CompanyId { get; set; }
    public string BranchName { get; set; }
    public int BranchId { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public string EmailId { get; set; }
    public string PhoneNo { get; set; }
    public DateTime IssuanceDate { get; set; }
    public string Remarks { get; set; }
    public List<MaterialOutItemViewModel> Items { get; set; }
}

public class MaterialOutItemViewModel
{
    public int AssetId { get; set; }
    public int SubAssetId { get; set; }
    public string SerialNo { get; set; }
    public string ModelNo { get; set; }
    public int Quantity { get; set; }
    public string OtherDetails { get; set; }
    public string MSOfficeKey { get; set; }
    public string WindowsKey { get; set; }
} 
}