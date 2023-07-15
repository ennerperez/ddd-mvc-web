using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Exceptions;
using Business.Models;
using Domain.Entities.Identity;
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
		public string GivenName { get; set; }
		public string Surname { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public bool EmailConfirmed { get; set; }
		public string PhoneNumber { get; set; }
		public bool PhoneNumberConfirmed { get; set; }
		public bool TwoFactorEnabled { get; set; }

		public string[] Roles { get; set; }
	}

	public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, int>
	{
		private readonly IGenericRepository<User> _repository;
		private readonly UserManager<User> _userManager;

		public CreateUserRequestHandler(IGenericRepository<User> repository, UserManager<User> userManager)
		{
			_repository = repository;
			_userManager = userManager;
		}

		public async Task<int> Handle(CreateUserRequest request, CancellationToken cancellationToken)
		{
			var entity = new User();
			entity.Email = request.Email;
			entity.NormalizedEmail = request.Email.ToUpper();
			entity.EmailConfirmed = request.EmailConfirmed;
			entity.UserName = request.UserName;
			entity.NormalizedUserName = request.UserName.ToUpper();
			entity.GivenName = request.GivenName;
			entity.Surname = request.Surname;
			entity.PhoneNumber = request.PhoneNumber;
			entity.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
			entity.TwoFactorEnabled = request.TwoFactorEnabled;
			entity.SecurityStamp = Guid.NewGuid().ToString();

			entity.UserClaims = new List<UserClaim>();
			entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.GivenName, request.GivenName));
			entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.Surname, request.Surname));

			if (!string.IsNullOrWhiteSpace(request.Password))
			{
				var ph = new PasswordHasher<User>();
				entity.PasswordHash = ph.HashPassword(entity, request.Password);
			}

			await _repository.CreateAsync(entity, cancellationToken);

			#region Roles

			if (request.Roles == null) request.Roles = Array.Empty<string>();
			var tba = request.Roles;
			if (tba.Any()) await _userManager.AddToRolesAsync(entity, tba);

			#endregion

			return entity.Id;
		}
	}

	public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
	{
		public CreateUserRequestValidator(IGenericRepository<User> repository)
		{
			RuleFor(m => m.Email).NotEmpty();
			RuleFor(m => new {m.Email})
				.CustomAsync(async (m, v, c) =>
				{
					var isInUse = await repository.AnyAsync(p => p.NormalizedEmail == m.Email.ToUpper(), c);
					if (isInUse) v.AddFailure("Email is already in use");
				});
		}
	}

	#endregion

	#region Read

	public class ReadPaginatedUserRequestHandler : IRequestHandler<PaginatedRequest<User, User>, PaginatedList<User>>
	{
		private readonly IGenericRepository<User> _repository;

		public ReadPaginatedUserRequestHandler(IGenericRepository<User> repository)
		{
			_repository = repository;
		}

		public async Task<PaginatedList<User>> Handle(PaginatedRequest<User, User> request, CancellationToken cancellationToken)
		{
			var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, null, null, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
			var number = ((request.Skip ?? 10) / (request.Take ?? 10)) + 1;
			var result = await PaginatedList<User>.CreateAsync(entities, number, request.Take ?? 10, cancellationToken);

			return result;
		}
	}

	public class ReadUserRequestHandler : IRequestHandler<RepositoryRequest<User, User>, User[]>
	{
		private readonly IGenericRepository<User> _repository;

		public ReadUserRequestHandler(IGenericRepository<User> repository)
		{
			_repository = repository;
		}

		public async Task<User[]> Handle(RepositoryRequest<User, User> request, CancellationToken cancellationToken)
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
		public string GivenName { get; set; }
		public string Surname { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public bool EmailConfirmed { get; set; }
		public string PhoneNumber { get; set; }
		public bool PhoneNumberConfirmed { get; set; }
		public bool TwoFactorEnabled { get; set; }

		public string[] Roles { get; set; }
	}

	public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest>
	{
		private readonly IGenericRepository<User> _repository;
		private readonly UserManager<User> _userManager;

		public UpdateUserRequestHandler(IGenericRepository<User> repository, UserManager<User> userManager)
		{
			_repository = repository;
			_userManager = userManager;
		}

		public async Task Handle(UpdateUserRequest request, CancellationToken cancellationToken)
		{
			var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, include: i => i.Include(m => m.UserClaims), cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(User), request.Id);

			entity.Email = request.Email;
			entity.NormalizedEmail = request.Email.ToUpper();
			entity.EmailConfirmed = request.EmailConfirmed;
			entity.UserName = request.UserName;
			entity.NormalizedUserName = request.UserName.ToUpper();
			entity.GivenName = request.GivenName;
			entity.Surname = request.Surname;
			entity.PhoneNumber = request.PhoneNumber;
			entity.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
			entity.TwoFactorEnabled = request.TwoFactorEnabled;

			if (!string.IsNullOrWhiteSpace(request.Password))
			{
				var ph = new PasswordHasher<User>();
				entity.PasswordHash = ph.HashPassword(entity, request.Password);
			}

			#region Claims

			if (entity.UserClaims == null || !entity.UserClaims.Any()) entity.UserClaims = new List<UserClaim>();

			var givenName = entity.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.GivenName);
			if (givenName != null)
				givenName.ClaimValue = request.GivenName;
			else
				entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.GivenName, request.GivenName));

			var surname = entity.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.Surname);
			if (surname != null)
				surname.ClaimValue = request.Surname;
			else
				entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.Surname, request.Surname));

			#endregion

			await _repository.UpdateAsync(entity, cancellationToken);

			#region Roles

			var currentRoles = await _userManager.GetRolesAsync(entity);
			if (request.Roles == null) request.Roles = Array.Empty<string>();
			var tbr = currentRoles.Where(m => !request.Roles.Contains(m)).ToArray();
			if (tbr.Any()) await _userManager.RemoveFromRolesAsync(entity, tbr);
			var tba = request.Roles.Where(m => !currentRoles.Contains(m)).ToArray();
			if (tba.Any()) await _userManager.AddToRolesAsync(entity, tba);

			#endregion

		}
	}

	public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
	{
		public UpdateUserRequestValidator(IGenericRepository<User> repository)
		{
			RuleFor(m => m.Id).NotEmpty();
			RuleFor(m => m.Email).NotEmpty();
			RuleFor(m => new {m.Id, m.Email})
				.CustomAsync(async (m, v, c) =>
				{
					var isInUse = await repository.AnyAsync(p => p.NormalizedEmail == m.Email.ToUpper() && p.Id != m.Id, c);
					if (isInUse) v.AddFailure("Email is already in use");
				});
		}
	}

/* PARTIAL */

	public class PartialUpdateUserRequest : IRequest
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string GivenName { get; set; }
		public string Surname { get; set; }
		public string PhoneNumber { get; set; }
		public string UserName { get; set; }
		public bool? EmailConfirmed { get; set; }
		public bool? PhoneNumberConfirmed { get; set; }
		public bool? TwoFactorEnabled { get; set; }

		public string[] Roles { get; set; }
	}

	public class PartialUpdateUserRequestHandler : IRequestHandler<PartialUpdateUserRequest>
	{
		private readonly IGenericRepository<User> _repository;
		private readonly UserManager<User> _userManager;

		public PartialUpdateUserRequestHandler(IGenericRepository<User> repository, UserManager<User> userManager)
		{
			_repository = repository;
			_userManager = userManager;
		}

		public async Task Handle(PartialUpdateUserRequest request, CancellationToken cancellationToken)
		{
			var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, include: i => i.Include(m => m.UserClaims), cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(User), request.Id);

			if (request.Email != null)
			{
				entity.Email = request.Email;
				entity.NormalizedEmail = request.Email.ToUpper();
			}

			if (request.EmailConfirmed != null) entity.EmailConfirmed = request.EmailConfirmed.Value;
			if (request.UserName != null)
			{
				entity.UserName = request.UserName;
				entity.NormalizedUserName = request.UserName.ToUpper();
			}

			if (request.GivenName != null) entity.GivenName = request.GivenName;
			if (request.Surname != null) entity.Surname = request.Surname;
			if (request.PhoneNumber != null) entity.PhoneNumber = request.PhoneNumber;
			if (request.PhoneNumberConfirmed != null) entity.PhoneNumberConfirmed = request.PhoneNumberConfirmed.Value;
			if (request.TwoFactorEnabled != null) entity.TwoFactorEnabled = request.TwoFactorEnabled.Value;

			if (!string.IsNullOrWhiteSpace(request.Password))
			{
				var ph = new PasswordHasher<User>();
				entity.PasswordHash = ph.HashPassword(entity, request.Password);
			}

			#region Claims

			if (entity.UserClaims == null || !entity.UserClaims.Any()) entity.UserClaims = new List<UserClaim>();
			if (request.GivenName != null)
			{
				var givenName = entity.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.GivenName);
				if (givenName != null)
					givenName.ClaimValue = request.GivenName;
				else
					entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.GivenName, request.GivenName));
			}

			if (request.Surname != null)
			{
				var surname = entity.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.Surname);
				if (surname != null)
					surname.ClaimValue = request.Surname;
				else
					entity.UserClaims.Add(new UserClaim(System.Security.Claims.ClaimTypes.Surname, request.Surname));
			}

			#endregion

			await _repository.UpdateAsync(entity, cancellationToken);

			#region Roles

			var currentRoles = await _userManager.GetRolesAsync(entity);
			if (request.Roles == null) request.Roles = Array.Empty<string>();
			var tbr = currentRoles.Where(m => !request.Roles.Contains(m)).ToArray();
			if (tbr.Any()) await _userManager.RemoveFromRolesAsync(entity, tbr);
			var tba = request.Roles.Where(m => !currentRoles.Contains(m)).ToArray();
			if (tba.Any()) await _userManager.AddToRolesAsync(entity, tba);

			#endregion

		}
	}

	public class PartialUpdateUserRequestValidator : AbstractValidator<PartialUpdateUserRequest>
	{
		public PartialUpdateUserRequestValidator(IGenericRepository<User> repository)
		{
			RuleFor(m => m.Id).NotEmpty();
			RuleFor(m => new {m.Id, m.Email})
				.CustomAsync(async (m, v, c) =>
				{
					if (m.Email != null)
					{
						var isInUse = await repository.AnyAsync(p => p.NormalizedEmail == m.Email.ToUpper() && p.Id != m.Id, c);
						if (isInUse) v.AddFailure("Email is already in use");
					}
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

		public async Task Handle(DeleteUserRequest request, CancellationToken cancellationToken)
		{
			var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(User), request.Id);

			await _repository.DeleteAsync(request.Id, cancellationToken);

		}
	}

	public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
	{
		public DeleteUserRequestValidator(IGenericRepository<User> repository)
		{
			RuleFor(m => m.Id).NotEmpty();
			RuleFor(m => new {m.Id})
				.CustomAsync(async (m, v, c) =>
				{
					if (m.Id == 1) v.AddFailure("Cannot delete the default user");
					if (await repository.CountAsync(cancellationToken: c) == 1) v.AddFailure("Cannot delete the last user");
				});
		}
	}

	#endregion
}
