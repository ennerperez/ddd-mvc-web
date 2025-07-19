using Domain.Abstractions;

namespace Domain.Entities.Cache
{
    public class Country : Entity<int>
    {
        public string Name { get; set; }

        // ReSharper disable once InconsistentNaming
        public string ISO3166 { get; set; }
    }
}
