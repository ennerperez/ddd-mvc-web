﻿using System.Security.Claims;
using System.Security.Principal;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Web.Services
{
	public class UserAccessorService : IUserAccessorService
	{
		private readonly IHttpContextAccessor _httpContext;

		public string Scheme
		{
			get
			{
				if (_httpContext.HttpContext != null) return _httpContext.HttpContext.Request.Scheme;
				return string.Empty;
			}
		}

		public string Host
		{
			get
			{
				if (_httpContext.HttpContext != null) return _httpContext.HttpContext.Request.Host.ToString();
				return string.Empty;
			}
		}

		public HttpContext Context => _httpContext.HttpContext;

		public UserAccessorService(IHttpContextAccessor httpContext)
		{
			_httpContext = httpContext;
		}

		public IPrincipal GetActiveUser()
		{
			if (_httpContext.HttpContext != null) return _httpContext.HttpContext.User;
			return null;
		}

		public string FindFirstValue(string claimType)
		{
			if (_httpContext.HttpContext != null) return _httpContext.HttpContext.User.FindFirstValue(claimType);
			return string.Empty;
		}
	}

}