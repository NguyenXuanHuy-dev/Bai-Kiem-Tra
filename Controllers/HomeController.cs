using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using baitap.Models;

namespace baitap.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? searchString, int? pageNumber)
    {
        int pageSize = 5;
        var courses = _context.Courses.Include(c => c.Category).AsNoTracking();

        if (!string.IsNullOrEmpty(searchString))
        {
            courses = courses.Where(s => s.Name.Contains(searchString));
            ViewData["CurrentFilter"] = searchString;
        }

        // Get enrolled courses for current student
        var userId = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userId))
        {
            var enrolledIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
            ViewBag.EnrolledCourseIds = enrolledIds;
        }
        else
        {
            ViewBag.EnrolledCourseIds = new List<int>();
        }

        return View(await PaginatedList<Course>.CreateAsync(courses, pageNumber ?? 1, pageSize));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
