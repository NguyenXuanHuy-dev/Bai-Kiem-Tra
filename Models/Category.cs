using System.Collections.Generic;

namespace baitap.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
