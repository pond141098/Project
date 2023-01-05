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
    }
}
