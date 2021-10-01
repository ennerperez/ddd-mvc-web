using System;

namespace Web.Models
{
    public class AboutViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public DateTime Published { get; set; }
        public string Color { get; set; }
        public bool Dependency { get; set; }
    }
}
