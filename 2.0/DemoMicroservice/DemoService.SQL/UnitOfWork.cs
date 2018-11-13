using System;
using System.Collections.Generic;
using DemoService.Core.Repositories;
using DemoService.SQL.Repositories;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace DemoService.SQL
{
	public class UnitOfWork : IUnitOfWork
	{
		#region Properties
		private readonly IConfiguration _configuration;
		private readonly DemoServiceDbContext _context;

		private static readonly Dictionary<Type, Func<IConfiguration, DemoServiceDbContext, int, string, object>>
			_repositories =
				new Dictionary<Type, Func<IConfiguration, DemoServiceDbContext, int, string, object>>
				{
					{
						typeof(IOrdersRepository),
						(configuration, context, apiVersion, tenantId) =>
							new OrdersRepository(configuration, context, apiVersion, tenantId)
					}
				};
		#endregion


		#region Constructor
		public UnitOfWork(IConfiguration configuration, DemoServiceDbContext context)
		{
			_configuration = configuration;
			_context = context;
		}
		#endregion


		#region public CreatEntityRepository
		public TEntityRepository CreatEntityRepository<TEntity, TEntityKey, TEntityRepository>(
			int apiVersion,
			string tenantId)
			where TEntity : class, ICoreEntity<TEntityKey>, new()
			where TEntityRepository : ICoreEntityRepository<TEntity, TEntityKey>
		{
			if (!_repositories.TryGetValue(typeof(TEntityRepository), out var factory))
				throw new NotSupportedException($"Cannot find repository factory for {typeof(TEntity).FullName}");

			// tODO: Find a way to eliminate type conversion
			return (TEntityRepository)factory(_configuration, _context, apiVersion, tenantId);
		}
		#endregion
	}
}