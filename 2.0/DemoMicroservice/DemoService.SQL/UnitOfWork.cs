using System;
using System.Collections.Generic;
using AutoMapper;
using DemoService.Core.Repositories;
using DemoService.SQL.Repositories;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Models;
using Foundry.Core.Shared.SQL;
using Microsoft.Extensions.Configuration;

namespace DemoService.SQL
{
	public class UnitOfWork : IUnitOfWork
	{
		#region Properties
		private readonly IConfiguration _configuration;
		private readonly DemoServiceDbContext _context;
		private readonly IMapper _mapper;

		public string AuthorizedUserEmail
		{
			get => _context.AuthorizedUserEmail;
			set => _context.AuthorizedUserEmail = value;
		}

		private static readonly Dictionary<Type, Func<SQLUnitOfWork<DemoServiceDbContext>, string, object>>
			_repositories =
				new Dictionary<Type, Func<SQLUnitOfWork<DemoServiceDbContext>, string, object>>
				{
					{
						typeof(IOrdersRepository),
						(sqlUnitOfWork, tenantId) =>
							new OrdersRepository(sqlUnitOfWork, tenantId)
					}
				};
		#endregion


		#region Constructor
		public UnitOfWork(IConfiguration configuration, DemoServiceDbContext context, IMapper mapper)
		{
			_configuration = configuration;
			_context = context;
			_mapper = mapper;
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
			return (TEntityRepository)factory(
				new SQLUnitOfWork<DemoServiceDbContext>(_configuration, _mapper, _context, apiVersion),
				tenantId);
		}
		#endregion
	}
}