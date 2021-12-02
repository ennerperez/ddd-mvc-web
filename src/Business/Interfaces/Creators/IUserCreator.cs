using System.Threading.Tasks;
using Domain.Entities;

namespace Business.Interfaces.Creators
{
    public interface IUserCreator: ICreator<User>
    {
        Task<User> CreateAsync(User account);
    }
}
