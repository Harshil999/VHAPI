﻿using Microsoft.EntityFrameworkCore;
using static VHEmpAPI.Shared.CommonProcOutputFields;
using System.ComponentModel.DataAnnotations.Schema;
using VHEmpAPI.Shared;

namespace VHEmpAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

       
        [NotMapped]
        public DbSet<IsValidData> IsValidData { get; set; }

        [NotMapped]
        public DbSet<OTP> OTP { get; set; }

        [NotMapped]
        public DbSet<TokenData> TokenData { get; set; }

        [NotMapped]
        public DbSet<LoginId_TokenData> LoginId_TokenData { get; set; }

        [NotMapped]
        public DbSet<IsValidToken> IsValidToken { get; set; }

        [NotMapped]
        public DbSet<DashBoardList>? DashboardList { get; set; }
        
        [NotMapped]
        public DbSet<Ddl_Value_Nm>? Ddl_Value_Nm { get; set; }

        [NotMapped]
        public DbSet<Resp_MispunchDtl_EmpInfo>? Resp_MispunchDtl_EmpInfo { get; set; }

        [NotMapped]
        public DbSet<Resp_AttDtl_EmpInfo>? Resp_AttDtl_EmpInfo { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Ignore<Organization>();
            //modelBuilder.Ignore<Floor>();
            //modelBuilder.Ignore<Ward>();
        }
    }
}
