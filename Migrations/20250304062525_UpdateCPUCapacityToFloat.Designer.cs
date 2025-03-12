﻿// <auto-generated />
using System;
using ContentManagementSystem.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ContentManagementSystem.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250304062525_UpdateCPUCapacityToFloat")]
    partial class UpdateCPUCapacityToFloat
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ContentManagementSystem.Models.AssetItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("AssetItems");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Laptop"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Desktop"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Server"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Others"
                        });
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Branch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Branches");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CompanyId = 1,
                            Name = "Main Branch"
                        },
                        new
                        {
                            Id = 2,
                            CompanyId = 1,
                            Name = "North Branch"
                        },
                        new
                        {
                            Id = 3,
                            CompanyId = 1,
                            Name = "South Branch"
                        });
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Companies");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "ASP Securities"
                        });
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Manufacturer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Manufacturers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Dell"
                        },
                        new
                        {
                            Id = 2,
                            Name = "HP"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Lenovo"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Others"
                        });
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AssetItemId")
                        .HasColumnType("int");

                    b.Property<DateTime>("BillDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("CustomAssetName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomManufacturerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomVendorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InvoiceNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("ManufacturerId")
                        .HasColumnType("int");

                    b.Property<string>("Month")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RecordDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ReqnQuantity")
                        .HasColumnType("int");

                    b.Property<int?>("VendorId")
                        .HasColumnType("int");

                    b.Property<string>("Year")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AssetItemId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("InvoiceNo")
                        .IsUnique();

                    b.HasIndex("ManufacturerId");

                    b.HasIndex("VendorId");

                    b.ToTable("Materials");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AssignmentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmployeeNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("MaterialItemId")
                        .HasColumnType("int");

                    b.Property<int>("MaterialOutId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeNumber");

                    b.HasIndex("MaterialItemId");

                    b.HasIndex("MaterialOutId");

                    b.ToTable("MaterialAssignments");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AssetItemId")
                        .HasColumnType("int");

                    b.Property<float?>("CPUCapacity")
                        .HasColumnType("real");

                    b.Property<string>("Generation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HardDisk")
                        .HasColumnType("int");

                    b.Property<string>("ItemName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MSOfficeKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaterialId")
                        .HasColumnType("int");

                    b.Property<string>("ModelNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Other")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RAMCapacity")
                        .HasColumnType("int");

                    b.Property<int?>("SSDCapacity")
                        .HasColumnType("int");

                    b.Property<string>("SerialNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("WarrantyDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("WindowsKey")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AssetItemId");

                    b.HasIndex("MaterialId");

                    b.ToTable("MaterialItems");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialOut", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BranchId")
                        .HasColumnType("int");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("EmployeeId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EmployeeId1")
                        .HasColumnType("int");

                    b.Property<DateTime>("IssuanceDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Remarks")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BranchId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("EmployeeId1");

                    b.ToTable("MaterialOuts");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Users", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Vendor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Vendors");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Dell"
                        },
                        new
                        {
                            Id = 2,
                            Name = "HP"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Lenovo"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Others"
                        });
                });

            modelBuilder.Entity("Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Department")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmployeeId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNo")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Branch", b =>
                {
                    b.HasOne("ContentManagementSystem.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Material", b =>
                {
                    b.HasOne("ContentManagementSystem.Models.AssetItem", "AssetItem")
                        .WithMany()
                        .HasForeignKey("AssetItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ContentManagementSystem.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ContentManagementSystem.Models.Manufacturer", "Manufacturer")
                        .WithMany()
                        .HasForeignKey("ManufacturerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ContentManagementSystem.Models.Vendor", "Vendor")
                        .WithMany()
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("AssetItem");

                    b.Navigation("Company");

                    b.Navigation("Manufacturer");

                    b.Navigation("Vendor");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialAssignment", b =>
                {
                    b.HasOne("Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeNumber")
                        .HasPrincipalKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ContentManagementSystem.Models.MaterialItem", "MaterialItem")
                        .WithMany()
                        .HasForeignKey("MaterialItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ContentManagementSystem.Models.MaterialOut", "MaterialOut")
                        .WithMany()
                        .HasForeignKey("MaterialOutId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("MaterialItem");

                    b.Navigation("MaterialOut");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialItem", b =>
                {
                    b.HasOne("ContentManagementSystem.Models.AssetItem", "AssetItem")
                        .WithMany()
                        .HasForeignKey("AssetItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ContentManagementSystem.Models.Material", "Material")
                        .WithMany("MaterialItems")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AssetItem");

                    b.Navigation("Material");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.MaterialOut", b =>
                {
                    b.HasOne("ContentManagementSystem.Models.Branch", "Branch")
                        .WithMany()
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ContentManagementSystem.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId1");

                    b.Navigation("Branch");

                    b.Navigation("Company");

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("ContentManagementSystem.Models.Material", b =>
                {
                    b.Navigation("MaterialItems");
                });
#pragma warning restore 612, 618
        }
    }
}
