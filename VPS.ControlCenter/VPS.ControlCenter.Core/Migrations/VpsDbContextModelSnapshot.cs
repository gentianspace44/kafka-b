﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VPS.ControlCenter.Core;

#nullable disable

namespace VPS.ControlCenter.Core.Migrations
{
    [DbContext(typeof(VpsDbContext))]
    partial class VpsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("VPS.ControlCenter.Core.Entities.DynamicSetting", b =>
                {
                    b.Property<int>("DynamicSettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DynamicSettingId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.HasKey("DynamicSettingId");

                    b.ToTable("DynamicSettings", (string)null);
                });

            modelBuilder.Entity("VPS.ControlCenter.Core.Entities.FeatureToggle", b =>
                {
                    b.Property<int>("FeatureToggleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FeatureToggleId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("ToggleValue")
                        .HasMaxLength(1000)
                        .HasColumnType("bit");

                    b.HasKey("FeatureToggleId");

                    b.ToTable("FeatureToggles", (string)null);
                });

            modelBuilder.Entity("VPS.ControlCenter.Core.Entities.VoucherProvider", b =>
                {
                    b.Property<int>("VoucherProviderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VoucherProviderId"), 1L, 1);

                    b.Property<string>("ImageSource")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("bit");

                    b.Property<string>("MicroServiceUrl")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("SyxCreditServiceUrl")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool?>("UseSxyCreditEndPoint")
                        .HasColumnType("bit");

                    b.Property<int>("VoucherTypeId")
                        .HasColumnType("int");

                    b.HasKey("VoucherProviderId");

                    b.HasIndex("VoucherTypeId");

                    b.ToTable("VoucherProviders", (string)null);
                });

            modelBuilder.Entity("VPS.ControlCenter.Core.Entities.VoucherType", b =>
                {
                    b.Property<int>("VoucherTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VoucherTypeId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("VoucherLength")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("VoucherTypeId");

                    b.ToTable("VoucherTypes", (string)null);

                    b.HasData(
                        new
                        {
                            VoucherTypeId = 1,
                            Name = "HollyTopUp",
                            VoucherLength = "15,17"
                        },
                        new
                        {
                            VoucherTypeId = 2,
                            Name = "OTT",
                            VoucherLength = "12"
                        },
                        new
                        {
                            VoucherTypeId = 3,
                            Name = "Flash_OneVoucher",
                            VoucherLength = "16"
                        },
                        new
                        {
                            VoucherTypeId = 4,
                            Name = "BluVoucher",
                            VoucherLength = "16"
                        },
                        new
                        {
                            VoucherTypeId = 5,
                            Name = "EasyLoad",
                            VoucherLength = "14"
                        });
                });

            modelBuilder.Entity("VPS.ControlCenter.Core.Entities.VoucherProvider", b =>
                {
                    b.HasOne("VPS.ControlCenter.Core.Entities.VoucherType", "VoucherType")
                        .WithMany()
                        .HasForeignKey("VoucherTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VoucherType");
                });
#pragma warning restore 612, 618
        }
    }
}
