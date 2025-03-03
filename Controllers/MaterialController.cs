using ContentManagementSystem.Context;
using ContentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq.Expressions;

namespace ContentManagementSystem.Controllers
{
    public class MaterialController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, ILogger<MaterialController> logger)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;

            // Add default branches if none exist
            if (!_db.Branches.Any())
            {
                var defaultBranches = new List<Branch>
                {
                    new Branch { Name = "Main Branch", CompanyId = 1 },
                    new Branch { Name = "North Branch", CompanyId = 1 },
                    new Branch { Name = "South Branch", CompanyId = 1 }
                };
                _db.Branches.AddRange(defaultBranches);
                _db.SaveChanges();
            }

            // Add default employees if none exist
            if (!_db.Employees.Any())
            {
                var defaultEmployees = new List<Employee>
                {
                    new Employee 
                    { 
                        EmployeeId = "EMP001",
                        Name = "John Doe",
                        Department = "IT",
                        Email = "john.doe@company.com",
                        PhoneNo = "1234567890"
                    },
                    new Employee 
                    { 
                        EmployeeId = "EMP002",
                        Name = "Jane Smith",
                        Department = "HR",
                        Email = "jane.smith@company.com",
                        PhoneNo = "2345678901"
                    },
                    new Employee 
                    { 
                        EmployeeId = "EMP003",
                        Name = "Mike Johnson",
                        Department = "Finance",
                        Email = "mike.johnson@company.com",
                        PhoneNo = "3456789012"
                    },
                    new Employee 
                    { 
                        EmployeeId = "EMP004",
                        Name = "Sarah Williams",
                        Department = "Operations",
                        Email = "sarah.williams@company.com",
                        PhoneNo = "4567890123"
                    }
                };
                _db.Employees.AddRange(defaultEmployees);
                _db.SaveChanges();
            }
        }

        public IActionResult Index()
        {
            try
            {
                if (!_db.Companies.Any())
                {
                    var defaultCompany = new Company { Name = "ASP Securities" };
                    _db.Companies.Add(defaultCompany);
                    _db.SaveChanges();
                }

                if (!_db.Manufacturers.Any())
                {
                    var defaultManufacturers = new List<Manufacturer>
                    {
                        new Manufacturer { Name = "Dell" },
                        new Manufacturer { Name = "Lenovo" },
                        new Manufacturer { Name = "HP" }
                    };
                    _db.Manufacturers.AddRange(defaultManufacturers);
                    _db.SaveChanges();
                }

                if (!_db.Vendors.Any())
                {
                    var defaultVendors = new List<Vendor>
                    {
                        new Vendor { Name = "TCS" },
                        new Vendor { Name = "Infosys" },
                        new Vendor { Name = "HCL" }
                    };
                    _db.Vendors.AddRange(defaultVendors);
                    _db.SaveChanges();
                }

                if (!_db.AssetItems.Any())
                {
                    var defaultAssets = new List<AssetItem>
                    {
                        new AssetItem { Name = "Laptop" },
                        new AssetItem { Name = "Desktop" },
                        new AssetItem { Name = "Others" }
                    };
                    _db.AssetItems.AddRange(defaultAssets);
                    _db.SaveChanges();
                }

                SetupViewBag();
                ViewBag.ShowSidebar = true;
                return View(GetMaterialViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index action: {ex.Message}");
                return View("Error");
            }
        }

        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return null;

            try
            {
                var dateParts = dateStr.Split('/');
                if (dateParts.Length == 3)
                {
                    int day = int.Parse(dateParts[0]);
                    int month = int.Parse(dateParts[1]);
                    int year = int.Parse(dateParts[2]);
                    return new DateTime(year, month, day);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing date '{dateStr}': {ex.Message}");
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MaterialViewModel model, IFormFile ImageFile)
        {
            try
            {
                if (model?.NewMaterial == null)
                {
                    ModelState.AddModelError("", "Invalid form submission");
                    SetupViewBag();
                    return View("Index", GetMaterialViewModel());
                }

                var material = model.NewMaterial;
                var form = Request.Form;

                // Parse Bill Date
                var billDateStr = form["NewMaterial.BillDate"].ToString();
                if (!string.IsNullOrEmpty(billDateStr))
                {
                    if (DateTime.TryParseExact(billDateStr, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime billDate))
                    {
                        material.BillDate = billDate;
                    }
                    else
                    {
                        ModelState.AddModelError("NewMaterial.BillDate", "Please enter date in DD/MM/YYYY format");
                        SetupViewBag();
                        return View("Index", GetMaterialViewModel(model?.NewMaterial));
                    }
                }

                var isOtherAsset = _db.AssetItems.Find(material.AssetItemId)?.Name == "Others";
                var othersVendor = _db.Vendors.FirstOrDefault(v => v.Name == "Others");
                var othersManufacturer = _db.Manufacturers.FirstOrDefault(m => m.Name == "Others");

                // Remove validation for non-required fields when asset is Others
                if (isOtherAsset)
                {
                    ModelState.Remove("NewMaterial.VendorId");
                    ModelState.Remove("NewMaterial.ManufacturerId");
                    ModelState.Remove("NewMaterial.BillDate");
                    
                    // Set values for non-required fields
                    material.VendorId = othersVendor?.Id;
                    material.ManufacturerId = othersManufacturer?.Id;
                    material.BillDate = DateTime.Now;  // Or any default date you prefer
                }
                else
                {
                    // Validate required fields for non-Other assets
                    if (!material.VendorId.HasValue)
                        ModelState.AddModelError("NewMaterial.VendorId", "Vendor is required");
                    if (!material.ManufacturerId.HasValue)
                        ModelState.AddModelError("NewMaterial.ManufacturerId", "Manufacturer is required");
                    if (material.BillDate == default)
                        ModelState.AddModelError("NewMaterial.BillDate", "Bill Date is required");
                }

                _logger.LogInformation($"Attempting to create material with Invoice No: {material.InvoiceNo}");

                // Log all incoming data
                _logger.LogInformation($"Form Data: CompanyId={material.CompanyId}, " +
                    $"AssetItemId={material.AssetItemId}, " +
                    $"VendorId={material.VendorId}, " +
                    $"ManufacturerId={material.ManufacturerId}, " +
                    $"ReqnQuantity={material.ReqnQuantity}");

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));
                    _logger.LogWarning($"Model validation failed: {errors}");
                    
                    SetupViewBag();
                    return View("Index", GetMaterialViewModel(material));
                }

                // Load related entities
                material.AssetItem = _db.AssetItems.Find(material.AssetItemId);
                
                // Handle image upload if a file was provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(fileStream);
                    }
                    
                    material.ImagePath = $"/uploads/{uniqueFileName}";
                    _logger.LogInformation($"Invoice image saved: {material.ImagePath}");
                }

                // Set the record date and other calculated fields
                material.RecordDate = DateTime.Now;
                material.Month = material.BillDate.ToString("MMMM");
                material.Year = material.BillDate.Year.ToString();

                // Get the form collection to process material items
                var materialItems = new List<MaterialItem>();
                
                // Get the number of items from the REQN quantity
                int itemCount = material.ReqnQuantity;
                
                bool isComputerAsset = material.AssetItem?.Name == "Desktop" || material.AssetItem?.Name == "Laptop";
                
                _logger.LogInformation($"Processing {itemCount} items for {(isComputerAsset ? "Computer" : "Other")} asset");

                for (int i = 0; i < itemCount; i++)
                {
                    try
                    {
                        var item = new MaterialItem
                        {
                            SerialNo = form[$"NewMaterial.MaterialItems[{i}].SerialNo"],
                            ModelNo = form[$"NewMaterial.MaterialItems[{i}].ModelNo"],
                            AssetItemId = material.AssetItemId,
                            Status = "UnAssigned"
                        };

                        _logger.LogInformation($"Processing item {i + 1}: SerialNo={item.SerialNo}, ModelNo={item.ModelNo}, AssetItemId={item.AssetItemId}");

                        if (isComputerAsset)
                        {
                            item.Generation = form[$"NewMaterial.MaterialItems[{i}].Generation"];
                            if (int.TryParse(form[$"NewMaterial.MaterialItems[{i}].CPUCapacity"], out int cpuCapacity))
                                item.CPUCapacity = cpuCapacity;
                            if (int.TryParse(form[$"NewMaterial.MaterialItems[{i}].HardDisk"], out int hardDisk))
                                item.HardDisk = hardDisk;
                            if (int.TryParse(form[$"NewMaterial.MaterialItems[{i}].RAMCapacity"], out int ramCapacity))
                                item.RAMCapacity = ramCapacity;
                            if (int.TryParse(form[$"NewMaterial.MaterialItems[{i}].SSDCapacity"], out int ssdCapacity))
                                item.SSDCapacity = ssdCapacity;

                            _logger.LogInformation($"Computer specs: Gen={item.Generation}, CPU={item.CPUCapacity}, HDD={item.HardDisk}, RAM={item.RAMCapacity}, SSD={item.SSDCapacity}");
                        }
                        else
                        {
                            item.ItemName = form[$"NewMaterial.MaterialItems[{i}].ItemName"];
                            item.Other = form[$"NewMaterial.MaterialItems[{i}].Other"];
                        }

                        // Parse warranty date
                        var warrantyDateStr = form[$"NewMaterial.MaterialItems[{i}].WarrantyDate"].ToString();
                        if (!string.IsNullOrEmpty(warrantyDateStr))
                        {
                            if (DateTime.TryParseExact(warrantyDateStr, "dd/MM/yyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime warrantyDate))
                            {
                                item.WarrantyDate = warrantyDate;
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Invalid warranty date format for item {i + 1}. Use DD/MM/YYYY");
                                SetupViewBag();
                                return View("Index", GetMaterialViewModel(model?.NewMaterial));
                            }
                        }

                        materialItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing item {i}: {ex.Message}");
                        ModelState.AddModelError("", $"Error processing item {i + 1}: {ex.Message}");
                        SetupViewBag();
                        return View("Index", GetMaterialViewModel(model?.NewMaterial));
                    }
                }

                material.MaterialItems = materialItems;

                // Handle custom names for Others
                if (_db.AssetItems.Find(material.AssetItemId)?.Name == "Others")
                {
                    material.CustomAssetName = model.NewMaterial.CustomAssetName;
                }

                // Handle Vendor
                if (material.VendorId == othersVendor?.Id)
                {
                    material.CustomVendorName = form["NewMaterial.CustomVendorName"];
                    _logger.LogInformation($"Setting custom vendor name: {material.CustomVendorName}");
                }

                // Handle Manufacturer
                if (material.ManufacturerId == othersManufacturer?.Id)
                {
                    material.CustomManufacturerName = form["NewMaterial.CustomManufacturerName"];
                    _logger.LogInformation($"Setting custom manufacturer name: {material.CustomManufacturerName}");
                }

                // Parse dates
                if (!string.IsNullOrEmpty(Request.Form["BillDate"]))
                {
                    material.BillDate = ParseDate(Request.Form["BillDate"]) ?? DateTime.Now;
                }

                var receivedDate = ParseDate(Request.Form["ReceivedDate"]);
                if (receivedDate.HasValue)
                {
                    material.RecordDate = receivedDate.Value;
                }

                // Add and save to database
                _logger.LogInformation("Attempting to save to database...");
                _db.Materials.Add(material);
                _db.SaveChanges();

                TempData["Success"] = "Material created successfully!";
                _logger.LogInformation($"Successfully created material with ID: {material.Id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating material: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error saving to database: {ex.Message}");
                SetupViewBag();
                return View("Index", GetMaterialViewModel(model?.NewMaterial));
            }
        }

        private void SetupViewBag()
        {
            ViewBag.Companies = new SelectList(_db.Companies.OrderBy(c => c.Name), "Id", "Name");
            ViewBag.AssetItems = new SelectList(_db.AssetItems.OrderBy(a => a.Name), "Id", "Name");
            ViewBag.Vendors = new SelectList(_db.Vendors.OrderBy(v => v.Name), "Id", "Name");
            ViewBag.Manufacturers = new SelectList(_db.Manufacturers.OrderBy(m => m.Name), "Id", "Name");
        }

        private MaterialViewModel GetMaterialViewModel(Material material = null)
        {
            try
            {
                var materials = _db.Materials
                    .Include(m => m.Company)
                    .Include(m => m.Vendor)
                    .Include(m => m.Manufacturer)
                    .Include(m => m.AssetItem)
                    .Include(m => m.MaterialItems)
                    .ToList() ?? new List<Material>();

                return new MaterialViewModel
                {
                    Materials = materials,
                    NewMaterial = material ?? new Material 
                    { 
                        CompanyId = _db.Companies.FirstOrDefault()?.Id ?? 0 
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMaterialViewModel: {ex.Message}");
                return new MaterialViewModel
                {
                    Materials = new List<Material>(),
                    NewMaterial = material ?? new Material()
                };
            }
        }

        public IActionResult AvailableStock(string searchTerm, string receivedDateFrom, string receivedDateTo)
        {
            try 
            {
                DateTime? fromDate = null;
                DateTime? toDate = null;

                if (!string.IsNullOrEmpty(receivedDateFrom))
                {
                    DateTime.TryParseExact(receivedDateFrom, "dd/MM/yyyy", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFromDate);
                    fromDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(receivedDateTo))
                {
                    DateTime.TryParseExact(receivedDateTo, "dd/MM/yyyy", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedToDate);
                    toDate = parsedToDate;
                }

                var query = _db.Materials
                    .Include(m => m.Company)
                    .Include(m => m.Vendor)
                    .Include(m => m.Manufacturer)
                    .Include(m => m.AssetItem)
                    .Include(m => m.MaterialItems)
                    .Where(m => m.MaterialItems.Any(mi => mi.Status == "UnAssigned"))
                    .Select(m => new Material
                    {
                        Id = m.Id,
                        InvoiceNo = m.InvoiceNo,
                        AssetItem = m.AssetItem,
                        Vendor = m.Vendor,
                        Manufacturer = m.Manufacturer,
                        CustomAssetName = m.CustomAssetName,
                        CustomVendorName = m.CustomVendorName,
                        CustomManufacturerName = m.CustomManufacturerName,
                        BillDate = m.BillDate,
                        RecordDate = m.RecordDate,
                        MaterialItems = m.MaterialItems.Where(mi => mi.Status == "UnAssigned")
                            .Select(mi => new MaterialItem
                            {
                                Id = mi.Id,
                                SerialNo = mi.SerialNo,
                                ModelNo = mi.ModelNo,
                                ItemName = mi.ItemName,
                                Generation = mi.Generation,
                                CPUCapacity = mi.CPUCapacity,
                                HardDisk = mi.HardDisk,
                                RAMCapacity = mi.RAMCapacity,
                                SSDCapacity = mi.SSDCapacity,
                                Other = mi.Other,
                                WarrantyDate = mi.WarrantyDate,
                                Status = mi.Status,
                                AssetItemId = mi.AssetItemId
                            }).ToList()
                    })
                    .AsNoTracking();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(m => 
                        m.InvoiceNo.ToLower().Contains(searchTerm) ||
                        (m.AssetItem.Name == "Others" && !string.IsNullOrEmpty(m.CustomAssetName) && m.CustomAssetName.ToLower().Contains(searchTerm)) ||
                        (m.Vendor.Name == "Others" && !string.IsNullOrEmpty(m.CustomVendorName) && m.CustomVendorName.ToLower().Contains(searchTerm)) ||
                        (m.Manufacturer.Name == "Others" && !string.IsNullOrEmpty(m.CustomManufacturerName) && m.CustomManufacturerName.ToLower().Contains(searchTerm)) ||
                        m.AssetItem.Name.ToLower().Contains(searchTerm) ||
                        m.Vendor.Name.ToLower().Contains(searchTerm) ||
                        m.Manufacturer.Name.ToLower().Contains(searchTerm)
                    );
                }

                // Apply Received Date filters
                if (fromDate.HasValue)
                {
                    query = query.Where(m => m.RecordDate.Date >= fromDate.Value.Date);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(m => m.RecordDate.Date <= toDate.Value.Date);
                }

                var materials = query.ToList();

                var model = new AvailableStockViewModel
                {
                    SearchTerm = searchTerm,
                    ReceivedDateFrom = fromDate,
                    ReceivedDateTo = toDate,
                    Materials = materials
                };

                ViewBag.ShowSidebar = true;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AvailableStock: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult AvailableItems(string searchTerm, 
            DateTime? billDateFrom, DateTime? billDateTo,
            DateTime? warrantyDateFrom, DateTime? warrantyDateTo)
        {
            var query = _db.MaterialItems
                .Include(mi => mi.Material)
                    .ThenInclude(m => m.AssetItem)
                .Where(mi => mi.Status == "UnAssigned");

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(mi => 
                    (mi.Material.AssetItem.Name == "Others" && mi.ItemName.ToLower().Contains(searchTerm)) ||
                    (mi.Material.AssetItem.Name != "Others" && mi.Material.AssetItem.Name.ToLower().Contains(searchTerm)) ||
                    mi.SerialNo.ToLower().Contains(searchTerm) ||
                    mi.ModelNo.ToLower().Contains(searchTerm)
                );
            }

            // Apply Bill Date filters
            if (billDateFrom.HasValue)
            {
                query = query.Where(mi => mi.Material.BillDate >= billDateFrom.Value);
            }
            if (billDateTo.HasValue)
            {
                query = query.Where(mi => mi.Material.BillDate <= billDateTo.Value);
            }

            // Apply Warranty Date filters
            if (warrantyDateFrom.HasValue)
            {
                query = query.Where(mi => mi.WarrantyDate >= warrantyDateFrom.Value);
            }
            if (warrantyDateTo.HasValue)
            {
                query = query.Where(mi => mi.WarrantyDate <= warrantyDateTo.Value);
            }

            var model = new AvailableItemViewModel
            {
                SearchTerm = searchTerm,
                BillDateFrom = billDateFrom,
                BillDateTo = billDateTo,
                WarrantyDateFrom = warrantyDateFrom,
                WarrantyDateTo = warrantyDateTo,
                AvailableItems = query.ToList()
            };

            return View(model);
        }

        public IActionResult InvoiceDashboard()
        {
            try
            {
                var materials = _db.Materials
                    .Include(m => m.Company)
                    .Include(m => m.AssetItem)
                    .Include(m => m.Vendor)
                    .Include(m => m.Manufacturer)
                    .Include(m => m.MaterialItems)
                    .OrderByDescending(m => m.BillDate)
                    .ToList();

                var model = new MaterialViewModel
                {
                    Materials = materials
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in InvoiceDashboard: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult AssignedAssets()
        {
            try
            {
                var assignedAssets = _db.MaterialAssignments
                    .Include(ma => ma.MaterialOut)
                        .ThenInclude(mo => mo.Company)
                    .Include(ma => ma.MaterialOut)
                        .ThenInclude(mo => mo.Branch)
                    .Include(ma => ma.MaterialItem)
                        .ThenInclude(mi => mi.AssetItem)
                    .Include(ma => ma.Employee)
                    .Select(ma => new AssignedAssetItem
                    {
                        MaterialOutId = ma.MaterialOutId,
                        IssuanceDate = ma.AssignmentDate,
                        CompanyName = ma.MaterialOut.Company.Name,
                        BranchName = ma.MaterialOut.Branch.Name,
                        EmployeeId = ma.EmployeeNumber,
                        EmployeeName = ma.Employee.Name,
                        Department = ma.Employee.Department,
                        AssetName = ma.MaterialItem.AssetItem.Name,
                        SerialNo = ma.MaterialItem.SerialNo,
                        ModelNo = ma.MaterialItem.ModelNo,
                        WarrantyDate = ma.MaterialItem.WarrantyDate,
                        Status = ma.MaterialItem.Status
                    })
                    .OrderByDescending(a => a.IssuanceDate)
                    .ToList();

                var viewModel = new AssignedAssetsViewModel
                {
                    AssignedAssets = assignedAssets
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AssignedAssets: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult MaterialOut()
        {
            try
            {
                // Ensure we have at least one company
                if (!_db.Companies.Any())
                {
                    _db.Companies.Add(new Company { Name = "ASP Securities" });
                    _db.SaveChanges();
                }

                // Get all companies and assets
                var companies = _db.Companies.ToList();
                var firstCompany = companies.FirstOrDefault();
                var assets = _db.AssetItems.ToList();

                var model = new MaterialOutViewModel
                {
                    CompanyId = firstCompany?.Id ?? 1,  // Set default CompanyId
                    IssuanceDate = DateTime.Today  // Set default date to today
                };

                // Setup ViewBag
                ViewBag.Companies = new SelectList(companies, "Id", "Name", firstCompany?.Id);
                ViewBag.Assets = new SelectList(assets, "Id", "Name");

                // Get initial branches for the default company
                var branches = _db.Branches
                    .Where(b => b.CompanyId == model.CompanyId)
                    .Select(b => new
                    {
                        id = b.Id.ToString(),
                        text = b.Name
                    })
                    .ToList();

                ViewBag.InitialBranches = branches;
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MaterialOut action: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public JsonResult GetEmployeeDetails(string empId)
        {
            var employee = _db.Employees.FirstOrDefault(e => e.EmployeeId == empId);
            if (employee == null)
                return Json(new { });

            return Json(new {
                name = employee.Name,
                department = employee.Department,
                email = employee.Email,
                phoneNo = employee.PhoneNo
            });
        }

        [HttpGet]
        public JsonResult GetBranches(string search = null, int? companyId = null)
        {
            try
            {
                var query = _db.Branches
                    .Include(b => b.Company)
                    .AsQueryable();

                // Filter by company if provided
                if (companyId.HasValue)
                {
                    query = query.Where(b => b.CompanyId == companyId);
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(b => b.Name.ToLower().Contains(search));
                }

                // First get the data
                var branchData = query
                    .Select(b => new
                    {
                        Branch = b,
                        CompanyName = b.Company.Name
                    })
                    .ToList();  // Execute query here

                // Then format the display text
                var branches = branchData
                    .Select(b => new
                    {
                        id = b.Branch.Id.ToString(),
                        text = $"{b.Branch.Name} ({b.CompanyName})"
                    })
                    .OrderBy(b => b.text)
                    .Take(20)
                    .ToList();

                return Json(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting branches: {ex.Message}");
                return Json(new object[] { });
            }
        }

        [HttpGet]
        public JsonResult GetSubAssets(int assetId)
        {
            var subAssets = _db.MaterialItems
                .Where(m => m.AssetItemId == assetId && m.Status == "Available")
                .Select(m => new { value = m.Id, text = m.ItemName })
                .Distinct()
                .ToList();
            return Json(subAssets);
        }

        [HttpGet]
        public JsonResult GetEmployeesList(string search)
        {
            if (string.IsNullOrEmpty(search))
                return Json(new object[] { });

            var employees = _db.Employees
                .Where(e => e.EmployeeId.Contains(search) || 
                            e.Name.Contains(search) || 
                            e.Department.Contains(search))
                .Take(10)
                .Select(e => new { 
                    id = e.EmployeeId,
                    text = $"{e.EmployeeId} - {e.Name} ({e.Department})",
                    name = e.Name,
                    department = e.Department,
                    email = e.Email,
                    phoneNo = e.PhoneNo
                })
                .ToList();
            
            return Json(employees);
        }

        [HttpGet]
        public JsonResult GetAvailableSerialNumbers(int assetId, string search = "")
        {
            var items = _db.MaterialItems
                .Include(mi => mi.AssetItem)
                .Where(mi => mi.AssetItemId == assetId && mi.Status == "UnAssigned")
                .Select(mi => new
                {
                    serialNo = mi.SerialNo,
                    modelNo = mi.ModelNo,
                    generation = mi.Generation,
                    cpuCapacity = mi.CPUCapacity,
                    ramCapacity = mi.RAMCapacity,
                    hardDisk = mi.HardDisk,
                    ssdCapacity = mi.SSDCapacity,
                    windowsKey = mi.WindowsKey,
                    msOfficeKey = mi.MSOfficeKey,
                    warrantyDate = mi.WarrantyDate,
                    other = mi.Other
                });

            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.serialNo.Contains(search));
            }

            return Json(items.Take(10).ToList());
        }

        [HttpGet]
        public JsonResult GetMaterialItemDetails(string serialNo)
        {
            var item = _db.MaterialItems
                .FirstOrDefault(m => m.SerialNo == serialNo);
            return Json(new { modelNo = item?.ModelNo });
        }

        [HttpPost]
        public IActionResult CreateMaterialOut(MaterialOutViewModel model, string BranchName)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            // Check if branch exists or create new one
                            var branch = _db.Branches
                                .FirstOrDefault(b => b.Name.ToLower() == BranchName.ToLower() && 
                                                   b.CompanyId == model.CompanyId);
                                
                            if (branch == null)
                            {
                                branch = new Branch
                                {
                                    Name = BranchName,
                                    CompanyId = model.CompanyId
                                };
                                _db.Branches.Add(branch);
                                _db.SaveChanges();
                            }

                            // Check if employee exists or create new one
                            var employee = _db.Employees
                                .FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

                            if (employee == null)
                            {
                                employee = new Employee
                                {
                                    EmployeeId = model.EmployeeId,
                                    Name = model.EmployeeName,
                                    Department = model.Department,
                                    Email = model.EmailId,
                                    PhoneNo = model.PhoneNo
                                };
                                _db.Employees.Add(employee);
                                _db.SaveChanges();
                            }

                            // Create MaterialOut record
                            var materialOut = new MaterialOut
                            {
                                CompanyId = model.CompanyId,
                                BranchId = branch.Id,
                                EmployeeId = employee.EmployeeId,
                                IssuanceDate = model.IssuanceDate,
                                Remarks = model.Remarks
                            };
                            _db.MaterialOuts.Add(materialOut);
                            _db.SaveChanges();

                            // Process material items
                            var form = Request.Form;
                            var serialNumbers = form.Where(x => x.Key.StartsWith("serialno-select"))
                                                  .Select(x => x.Value.ToString())
                                                  .Where(x => !string.IsNullOrEmpty(x))
                                                  .ToList();

                            for (int i = 0; i < serialNumbers.Count; i++)
                            {
                                var serialNo = serialNumbers[i];
                                var materialItem = _db.MaterialItems
                                    .FirstOrDefault(m => m.SerialNo == serialNo);

                                if (materialItem != null)
                                {
                                    // Update material item status and keys
                                    materialItem.Status = "Assigned";
                                    materialItem.WindowsKey = form[$"windows-key[{i}]"].ToString();
                                    materialItem.MSOfficeKey = form[$"msoffice-key[{i}]"].ToString();
                                    _db.MaterialItems.Update(materialItem);

                                    // Create assignment record
                                    var assignment = new MaterialAssignment
                                    {
                                        MaterialOutId = materialOut.Id,
                                        MaterialItemId = materialItem.Id,
                                        EmployeeNumber = employee.EmployeeId,
                                        AssignmentDate = model.IssuanceDate
                                    };
                                    _db.MaterialAssignments.Add(assignment);
                                }
                            }

                            _db.SaveChanges();
                            transaction.Commit();

                            TempData["Success"] = "Material OUT created successfully";
                            return RedirectToAction("AssignedAssets");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError($"Error in CreateMaterialOut: {ex.Message}");
                            ModelState.AddModelError("", "Error saving changes. Please try again.");
                        }
                    }
                }

                // If we get here, something failed, redisplay form
                ViewBag.Companies = new SelectList(_db.Companies, "Id", "Name", model.CompanyId);
                ViewBag.Assets = new SelectList(_db.AssetItems, "Id", "Name");
                return View("MaterialOut", model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateMaterialOut: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public JsonResult GetBranchesDebug(int? companyId = null)
        {
            var query = _db.Branches.AsQueryable();
            if (companyId.HasValue)
            {
                query = query.Where(b => b.CompanyId == companyId);
            }

            var allBranches = query
                .Select(b => new { 
                    id = b.Id, 
                    name = b.Name,
                    companyId = b.CompanyId
                })
                .ToList();

            return Json(new { 
                branchCount = allBranches.Count,
                branches = allBranches
            });
        }

        [HttpPost]
        public JsonResult CreateBranch(string name, int companyId)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return Json(new { success = false, message = "Branch name is required" });

                var branch = new Branch
                {
                    Name = name,
                    CompanyId = companyId
                };

                _db.Branches.Add(branch);
                _db.SaveChanges();

                return Json(new { 
                    success = true, 
                    branchId = branch.Id,
                    message = "Branch created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating branch: {ex.Message}");
                return Json(new { success = false, message = "Error creating branch" });
            }
        }
    }
} 