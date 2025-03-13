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
using ContentManagementSystem.Filters;
using System.Threading.Tasks;

namespace ContentManagementSystem.Controllers
{
    [AuthenticationFilter]
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

          
           

           
        }

        public IActionResult Index()
        {
            PopulateDropDowns();
            return View(new MaterialViewModel());
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
        public async Task<IActionResult> Create(MaterialViewModel model, IFormFile ImageFile)
        {
            try
            {
                if (model?.NewMaterial == null)
                {
                    ModelState.AddModelError("", "Invalid form submission");
                    PopulateDropDowns();
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
                        PopulateDropDowns();
                        return View("Index", GetMaterialViewModel(model?.NewMaterial));
                    }
                }

                // Get or create the "Others" vendor
                var othersVendor = _db.Vendors.FirstOrDefault(v => v.Name == "Others");
                if (othersVendor == null)
                {
                    othersVendor = new Vendor { Name = "Others" };
                    _db.Vendors.Add(othersVendor);
                    _db.SaveChanges();
                }

                // Set VendorId to Others and validate CustomVendorName
                material.VendorId = othersVendor.Id;
                if (string.IsNullOrWhiteSpace(material.CustomVendorName))
                {
                    ModelState.AddModelError("NewMaterial.CustomVendorName", "Vendor Name is required");
                }

                var isOtherAsset = _db.AssetItems.Find(material.AssetItemId)?.Name == "Others";

                // Remove validation for non-required fields when asset is Others
                if (isOtherAsset)
                {
                    ModelState.Remove("NewMaterial.BillDate");
                    // Don't remove Manufacturer validation - it should be independent of Asset type
                    material.BillDate = DateTime.Now;
                }
                else
                {
                    // Validate required fields for non-Other assets
                    if (material.BillDate == default)
                        ModelState.AddModelError("NewMaterial.BillDate", "Bill Date is required");
                }

                // Always validate Manufacturer
                if (!material.ManufacturerId.HasValue)
                    ModelState.AddModelError("NewMaterial.ManufacturerId", "Manufacturer is required");

                // Handle Manufacturer
                var othersManufacturer = _db.Manufacturers.FirstOrDefault(m => m.Name == "Others");
                if (material.ManufacturerId == othersManufacturer?.Id)
                {
                    if (string.IsNullOrWhiteSpace(material.CustomManufacturerName))
                    {
                        ModelState.AddModelError("NewMaterial.CustomManufacturerName", "Custom Manufacturer Name is required when 'Others' is selected");
                    }
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
                    
                    PopulateDropDowns();
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

                // Create a HashSet to track serial numbers in current submission
                var currentSerialNumbers = new HashSet<string>();

                for (int i = 0; i < itemCount; i++)
                {
                    try
                    {
                        var serialNo = form[$"NewMaterial.MaterialItems[{i}].SerialNo"].ToString();
                        
                        // Skip empty serial numbers
                        if (string.IsNullOrWhiteSpace(serialNo))
                        {
                            continue;
                        }

                        // Check if serial number is already used in database
                        if (await _db.MaterialItems.AnyAsync(m => m.SerialNo == serialNo))
                        {
                            ModelState.AddModelError("", $"Serial number '{serialNo}' is already in use");
                            _logger.LogWarning($"Duplicate serial number attempt: {serialNo}");
                            PopulateDropDowns();
                            return View("Index", GetMaterialViewModel(material));
                        }

                        // Check if serial number is duplicated in current submission
                        if (!currentSerialNumbers.Add(serialNo))
                        {
                            ModelState.AddModelError("", $"Serial number '{serialNo}' is used multiple times in this submission");
                            _logger.LogWarning($"Duplicate serial number in submission: {serialNo}");
                            PopulateDropDowns();
                            return View("Index", GetMaterialViewModel(material));
                        }

                        var item = new MaterialItem
                        {
                            SerialNo = serialNo,
                            ModelNo = form[$"NewMaterial.MaterialItems[{i}].ModelNo"].ToString(),
                            AssetItemId = material.AssetItemId,
                            Status = "UnAssigned"
                        };

                        _logger.LogInformation($"Processing item {i + 1}: SerialNo={item.SerialNo}, ModelNo={item.ModelNo}, AssetItemId={item.AssetItemId}");

                        if (isComputerAsset)
                        {
                            item.Generation = form[$"NewMaterial.MaterialItems[{i}].Generation"];
                            item.Processor = form[$"NewMaterial.MaterialItems[{i}].Processor"];
                            // Parse and set RAM value
                            if (int.TryParse(form[$"NewMaterial.MaterialItems[{i}].RAMCapacity"], out int ramValue))
                            {
                                item.RAMCapacity = ramValue;
                            }
                            else
                            {
                                _logger.LogWarning($"Invalid RAM value for item {i}");
                            }
                            item.HardDisk = form[$"NewMaterial.MaterialItems[{i}].HardDisk"];
                            item.SSDCapacity = form[$"NewMaterial.MaterialItems[{i}].SSDCapacity"];

                            _logger.LogInformation($"Computer specs: Gen={item.Generation}, " +
                                $"Processor={item.Processor}, RAM={item.RAMCapacity}GB, " +
                                $"HDD={item.HardDisk}, SSD={item.SSDCapacity}");
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
                                PopulateDropDowns();
                                return View("Index", GetMaterialViewModel(model?.NewMaterial));
                            }
                        }

                        materialItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing item {i}: {ex.Message}");
                        ModelState.AddModelError("", $"Error processing item {i + 1}: {ex.Message}");
                        PopulateDropDowns();
                        return View("Index", GetMaterialViewModel(model?.NewMaterial));
                    }
                }

                material.MaterialItems = materialItems;

                // Handle custom names for Others
                if (_db.AssetItems.Find(material.AssetItemId)?.Name == "Others")
                {
                    material.CustomAssetName = model.NewMaterial.CustomAssetName;
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

                // Check for duplicate serial numbers within the form
                var serialNumbers = material.MaterialItems.Select(m => m.SerialNo).ToList();
                if (serialNumbers.Count != serialNumbers.Distinct().Count())
                {
                    ModelState.AddModelError("", "Duplicate serial numbers found in the form");
                    TempData["Error"] = "Duplicate serial numbers are not allowed";
                    return View("Index", GetMaterialViewModel(material));
                }

                // Check each serial number against the database
                foreach (var item in material.MaterialItems)
                {
                    if (!string.IsNullOrEmpty(item.SerialNo))
                    {
                        if (!await IsSerialNumberUnique(item.SerialNo))
                        {
                            ModelState.AddModelError("", $"Serial number {item.SerialNo} already exists in the system");
                            TempData["Error"] = $"Serial number {item.SerialNo} is already in use";
                            return View("Index", GetMaterialViewModel(material));
                        }
                    }
                }

                // Add and save to database
                _logger.LogInformation("Attempting to save to database...");
                _db.Materials.Add(material);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Material created successfully!";
                _logger.LogInformation($"Successfully created material with ID: {material.Id}");

                foreach (var item in material.MaterialItems)
                {
                    _logger.LogInformation($"Material Item - Serial: {item.SerialNo}, Processor: {item.Processor}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Check for duplicate invoice number
                if (ex.InnerException?.Message.Contains("IX_Materials_InvoiceNo") == true)
                {
                    var invoiceNo = model.NewMaterial.InvoiceNo;
                    ModelState.AddModelError("NewMaterial.InvoiceNo", 
                        $"Invoice number '{invoiceNo}' already exists. Please use a different invoice number.");
                    TempData["Error"] = $"Invoice number '{invoiceNo}' is already in use";
                    
                    // Log the duplicate invoice attempt
                    _logger.LogWarning($"Attempted to create duplicate invoice number: {invoiceNo}");
                }
                // Handle duplicate serial numbers
                else if (ex.InnerException?.Message.Contains("UC_MaterialItems_SerialNo") == true)
                {
                    ModelState.AddModelError("", "One or more serial numbers are already in use");
                    TempData["Error"] = "Duplicate serial numbers are not allowed";
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while saving the material");
                    TempData["Error"] = "An error occurred while saving";
                }
                PopulateDropDowns();
                return View("Index", GetMaterialViewModel(model?.NewMaterial));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating material: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error saving to database: {ex.Message}");
                PopulateDropDowns();
                return View("Index", GetMaterialViewModel(model?.NewMaterial));
            }
        }

        private void PopulateDropDowns()
        {
            ViewBag.Companies = new SelectList(_db.Companies.Where(c => c.Id == 1).OrderBy(c => c.Name),    "Id","Name");
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
                                Processor = mi.Processor,
                                HardDisk = mi.HardDisk,
                                RAMCapacity = mi.RAMCapacity,
                                SSDCapacity = mi.SSDCapacity,
                                Other = mi.Other,
                                WarrantyDate = mi.WarrantyDate,
                           
                                Status = mi.Status,
                                AssetItemId = mi.AssetItemId,
                               
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

        public IActionResult InvoiceRecord(string searchTerm, string startDate, string endDate, 
            string receivedDateFrom, string receivedDateTo)
        {
            try
            {
                // Parse dates
                DateTime? billDateFrom = ParseDate(startDate);
                DateTime? billDateTo = ParseDate(endDate);
                DateTime? recDateFrom = ParseDate(receivedDateFrom);
                DateTime? recDateTo = ParseDate(receivedDateTo);

                var query = _db.Materials
                    .Include(m => m.Company)
                    .Include(m => m.AssetItem)
                    .Include(m => m.Vendor)
                    .Include(m => m.Manufacturer)
                    .Include(m => m.MaterialItems)
                    .AsNoTracking();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(m => 
                        m.InvoiceNo.ToLower().Contains(searchTerm) ||
                        m.Company.Name.ToLower().Contains(searchTerm) ||
                        m.AssetItem.Name.ToLower().Contains(searchTerm) ||
                        m.CustomVendorName.ToLower().Contains(searchTerm) ||
                        m.Manufacturer.Name.ToLower().Contains(searchTerm) ||
                        m.MaterialItems.Any(mi => 
                            mi.SerialNo.ToLower().Contains(searchTerm) ||
                            mi.ModelNo.ToLower().Contains(searchTerm))
                    );
                }

                // Apply date filters
                if (billDateFrom.HasValue)
                    query = query.Where(m => m.BillDate.Date >= billDateFrom.Value.Date);
                if (billDateTo.HasValue)
                    query = query.Where(m => m.BillDate.Date <= billDateTo.Value.Date);
                if (recDateFrom.HasValue)
                    query = query.Where(m => m.RecordDate.Date >= recDateFrom.Value.Date);
                if (recDateTo.HasValue)
                    query = query.Where(m => m.RecordDate.Date <= recDateTo.Value.Date);

                // Order by date descending
                query = query.OrderByDescending(m => m.BillDate);

                var model = new MaterialViewModel
                {
                    Materials = query.ToList(),
                    SearchTerm = searchTerm,
                    StartDate = startDate,
                    EndDate = endDate,
                    ReceivedDateFrom = receivedDateFrom,
                    ReceivedDateTo = receivedDateTo
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in InvoiceDashboard: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult AssignedAssets(string? issuanceDateFrom, string issuanceDateTo)
        {
            try

            {
                DateTime? fromDate = DateTime.Today;
                DateTime? toDate = DateTime.Today;

                if (!string.IsNullOrEmpty(issuanceDateFrom))
                {
                    DateTime.TryParseExact(issuanceDateFrom, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFromDate);
                    fromDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(issuanceDateTo))
                {
                    DateTime.TryParseExact(issuanceDateTo, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedToDate);
                    toDate = parsedToDate;
                }

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

                if(issuanceDateFrom != null || issuanceDateTo !=null)
                {
                    assignedAssets= assignedAssets.Where(ma => ma.IssuanceDate >= fromDate && ma.IssuanceDate <= toDate).ToList();
                }

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
            SetupMaterialOutViewBag();
            return View(new MaterialOutViewModel());
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
                    processor = mi.Processor,
                    ramCapacity = mi.RAMCapacity,
                    hardDisk = mi.HardDisk,
                    ssdCapacity = mi.SSDCapacity,
                    windowsKey = mi.WindowsKey,
                    msOfficeKey = mi.MSOfficeKey,
                    warrantyDate = mi.WarrantyDate,
                    other = mi.Other,
                   

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
        public async Task<IActionResult> MaterialOut(MaterialOutViewModel model)
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
                                .FirstOrDefault(b => b.Name.ToLower() == model.BranchName.ToLower() && 
                                                   b.CompanyId == model.CompanyId);
                                
                            if (branch == null)
                            {
                                branch = new Branch
                                {
                                    Name = model.BranchName,
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

                            var windowsKeys = form["WindowsKey"].ToString().Split(',');
                            var msOfficeKeys = form["MSOfficeKey"].ToString().Split(',');

                            for (int i = 0; i < serialNumbers.Count; i++)
                            {
                                var serialNo = serialNumbers[i];
                                var materialItem = _db.MaterialItems
                                    .FirstOrDefault(m => m.SerialNo == serialNo);

                                if (materialItem != null)
                                {
                                    materialItem.Status = "Assigned";
                                    materialItem.WindowsKey = windowsKeys.Length > i ? windowsKeys[i] : "";
                                    materialItem.MSOfficeKey = msOfficeKeys.Length > i ? msOfficeKeys[i] : "";
                                    _db.MaterialItems.Update(materialItem);

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

                            TempData["Success"] = $"Material has been successfully assigned to {model.EmployeeName} ({model.EmployeeId})";
                            ModelState.Clear();
                            // Prepare a fresh model with necessary ViewBag data
                            var newModel = new MaterialOutViewModel();
                            SetupMaterialOutViewBag();
                            return View(newModel);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError($"Error in MaterialOut: {ex.Message}");
                            ModelState.AddModelError("", "An error occurred while processing the request.");
                            SetupMaterialOutViewBag();
                            return View("MaterialOut", model);
                        }
                    }
                }
                SetupMaterialOutViewBag();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MaterialOut: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while processing the request.");
                SetupMaterialOutViewBag();
                return View("MaterialOut", model);
            }
        }

        private void SetupMaterialOutViewBag()
        {
            ViewBag.Companies = new SelectList(_db.Companies.OrderBy(c => c.Name), "Id", "Name");
            ViewBag.AssetItems = new SelectList(_db.AssetItems.OrderBy(a => a.Name), "Id", "Name");
            ViewBag.Assets = ViewBag.AssetItems;
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

        // Add this method to check for duplicate serial numbers
        private async Task<bool> IsSerialNumberUnique(string serialNo)
        {
            return !await _db.MaterialItems.AnyAsync(m => m.SerialNo == serialNo);
        }

        // Add an API endpoint to check serial number availability
        [HttpGet]
        public async Task<IActionResult> CheckSerialNumber(string serialNo)
        {
            if (string.IsNullOrEmpty(serialNo))
            {
                return Json(new { isAvailable = false, message = "Serial number cannot be empty" });
            }

            var isUnique = await IsSerialNumberUnique(serialNo);
            return Json(new { 
                isAvailable = isUnique, 
                message = isUnique ? "Serial number is available" : "Serial number already exists" 
            });
        }

        [HttpGet]
        public IActionResult GetMaterialItems(int materialId)
        {
            try
            {
                var materialItems = _db.MaterialItems
                    .Include(mi => mi.Material)
                        .ThenInclude(m => m.AssetItem)
                    .Where(mi => mi.MaterialId == materialId)
                    .Select(mi => new
                    {
                        mi.SerialNo,
                        mi.ModelNo,
                        mi.ItemName,
                        mi.Generation,
                        mi.Processor,
                        mi.RAMCapacity,
                        mi.HardDisk,
                        mi.SSDCapacity,
                        mi.WindowsKey,
                        mi.MSOfficeKey,
                        mi.Other,
                        WarrantyDate = mi.WarrantyDate.HasValue ? mi.WarrantyDate.Value.ToString("dd/MM/yyyy") : "",
                        mi.Status,
                        AssetType = mi.Material.AssetItem.Name
                    })
                    .ToList();

                return Json(new { success = true, data = materialItems });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching material items: {ex.Message}");
                return Json(new { success = false, message = "Error fetching material items" });
            }
        }

        // Add a method to check if invoice number exists
        [HttpGet]
        public async Task<IActionResult> CheckInvoiceNumber(string invoiceNo)
        {
            try
            {
                var exists = await _db.Materials.AnyAsync(m => m.InvoiceNo == invoiceNo);
                return Json(new { 
                    isAvailable = !exists, 
                    message = exists ? "This invoice number already exists" : "Invoice number is available" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking invoice number: {ex.Message}");
                return Json(new { isAvailable = false, message = "Error checking invoice number" });
            }
        }
    }
} 