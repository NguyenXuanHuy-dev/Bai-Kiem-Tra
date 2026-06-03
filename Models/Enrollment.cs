using System;
using Microsoft.AspNetCore.Identity;

namespace baitap.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; } = DateTime.Now;

        public IdentityUser? User { get; set; }
        public Course? Course { get; set; }
    }
}
