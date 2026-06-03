using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using baitap.Models;

namespace baitap.Controllers
{
    [Authorize(Roles = "STUDENT,Student")]
    public class EnrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Enroll/EnrollCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (!isEnrolled)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrollDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Enroll/CancelCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelCourse(int courseId, string? redirectUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
                return Redirect(redirectUrl);

            return RedirectToAction(nameof(MyCourses));
        }

        // GET: /Enroll/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Category)
                .Select(e => e.Course)
                .ToListAsync();

            return View(enrolledCourses);
        }
    }
}
