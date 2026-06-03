using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace baitap.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roleNames = { "ADMIN", "STUDENT" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@course.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(user, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "ADMIN");
                }
            }
            else
            {
                // Ensure existing admin user has the uppercase ADMIN role
                if (!await userManager.IsInRoleAsync(adminUser, "ADMIN"))
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                }
            }

            // Migrate all existing users in the database to uppercase roles if they had lowercase ones
            var allUsers = userManager.Users.ToList();
            foreach (var u in allUsers)
            {
                var currentRoles = await userManager.GetRolesAsync(u);
                
                if (currentRoles.Contains("Admin") && !currentRoles.Contains("ADMIN"))
                {
                    await userManager.AddToRoleAsync(u, "ADMIN");
                }
                
                if (currentRoles.Contains("Student") && !currentRoles.Contains("STUDENT"))
                {
                    await userManager.AddToRoleAsync(u, "STUDENT");
                }

                // If user has no roles, assign STUDENT
                if (currentRoles.Count == 0)
                {
                    await userManager.AddToRoleAsync(u, "STUDENT");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Công nghệ thông tin" },
                    new Category { Name = "Kinh tế" },
                    new Category { Name = "Ngoại ngữ" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Courses
            if (!context.Courses.Any())
            {
                var itCat = context.Categories.FirstOrDefault(c => c.Name == "Công nghệ thông tin")?.Id ?? 1;
                var bizCat = context.Categories.FirstOrDefault(c => c.Name == "Kinh tế")?.Id ?? 2;
                var langCat = context.Categories.FirstOrDefault(c => c.Name == "Ngoại ngữ")?.Id ?? 3;

                context.Courses.AddRange(
                    new Course
                    {
                        Name = "Lập trình Web ASP.NET Core",
                        Credits = 3,
                        Lecturer = "ThS. Nguyễn Văn A",
                        Image = "/images/aspnet.jpg",
                        CategoryId = itCat
                    },
                    new Course
                    {
                        Name = "Cấu trúc dữ liệu và Giải thuật",
                        Credits = 4,
                        Lecturer = "TS. Trần Văn B",
                        Image = "/images/dsa.jpg",
                        CategoryId = itCat
                    },
                    new Course
                    {
                        Name = "Kinh tế vĩ mô",
                        Credits = 3,
                        Lecturer = "ThS. Lê Thị C",
                        Image = "/images/macroeconomics.jpg",
                        CategoryId = bizCat
                    },
                    new Course
                    {
                        Name = "Tiếng Anh chuyên ngành CNTT",
                        Credits = 2,
                        Lecturer = "Cô Phạm Thị D",
                        Image = "/images/english.jpg",
                        CategoryId = langCat
                    },
                    new Course
                    {
                        Name = "Trí tuệ nhân tạo (AI)",
                        Credits = 4,
                        Lecturer = "GS. Nguyễn Đức E",
                        Image = "/images/ai.jpg",
                        CategoryId = itCat
                    },
                    new Course
                    {
                        Name = "Quản trị doanh nghiệp",
                        Credits = 3,
                        Lecturer = "TS. Hoàng Văn F",
                        Image = "/images/management.jpg",
                        CategoryId = bizCat
                    },
                    new Course
                    {
                        Name = "Lập trình di động Flutter",
                        Credits = 3,
                        Lecturer = "ThS. Vũ Minh G",
                        Image = "/images/flutter.jpg",
                        CategoryId = itCat
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
