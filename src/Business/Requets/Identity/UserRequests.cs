using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Models;
using Domain.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Business.Requests.Identity
{
    #region Create

    public class CreateUserRequest : IRequest<int>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
    }

    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, int>
    {
        private readonly IGenericRepository<User> _repository;
        //private readonly IdentityDbContext _context;

        public CreateUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var entity = new User
            {
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                UserName = request.UserName,
                NormalizedUserName = request.UserName.ToUpper(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
            };

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var ph = new PasswordHasher<User>();
                entity.PasswordHash = ph.HashPassword(entity, request.Password);
            }

            await _repository.CreateAsync(entity);
            //await _repository.SaveChangesAsync();

            entity.DomainEvents.Add(new DomainEvent<User>("Create", entity));

            return entity.Id;
        }
    }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator(IGenericRepository<User> repository)
        {
            RuleFor(m => m.Email)
                .NotEmpty()
                .CustomAsync(async (v, x, c) =>
                {
                    var emailInUse = await repository.AnyAsync(m => m.NormalizedEmail == v.ToUpper());
                    if (emailInUse) x.AddFailure("Email is already in use");
                });
        }
    }

    #endregion

    #region Read

    public class ReadPaginatedUsersRequestHandler : IRequestHandler<GenericPaginatedRequest<User, User>, PaginatedList<User>>
    {
        private readonly IGenericRepository<User> _repository;

        public ReadPaginatedUsersRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<User>> Handle(GenericPaginatedRequest<User, User> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, null,null, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var take = request.Take ?? 10;
            var skip = request.Skip ?? 10;
            var number = (skip / take) + 1;
            var result = await PaginatedList<User>.CreateAsync(entities, number, request.Take ?? 10);

            return result;
        }
    }

    public class ReadUsersRequestHandler : IRequestHandler<GenericRequest<User, User>, User[]>
    {
        private readonly IGenericRepository<User> _repository;

        public ReadUsersRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<User[]> Handle(GenericRequest<User, User> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var items = await entities.ToArrayAsync();
            return items;
        }
    }

    #endregion

    #region Update

    public class UpdateUserRequest : CreateUserRequest
    {
        public int Id { get; set; }
    }

    public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest, int>
    {
        private readonly IGenericRepository<User> _repository;
        //private readonly IdentityDbContext _context;

        public UpdateUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(request.Email)) entity.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.FirstName)) entity.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName)) entity.LastName = request.LastName;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) entity.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var ph = new PasswordHasher<User>();
                entity.PasswordHash = ph.HashPassword(entity, request.Password);
            }

            await _repository.UpdateAsync(entity);
            //await _repository.SaveChangesAsync();

            entity.DomainEvents.Add(new DomainEvent<User>("Update", entity));

            return entity.Id;
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator(IGenericRepository<User> repository)
        {
            RuleFor(m => new {m.Id, m.Email})
                .CustomAsync(async (v, x, c) =>
                {
                    var emailInUse = await repository.AnyAsync(m => m.NormalizedEmail == v.Email.ToUpper() && m.Id != v.Id);
                    if (emailInUse) x.AddFailure("Email is already in use");
                });
        }
    }

    #endregion

    #region Delete

    public class DeleteUserRequest : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteUserRequestHandler : IRequestHandler<DeleteUserRequest, int>
    {
        private readonly IGenericRepository<User> _repository;
        //private readonly IdentityDbContext _context;

        public DeleteUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("User not found");

            await _repository.DeleteAsync(request.Id);
            //await _repository.SaveChangesAsync();

            entity.DomainEvents.Add(new DomainEvent<User>("Delete", entity));

            return entity.Id;
        }
    }

    public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator(IGenericRepository<User> repository)
        {
            RuleFor(m => m.Id)
                .NotEmpty()
                .CustomAsync(async (v, x, c) =>
                {
                    if (v == 1) x.AddFailure("Cannot delete the default user");
                    if (await repository.CountAsync() == 1) x.AddFailure("Cannot delete the last user");
                });
        }
    }

    #endregion
}
