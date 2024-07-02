using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wkkim.Blog.Web.Data
{
    public class AuthDBContext : IdentityDbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var adminRoleId = "1bff0252-3c4c-4a6b-a5e5-1107b84fc858";
            var superAdminRoleId = "69e918f0-f3ff-40a7-960e-b19259f6b6db";
            var userRoleId = "800ef519-39a6-4f01-bc1f-0559259cb26c";

            // Seed Role (User Admin SuperAdmin)
            var roles = new List<IdentityRole>
             {
                 new IdentityRole
                 {
                     Name="Admin",
                     NormalizedName="Admin",
                     Id= adminRoleId,
                     ConcurrencyStamp = adminRoleId
                 },
                 new IdentityRole
                 {
                     Name="SuperAdmin",
                     NormalizedName ="SuperAdmin",
                     Id=superAdminRoleId,
                     ConcurrencyStamp=superAdminRoleId
                 },
                 new IdentityRole
                 {
                     Name = "User",
                     NormalizedName = "User",
                     Id=userRoleId,
                     ConcurrencyStamp = userRoleId
                 }
             };

            builder.Entity<IdentityRole>().HasData(roles);

            // Seed SuperAdminUser
            var superAdminId = "2fcf4ccb-0941-4e65-906f-9dd3e326299f";
            var superAdminUser = new IdentityUser
            {
                UserName = "superAdmin@wkkimBlog.com",
                Email = "superAdmin@wkkimBlog.com",
                NormalizedEmail = "superAdmin@wkkimBlog.com".ToUpper(),
                NormalizedUserName = "superAdmin@wkkimBlog.com".ToUpper(),
                Id = superAdminId
            };

            superAdminUser.PasswordHash = new PasswordHasher<IdentityUser>()
                .HashPassword(superAdminUser, "superAdmin@wkkimBlog.com123");

            builder.Entity<IdentityUser>().HasData(superAdminUser);

            // Add All roles to SuperAdminUser
            var superAdminRoles = new List<IdentityUserRole<string>>
 {
     new IdentityUserRole<string>
     {
         RoleId = adminRoleId,
         UserId = superAdminId
     },
     new IdentityUserRole<string>
     {
         RoleId = superAdminRoleId,
         UserId = superAdminId
     },
     new IdentityUserRole<string>
     {
         RoleId = userRoleId,
         UserId = superAdminId
     },
 };

            builder.Entity<IdentityUserRole<string>>().HasData(superAdminRoles);
        }
    }
}
