using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Exceptions;
using Business.Models;
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
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }

    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, int>
    {
        private readonly IGenericRepository<User> _repository;

        public CreateUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var entity = new User();
            entity.Email = request.Email;
            entity.NormalizedEmail = request.Email.ToUpper();
            entity.EmailConfirmed = request.EmailConfirmed;
            entity.UserName = request.UserName;
            entity.NormalizedUserName = request.UserName.ToUpper();
            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.PhoneNumber = request.PhoneNumber;
            entity.PhoneNumberConfirmed = request.PhoneNumberConfirmed;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var ph = new PasswordHasher<User>();
                entity.PasswordHash = ph.HashPassword(entity, request.Password);
            }

            await _repository.CreateAsync(entity, cancellationToken);

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
                    var emailInUse = await repository.AnyAsync(m => m.NormalizedEmail == v.ToUpper(), c);
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
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, null, null, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var number = ((request.Skip ?? 10) / (request.Take ?? 10)) + 1;
            var result = await PaginatedList<User>.CreateAsync(entities, number, request.Take ?? 10, cancellationToken);

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
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var items = await entities.ToArrayAsync(cancellationToken);
            return items;
        }
    }

    #endregion

    #region Update

    public class UpdateUserRequest : IRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }

    public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest>
    {
        private readonly IGenericRepository<User> _repository;

        public UpdateUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null) throw new NotFoundException(nameof(User), request.Id);

            entity.Email = request.Email;
            entity.NormalizedEmail = request.Email.ToUpper();
            entity.EmailConfirmed = request.EmailConfirmed;
            entity.UserName = request.UserName;
            entity.NormalizedUserName = request.UserName.ToUpper();
            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.PhoneNumber = request.PhoneNumber;
            entity.PhoneNumberConfirmed = request.PhoneNumberConfirmed;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var ph = new PasswordHasher<User>();
                entity.PasswordHash = ph.HashPassword(entity, request.Password);
            }

            await _repository.UpdateAsync(entity, cancellationToken);

            return Unit.Value;
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator(IGenericRepository<User> repository)
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => m.Email).NotEmpty();
            RuleFor(m => new {m.Id, m.Email})
                .CustomAsync(async (v, x, c) =>
                {
                    var emailInUse = await repository.AnyAsync(m => m.NormalizedEmail == v.Email.ToUpper() && m.Id != v.Id, c);
                    if (emailInUse) x.AddFailure("Email is already in use");
                });
        }
    }

    #endregion

    #region Delete

    public class DeleteUserRequest : IRequest
    {
        public int Id { get; set; }
    }

    public class DeleteUserRequestHandler : IRequestHandler<DeleteUserRequest>
    {
        private readonly IGenericRepository<User> _repository;

        public DeleteUserRequestHandler(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null) throw new NotFoundException(nameof(User), request.Id);

            await _repository.DeleteAsync(request.Id, cancellationToken);

            return Unit.Value;
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
                    if (await repository.CountAsync(cancellationToken: c) == 1) x.AddFailure("Cannot delete the last user");
                });
        }
    }

    #endregion
}
