using System;
using System.Security.Claims;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;

#if USING_SMARTSCHEMA
using Microsoft.AspNetCore.Authorization;
#endif

// ReSharper disable RedundantCast

namespace Web.Controllers
{
#if USING_SMARTSCHEMA
	[SmartAuthorize]
#endif
#if USING_SWAGGER
	[ApiExplorerSettings(IgnoreApi = true)]
#endif
	public abstract class MvcControllerBase : Controller
	{
		private ISender _mediator = null!;
		protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

		protected IGenericRepository<TEntity> Repository<TEntity>()
			where TEntity : class, IEntity<int>
			=> HttpContext.RequestServices.GetRequiredService<IGenericRepository<TEntity>>();
		protected IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>()
			where TKey : struct, IComparable<TKey>, IEquatable<TKey>
			where TEntity : class, IEntity<TKey>
			=> HttpContext.RequestServices.GetRequiredService<IGenericRepository<TEntity, TKey>>();

		protected bool IsAdmin => User.IsInRole(Roles.Admin);
		protected int UserId => User.GetUserId<int>();

	}

}
