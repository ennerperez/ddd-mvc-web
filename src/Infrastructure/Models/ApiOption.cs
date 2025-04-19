using System;

namespace Infrastructure.Models
{
    public class ApiOption
    {
        public Uri BaseAddress { get; set; }
        public Type ContentSerializer { get; set; }
    }
}
