using System;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstractions;
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
        //private readonly IdentityDbContext _context;

        public CreateSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = new Setting
            {
                Key = request.Key,
                Type = request.Type,
                Value = request.Value,
            };

            await _repository.CreateAsync(entity);

            return entity.Id;
        }
    }

    public class CreateSettingRequestValidator : AbstractValidator<CreateSettingRequest>
    {
        public CreateSettingRequestValidator(IGenericRepository<Setting> repository)
        {
            RuleFor(m => m.Key)
                .NotEmpty()
                .CustomAsync(async (v, x, _) =>
                {
                    var identificationInUse = await repository.AnyAsync(m => m.Key == v);
                    if (identificationInUse) x.AddFailure("Key is already in use");
                });
            RuleFor(m => m.Type).NotEmpty();
        }
    }

    #endregion

    #region Read

    public class ReadPaginatedSettingsRequestHandler : IRequestHandler<GenericPaginatedRequest<Setting, Setting>, PaginatedList<Setting>>
    {
        private readonly IGenericRepository<Setting> _repository;

        public ReadPaginatedSettingsRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedList<Setting>> Handle(GenericPaginatedRequest<Setting, Setting> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            var take = request.Take ?? 10;
            var skip = request.Skip ?? 10;
            var number = (skip / take) + 1;
            var result = await PaginatedList<Setting>.CreateAsync(entities, number, request.Take ?? 10);

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

    public class UpdateSettingRequest : CreateSettingRequest
    {
        public int Id { get; set; }
    }

    public class UpdateSettingRequestHandler : IRequestHandler<UpdateSettingRequest, int>
    {
        private readonly IGenericRepository<Setting> _repository;

        public UpdateSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(UpdateSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("Setting not found");

            entity.Key = request.Key;
            entity.Type = request.Type;
            entity.Value = request.Value;

            await _repository.UpdateAsync(entity);

            return entity.Id;
        }
    }

    public class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
    {
        public UpdateSettingRequestValidator(IGenericRepository<Setting> repository)
        {
            RuleFor(m => new {m.Id, m.Key})
                .NotEmpty()
                .CustomAsync(async (v, x, _) =>
                {
                    var identificationInUse = await repository.AnyAsync(m => m.Key == v.Key && m.Id != v.Id);
                    if (identificationInUse) x.AddFailure("Key is already in use");
                });
            RuleFor(m => m.Type).NotEmpty();
        }
    }

    #endregion

    #region Delete

    public class DeleteSettingRequest : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteSettingRequestHandler : IRequestHandler<DeleteSettingRequest, int>
    {
        private readonly IGenericRepository<Setting> _repository;

        public DeleteSettingRequestHandler(IGenericRepository<Setting> repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(DeleteSettingRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FirstOrDefaultAsync(s => s, p => p.Id == request.Id);
            if (entity == null) throw new Exception("Setting not found");

            await _repository.DeleteAsync(request.Id);

            return entity.Id;
        }
    }

    public class DeleteSettingRequestValidator : AbstractValidator<DeleteSettingRequest>
    {
        public DeleteSettingRequestValidator()
        {
            RuleFor(m => m.Id)
                .NotEmpty();
        }
    }

    #endregion
}
