using Business.Models;
using MediatR;

namespace Business.Interfaces
{
    public interface IPaginatedRequest<TResult>: IRequest<PaginatedList<TResult>>
    {
    }
}
