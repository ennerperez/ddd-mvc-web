using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface IValidator<T> where T : class
    {
        Task ValidateAsync(T entity);
    }
}
