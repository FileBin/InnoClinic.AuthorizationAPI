using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;

namespace Filebin.AuthServer.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string> {
    public ApplicationDbContext(DbContextOptions options) : base(options) {
        Database.EnsureCreated();
    }
}