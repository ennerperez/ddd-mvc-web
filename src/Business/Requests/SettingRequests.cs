using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Exceptions;
using Business.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Business.Requests
{
    #region Create

    public class CreateSettingRequest : IRequest<int>
    {
        public string Key { get; set; }
        public Data Type { get; set; }
        public string Value { get; set; }
    }

    public class CreateSettingRequestHandler : IRequestHandler<CreateSettingRequest, int>
    {
        private readonly IGenericRepository<Setting> _repository;

        public CreateSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = new Setting();
            entity.Key = request.Key;
            entity.Type = request.Type;
            entity.Value = request.Value;

            await _repository.CreateAsync(entity, cancellationToken);

            return entity.Id;
        }
    }

    public class CreateSettingRequestValidator : AbstractValidator<CreateSettingRequest>
    {
        public CreateSettingRequestValidator(IGenericRepository<Setting> repository)
        {
            RuleFor(m => m.Key).NotEmpty();
            RuleFor(m => m.Type).NotEmpty();
            RuleFor(m => new { m.Key })
                .CustomAsync(async (m, v, c) =>
                {
                    var keyInUse = await repository.AnyAsync(p => p.Key == m.Key, c);
                    if (keyInUse) v.AddFailure("Key is already in use");
                });
        }
    }

    #endregion

    #region Read

    public class ReadPaginatedSettingsRequestHandler : IRequestHandler<PaginatedRequest<Setting, Setting>, PaginatedList<Setting>>
    {
        private readonly IGenericRepository<Setting> _repository;

        public ReadPaginatedSettingsRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<Setting>> Handle(PaginatedRequest<Setting, Setting> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var number = ((request.Skip ?? 10) / (request.Take ?? 10)) + 1;
            var result = await PaginatedList<Setting>.CreateAsync(entities, number, request.Take ?? 10, cancellationToken);

            return result;
        }
    }

    public class ReadSettingsRequestHandler : IRequestHandler<GenericRequest<Setting, Setting>, Setting[]>
    {
        private readonly IGenericRepository<Setting> _repository;

        public ReadSettingsRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<Setting[]> Handle(GenericRequest<Setting, Setting> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var items = await entities.ToArrayAsync();
            return items;
        }
    }

    #endregion

    #region Update

    public class UpdateSettingRequest : IRequest
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public Data Type { get; set; }
        public string Value { get; set; }
    }

    public class UpdateSettingRequestHandler : IRequestHandler<UpdateSettingRequest>
    {
        private readonly IGenericRepository<Setting> _repository;

        public UpdateSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null) throw new NotFoundException(nameof(Setting), request.Id);

            entity.Key = request.Key;
            entity.Type = request.Type;
            entity.Value = request.Value;

            await _repository.UpdateAsync(entity, cancellationToken);

            return Unit.Value;
        }
    }

    public class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
    {
        public UpdateSettingRequestValidator(IGenericRepository<Setting> repository)
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => m.Key).NotEmpty();
            RuleFor(m => m.Type).NotEmpty();
            RuleFor(m => new { m.Id, m.Key })
                .CustomAsync(async (m, v, c) =>
                {
                    var keyInUse = await repository.AnyAsync(p => p.Key == m.Key && p.Id != m.Id, c);
                    if (keyInUse) v.AddFailure("Key is already in use");
                });
        }
    }

    /* PARTIAL */

    public class PartialUpdateSettingRequest : IRequest
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public Data? Type { get; set; }
        public string Value { get; set; }
    }

    public class PartialUpdateSettingRequestHandler : IRequestHandler<PartialUpdateSettingRequest>
    {
        private readonly IGenericRepository<Setting> _repository;

        public PartialUpdateSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(PartialUpdateSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null) throw new NotFoundException(nameof(Setting), request.Id);

            if (request.Key != null) entity.Key = request.Key;
            if (request.Type != null) entity.Type = request.Type.Value;
            if (request.Value != null) entity.Value = request.Value;

            await _repository.UpdateAsync(entity, cancellationToken);

            return Unit.Value;
        }
    }

    public class PartialUpdateSettingRequestValidator : AbstractValidator<PartialUpdateSettingRequest>
    {
        public PartialUpdateSettingRequestValidator(IGenericRepository<Setting> repository)
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => new { m.Id, m.Key })
                .CustomAsync(async (m, v, c) =>
                {
                    if (m.Key != null)
                    {
                        var keyInUse = await repository.AnyAsync(p => p.Key == m.Key && p.Id != m.Id, c);
                        if (keyInUse) v.AddFailure("Key is already in use");
                    }
                });
        }
    }

    #endregion

    #region Delete

    public class DeleteSettingRequest : IRequest
    {
        public int Id { get; set; }
    }

    public class DeleteSettingRequestHandler : IRequestHandler<DeleteSettingRequest>
    {
        private readonly IGenericRepository<Setting> _repository;

        public DeleteSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null) throw new NotFoundException(nameof(Setting), request.Id);

            await _repository.DeleteAsync(request.Id, cancellationToken);

            return Unit.Value;
        }
    }

    public class DeleteSettingRequestValidator : AbstractValidator<DeleteSettingRequest>
    {
        public DeleteSettingRequestValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
        }
    }

    #endregion
}
