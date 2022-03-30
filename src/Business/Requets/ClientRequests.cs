using System;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Models;
using Domain.Abstractions;
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
        //private readonly IdentityDbContext _context;

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

            await _repository.CreateAsync(entity);
            //await _repository.SaveChangesAsync();

            entity.DomainEvents.Add(new DomainEvent<Client>("Create", entity));

            return entity.Id;
        }
    }

    public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
    {
        public CreateClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => m.Identification)
                .NotEmpty()
                .CustomAsync(async (v, x, c) =>
                {
                    var identificationInUse = await repository.AnyAsync(m => m.Identification == v);
                    if (identificationInUse) x.AddFailure("Identification is already in use");
                });
            RuleFor(m => m.FullName).NotEmpty();
        }
    }

    #endregion

    #region Read

    public class ReadPaginatedClientsRequestHandler : IRequestHandler<GenericPaginatedRequest<Client, Client>, PaginatedList<Client>>
    {
        private readonly IGenericRepository<Client> _repository;

        public ReadPaginatedClientsRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<Client>> Handle(GenericPaginatedRequest<Client, Client> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var take = request.Take ?? 10;
            var skip = request.Skip ?? 10;
            var number = (skip / take) + 1;
            var result = await PaginatedList<Client>.CreateAsync(entities, number, request.Take ?? 10);

            return result;
        }
    }

    public class ReadClientsRequestHandler : IRequestHandler<GenericRequest<Client, Client>, Client[]>
    {
        private readonly IGenericRepository<Client> _repository;

        public ReadClientsRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<Client[]> Handle(GenericRequest<Client, Client> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var items = await entities.ToArrayAsync();
            return items;
        }
    }

    #endregion

    #region Update

    public class UpdateClientRequest : CreateClientRequest
    {
        public int Id { get; set; }
    }

    public class UpdateClientRequestHandler : IRequestHandler<UpdateClientRequest, int>
    {
        private readonly IGenericRepository<Client> _repository;
        //private readonly IdentityDbContext _context;

        public UpdateClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(UpdateClientRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("Client not found");

            if (!string.IsNullOrWhiteSpace(request.Identification)) entity.Identification = request.Identification;
            if (!string.IsNullOrWhiteSpace(request.FullName)) entity.FullName = request.FullName;
            entity.Address = request.Address;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Category = request.Category;

            await _repository.UpdateAsync(entity);
            //await _repository.SaveChangesAsync();

            entity.DomainEvents.Add(new DomainEvent<Client>("Update", entity));

            return entity.Id;
        }
    }

    public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
    {
        public UpdateClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => new {m.Id, m.Identification})
                .NotEmpty()
                .CustomAsync(async (v, x, c) =>
                {
                    var identificationInUse = await repository.AnyAsync(m => m.Identification == v.Identification && m.Id != v.Id);
                    if (identificationInUse) x.AddFailure("Identification is already in use");
                });
            RuleFor(m => m.FullName).NotEmpty();
        }
    }

    #endregion

    #region Delete

    public class DeleteClientRequest : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteClientRequestHandler : IRequestHandler<DeleteClientRequest, int>
    {
        private readonly IGenericRepository<Client> _repository;
        //private readonly IdentityDbContext _context;

        public DeleteClientRequestHandler(IGenericRepository<Client> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(DeleteClientRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("Client not found");

            await _repository.DeleteAsync(request.Id);
            //await _repository.SaveChangesAsync();

            //entity.DomainEvents.Add(new DomainEvent<User>("Delete", entity));

            return entity.Id;
        }
    }

    public class DeleteClientRequestValidator : AbstractValidator<DeleteClientRequest>
    {
        public DeleteClientRequestValidator(IGenericRepository<Client> repository)
        {
            RuleFor(m => m.Id)
                .NotEmpty();
        }
    }

    #endregion
}
