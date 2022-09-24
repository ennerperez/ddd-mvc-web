using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Exceptions;
using Business.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Business.Requests
{
	#region Create

	public class CreateBudgetRequest : IRequest<Guid>
	{
		public string Code { get; set; }

		public int ClientId { get; set; }

		public decimal Subtotal { get; set; }
		public decimal Taxes { get; set; }
		public decimal Total { get; set; }
	}

	public class CreateBudgetRequestHandler : IRequestHandler<CreateBudgetRequest, Guid>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;
		private readonly IIdentityService _identityService;

		public CreateBudgetRequestHandler(IGenericRepository<Budget, Guid> repository, IIdentityService identityService)
		{
			_repository = repository;
			_identityService = identityService;
		}

		public async Task<Guid> Handle(CreateBudgetRequest request, CancellationToken cancellationToken)
		{
			var entity = new Budget();

			entity.Code = request.Code;
			entity.ClientId = request.ClientId;
			entity.Subtotal = request.Subtotal;
			entity.Taxes = request.Taxes;
			entity.Total = request.Total;

			entity.CreatedById = int.Parse(_identityService.User.GetUserId());

			await _repository.CreateAsync(entity);

			return entity.Id;
		}
	}

	public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
	{
		public CreateBudgetRequestValidator(IGenericRepository<Budget, Guid> repository)
		{
			RuleFor(m => m.Code).NotEmpty();
			RuleFor(m => new { m.Subtotal, m.Taxes, m.Total })
				.Custom((m, v) =>
				{
					if (m.Subtotal + m.Taxes != m.Total)
						v.AddFailure("Total is miscalculated");
				});
			RuleFor(m => new { m.Code })
				.CustomAsync(async (m, v, c) =>
				{
					var isInUse = await repository.AnyAsync(p => p.Code == m.Code, c);
					if (isInUse) v.AddFailure("Code is already in use");
				});
		}
	}

	#endregion

	#region Read

	public class ReadPaginatedBudgetsRequestHandler : IRequestHandler<PaginatedRequest<Budget, Guid, Budget>, PaginatedList<Budget>>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;

		public ReadPaginatedBudgetsRequestHandler(IGenericRepository<Budget, Guid> repository)
		{
			_repository = repository;
		}

		public async Task<PaginatedList<Budget>> Handle(PaginatedRequest<Budget, Guid, Budget> request, CancellationToken cancellationToken)
		{
			var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
			var number = ((request.Skip ?? 10) / (request.Take ?? 10)) + 1;
			var result = await PaginatedList<Budget>.CreateAsync(entities, number, request.Take ?? 10, cancellationToken);

			return result;
		}
	}

	public class ReadBudgetsRequestHandler : IRequestHandler<GenericRequest<Budget, Guid, Budget>, Budget[]>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;

		public ReadBudgetsRequestHandler(IGenericRepository<Budget, Guid> repository)
		{
			_repository = repository;
		}

		public async Task<Budget[]> Handle(GenericRequest<Budget, Guid, Budget> request, CancellationToken cancellationToken)
		{
			var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
			var items = await entities.ToArrayAsync(cancellationToken);
			return items;
		}
	}

	#endregion

	#region Update

	public class UpdateBudgetRequest : IRequest
	{
		public Guid Id { get; set; }

		public string Code { get; set; }

		public int ClientId { get; set; }
		public States State { get; set; }

		public decimal Subtotal { get; set; }
		public decimal Taxes { get; set; }
		public decimal Total { get; set; }
	}

	public class UpdateBudgetRequestHandler : IRequestHandler<UpdateBudgetRequest>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;
		private readonly IIdentityService _identityService;

		public UpdateBudgetRequestHandler(IGenericRepository<Budget, Guid> repository, IIdentityService identityService)
		{
			_repository = repository;
			_identityService = identityService;
		}

		public async Task<Unit> Handle(UpdateBudgetRequest request, CancellationToken cancellationToken)
		{
			var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(Budget), request.Id);

			entity.Code = request.Code;
			entity.ClientId = request.ClientId;
			entity.State = request.State;
			entity.Subtotal = request.Subtotal;
			entity.Taxes = request.Taxes;
			entity.Total = request.Total;

			entity.ModifiedAt = DateTime.Now;
			entity.ModifiedById = int.Parse(_identityService.User.GetUserId());

			await _repository.UpdateAsync(entity, cancellationToken);

			return Unit.Value;
		}
	}

	public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
	{
		public UpdateBudgetRequestValidator(IGenericRepository<Budget, Guid> repository)
		{
			RuleFor(m => m.Id).NotEmpty();
			RuleFor(m => m.Code).NotEmpty();
			RuleFor(m => new { m.Subtotal, m.Taxes, m.Total })
				.Custom((m, v) =>
				{
					if (m.Subtotal + m.Taxes != m.Total)
						v.AddFailure("Total is miscalculated");
				});
			RuleFor(m => new { m.Id, m.Code })
				.CustomAsync(async (m, v, c) =>
				{
					var isInUse = await repository.AnyAsync(p => p.Code == m.Code && p.Id != m.Id, c);
					if (isInUse) v.AddFailure("Code is already in use");
				});
		}
	}

	/* PARTIAL */

	public class PartialUpdateBudgetRequest : IRequest
	{
		public Guid Id { get; set; }

		public string Code { get; set; }

		public int? ClientId { get; set; }
		public States? State { get; set; }

		public decimal? Subtotal { get; set; }
		public decimal? Taxes { get; set; }
		public decimal? Total { get; set; }
	}

	public class PartialUpdateBudgetRequestHandler : IRequestHandler<PartialUpdateBudgetRequest>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;
		private readonly IIdentityService _identityService;

		public PartialUpdateBudgetRequestHandler(IGenericRepository<Budget, Guid> repository, IIdentityService identityService)
		{
			_repository = repository;
			_identityService = identityService;
		}

		public async Task<Unit> Handle(PartialUpdateBudgetRequest request, CancellationToken cancellationToken)
		{
			var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(Budget), request.Id);

			if (request.Code != null) entity.Code = request.Code;
			if (request.ClientId != null) entity.ClientId = request.ClientId.Value;
			if (request.State != null) entity.State = request.State.Value;
			if (request.Subtotal != null) entity.Subtotal = request.Subtotal.Value;
			if (request.Taxes != null) entity.Taxes = request.Taxes.Value;
			if (request.Total != null) entity.Total = request.Total.Value;

			entity.ModifiedAt = DateTime.Now;
			entity.ModifiedById = int.Parse(_identityService.User.GetUserId());

			await _repository.UpdateAsync(entity, cancellationToken);

			return Unit.Value;
		}
	}

	public class PartialUpdateBudgetRequestValidator : AbstractValidator<PartialUpdateBudgetRequest>
	{
		public PartialUpdateBudgetRequestValidator(IGenericRepository<Budget, Guid> repository)
		{
			RuleFor(m => m.Id).NotEmpty();
			RuleFor(m => new { m.Subtotal, m.Taxes, m.Total })
				.Custom((m, v) =>
				{
					if (m.Subtotal + m.Taxes != m.Total)
						v.AddFailure("Total is miscalculated");
				});
			RuleFor(m => new { m.Id, m.Code })
				.CustomAsync(async (m, v, c) =>
				{
					if (!string.IsNullOrWhiteSpace(m.Code))
					{
						var isInUse = await repository.AnyAsync(p => p.Code == m.Code && p.Id != m.Id, c);
						if (isInUse) v.AddFailure("Code is already in use");
					}
				});
		}
	}

	#endregion

	#region Delete

	public class DeleteBudgetRequest : IRequest
	{
		public Guid? Id { get; set; }
		public string Code { get; set; }
	}

	public class DeleteBudgetRequestHandler : IRequestHandler<DeleteBudgetRequest>
	{
		private readonly IGenericRepository<Budget, Guid> _repository;
		private readonly IIdentityService _identityService;

		public DeleteBudgetRequestHandler(IGenericRepository<Budget, Guid> repository, IIdentityService identityService)
		{
			_repository = repository;
			_identityService = identityService;
		}

		public async Task<Unit> Handle(DeleteBudgetRequest request, CancellationToken cancellationToken)
		{
			Budget entity = null;
			if (request.Id != null)
				entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
			else if (!string.IsNullOrWhiteSpace(request.Code))
				entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Code == request.Code, cancellationToken: cancellationToken);
			if (entity == null) throw new NotFoundException(nameof(Budget), request.Id);

			await _repository.DeleteAsync(request.Id, cancellationToken);

			entity.DeletedAt = DateTime.Now;
			var userId = _identityService.User.GetUserId();
			int.TryParse(userId, out int _deletedById);
			if (_deletedById != 0)
				entity.DeletedById = _deletedById;
			await _repository.UpdateAsync(entity);

			return Unit.Value;
		}
	}

	public class DeleteBudgetRequestValidator : AbstractValidator<DeleteBudgetRequest>
	{
		public DeleteBudgetRequestValidator()
		{
			RuleFor(m => new { m.Id, m.Code }).Custom((m, v) =>
			{
				if (m.Id == null && string.IsNullOrWhiteSpace(m.Code))
					v.AddFailure("Id or Code must have a value");
			});
		}
	}

	#endregion
}
