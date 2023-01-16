using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeniorProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MASTER_BANK> MASTER_BANK { get; set; }
        public DbSet<MASTER_FACULTY> MASTER_FACULTY { get; set; }
        public DbSet<MASTER_BRANCH> MASTER_BRANCH { get; set; }
        public DbSet<MASTER_STATUS> MASTER_STATUS { get; set; }
        public DbSet<MASTER_TYPEJOB> MASTER_TYPEJOB { get; set; }
        public DbSet<MASTER_ROLE> MASTER_ROLE { get; set; }
        public DbSet<TRANSACTION_JOB> TRANSACTION_JOB { get; set; }
        public DbSet<TRANSACTION_REGISTER> TRANSACTION_REGISTER { get; set; }
    }
}
