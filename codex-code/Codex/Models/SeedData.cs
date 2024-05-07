using Codex.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Codex.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if there is at least one role in the database
                // meaning that the code has been executed
                // Therefore, we return to prevent inserting roles again
                // This method should be executed only once
                if (context.Roles.Any())
                {
                    return; // the database already contains roles
                }

                // CREATING ROLES IN THE DATABASE
                // if it doesn't contain roles, they will be created
                // the IDs (Id, RoleId, UserId) were generated using: https://guidgenerator.com/
                context.Roles.AddRange(
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() }
                );

                // a new instance that we will use for creating user passwords
                // passwords are hashed
                var hasher = new PasswordHasher<ApplicationUser>();

                // CREATING USERS IN THE DATABASE
                // One user is created for each role
                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // primary key
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // primary key
                        UserName = "editor@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "EDITOR@TEST.COM",
                        Email = "editor@test.com",
                        NormalizedUserName = "EDITOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Editor1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2", // primary key
                        UserName = "user@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User1!")
                    }
                );

                // USER-ROLE ASSOCIATION
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    }
                );

                context.SaveChanges();
            }
        }

    }
}
