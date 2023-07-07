using Domain.Interfaces;

namespace Domain.Entities.Cache
{
    public class Country : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ISO3166 { get; set; }
    }
}
