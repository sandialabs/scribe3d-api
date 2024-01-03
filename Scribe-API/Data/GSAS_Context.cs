using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSAS_Web.Data
{
    public class GSAS_Context : IdentityDbContext<AppUser>
    {
        public GSAS_Context(DbContextOptions<GSAS_Context> options)
            : base(options)
        {
        }

        public DbSet<ToolFile> ToolFile { get; set; }
        public DbSet<Request> Request { get; set; }
        public DbSet<RequestTool> RequestTools { get; set; } 
        public DbSet<Tool> Tool { get; set; }
        public DbSet<UserTool> UserTools { get; set; }
        public DbSet<ScribeSession> ScribeSession { get; set; }
        public DbSet<ScribeProposedChange> ScribeProposedChange { get; set; }
        public DbSet<ScribeMessage> ScribeMessage { get; set; }
    }
} 