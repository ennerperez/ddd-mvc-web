using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Exceptions;
using Business.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Business.Requests
{
    #region Create

    public class CreateClientRequest : IRequest<int>
    {
        public string Identification { get; set; }
        public string Category { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class CreateClientRequestHandler : IRequestHandler<CreateClientRequest, int>
    {
        private readonly IGenericRepository<Client> _repository;

        public CreateClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateClientRequest request, CancellationToken cancellationToken)
        {
            var entity = new Client
            {
                Identification = request.Identification,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Category = request.Category
            };

            await _repository.CreateAsync(entity, cancellationToken);

            return entity.Id;
        }
    }

    public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
    {
        public CreateClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => m.Identification).NotEmpty();
            RuleFor(m => m.FullName).NotEmpty();
            RuleFor(m => new { m.Identification })
                .CustomAsync(async (m, v, c) =>
                {
                    var isInUse = await repository.AnyAsync(p => p.Identification == m.Identification, c);
                    if (isInUse)
                    {
                        v.AddFailure("Identification is already in use");
                    }
                });
        }
    }

    #endregion

    #region Read

    public class ReadPaginatedClientRequestHandler : IRequestHandler<PaginatedRequest<Client, Client>, PaginatedList<Client>>
    {
        private readonly IGenericRepository<Client> _repository;

        public ReadPaginatedClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<Client>> Handle(PaginatedRequest<Client, Client> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, null, null, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var number = ((request.Skip ?? 10) / (request.Take ?? 10)) + 1;
            var result = await PaginatedList<Client>.CreateAsync(entities, number, request.Take ?? 10, cancellationToken);

            return result;
        }
    }

    public class ReadClientRequestHandler : IRequestHandler<RepositoryRequest<Client, Client>, Client[]>
    {
        private readonly IGenericRepository<Client> _repository;

        public ReadClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<Client[]> Handle(RepositoryRequest<Client, Client> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var items = await entities.ToArrayAsync(cancellationToken);
            return items;
        }
    }

    #endregion

    #region Update

    public class UpdateClientRequest : IRequest
    {
        public int Id { get; set; }
        public string Identification { get; set; }
        public string Category { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class UpdateClientRequestHandler : IRequestHandler<UpdateClientRequest>
    {
        private readonly IGenericRepository<Client> _repository;

        public UpdateClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpdateClientRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(Client), request.Id);
            entity.Identification = request.Identification;
            entity.FullName = request.FullName;
            entity.Address = request.Address;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Category = request.Category;

            await _repository.UpdateAsync(entity, cancellationToken);
        }
    }

    public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
    {
        public UpdateClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => m.Identification).NotEmpty();
            RuleFor(m => m.FullName).NotEmpty();
            RuleFor(m => new { m.Id, m.Identification })
                .CustomAsync(async (m, v, c) =>
                {
                    var isInUse = await repository.AnyAsync(p => p.Identification == m.Identification && p.Id != m.Id, c);
                    if (isInUse)
                    {
                        v.AddFailure("Identification is already in use");
                    }
                });
        }
    }

    /* PARTIAL */

    public class PartialUpdateClientRequest : IRequest
    {
        public int Id { get; set; }
        public string Identification { get; set; }
        public string Category { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class PartialUpdateClientRequestHandler : IRequestHandler<PartialUpdateClientRequest>
    {
        private readonly IGenericRepository<Client> _repository;

        public PartialUpdateClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task Handle(PartialUpdateClientRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(Client), request.Id);
            if (string.IsNullOrWhiteSpace(entity.Identification))
            {
                entity.Identification = request.Identification;
            }

            if (string.IsNullOrWhiteSpace(entity.FullName))
            {
                entity.FullName = request.FullName;
            }

            if (string.IsNullOrWhiteSpace(entity.Address))
            {
                entity.Address = request.Address;
            }

            if (string.IsNullOrWhiteSpace(entity.PhoneNumber))
            {
                entity.PhoneNumber = request.PhoneNumber;
            }

            if (string.IsNullOrWhiteSpace(entity.Category))
            {
                entity.Category = request.Category;
            }

            await _repository.UpdateAsync(entity, cancellationToken);
        }
    }

    public class PartialUpdateClientRequestValidator : AbstractValidator<PartialUpdateClientRequest>
    {
        public PartialUpdateClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => new { m.Id, m.Identification })
                .CustomAsync(async (m, v, c) =>
                {
                    if (m.Identification != null)
                    {
                        var isInUse = await repository.AnyAsync(p => p.Identification == m.Identification && p.Id != m.Id, c);
                        if (isInUse)
                        {
                            v.AddFailure("Identification is already in use");
                        }
                    }
                });
        }
    }

    #endregion

    #region Delete

    public class DeleteClientRequest : IRequest
    {
        public int Id { get; set; }
    }

    public class DeleteClientRequestHandler : IRequestHandler<DeleteClientRequest>
    {
        private readonly IGenericRepository<Client> _repository;

        public DeleteClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteClientRequest request, CancellationToken cancellationToken)
        {
            _ = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken) ?? throw new NotFoundException(nameof(Client), request.Id);
            await _repository.DeleteAsync(request.Id, cancellationToken);
        }
    }

    public class DeleteClientRequestValidator : AbstractValidator<DeleteClientRequest>
    {
        public DeleteClientRequestValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
        }
    }

    #endregion
}
