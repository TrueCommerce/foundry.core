using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DemoService.Core.Models;

namespace DemoService.SQL
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			#region Order
			CreateMap<Order, Order>()
				.ForMember(e => e.DateCreated, opt => opt.Ignore())
				.ForMember(e => e.UserCreated, opt => opt.Ignore())
				.ForMember(e => e.DateModified, opt => opt.Ignore())
				.ForMember(e => e.UserModified, opt => opt.Ignore())
				.AfterMap((from, to) =>
				{
					if (from.Lines == null)
						return;

					#region map Lines
					if (to.Lines == null)
						to.Lines = new List<OrderLine>();

					// remove unselected
					var removed = to.Lines.Where(
							eEntity =>
								!from.Lines
									.Select(eModel => eModel.LineNumber)
									.Contains(eEntity.LineNumber))
						.ToList();
					foreach (var f in removed)
						to.Lines.Remove(f);

					// add new
					var added = @from.Lines.Where(eModel =>
							to.Lines.All(eEntity =>
								eEntity.LineNumber != eModel.LineNumber))
						.Select(Mapper.Map<OrderLine, OrderLine>)
						.ToList();
					foreach (var f in added)
						to.Lines.Add(f);
					#endregion
				});
			#endregion

			#region OrderLines
			CreateMap<OrderLine, OrderLine>();
			#endregion
		}
	}
}