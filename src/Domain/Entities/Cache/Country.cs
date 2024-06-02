using Domain.Interfaces;

namespace Domain.Entities.Cache
{
    public class Country : IEntity
    {
        public string Name { get; set; }
        public string ISO3166 { get; set; }
        public int Id { get; set; }
    }
}
