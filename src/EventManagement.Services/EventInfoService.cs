using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace losol.EventManagement.Services
{
	public class EventInfoService : IEventInfoService
	{
		private readonly ApplicationDbContext _db;
		private readonly IProductsService _productsService;

		public EventInfoService(ApplicationDbContext db, IProductsService productsService)
		{
			_db = db;
			_productsService = productsService;
		}

		public async Task<List<EventInfo>> GetFeaturedEventsAsync() 
		{
			return await _db.EventInfos.Where(i => i.Published && i.Featured)
					  .ToListAsync();
		}

		public async Task<List<EventInfo>> GetUpcomingEventsAsync()
		{
			return await _db.EventInfos
				.Where(a => a.DateStart >= DateTime.Now)
				.OrderByDescending(a => a.DateStart)
				.ToListAsync();
		}

		public async Task<EventInfo> GetAsync(int id)
		{
			return await _db.EventInfos
				            .SingleOrDefaultAsync(m => m.EventInfoId == id);
		}

		public async Task<EventInfo> GetWithProductsAsync(int id)
		{
			return await _db.EventInfos
				            .Include(ei => ei.Products)
				            	.ThenInclude(products => products.ProductVariants)
							.SingleOrDefaultAsync(m => m.EventInfoId == id);
		}

		public async Task<bool> AddAsync(EventInfo info)
		{
			await _db.EventInfos.AddAsync(info);
			return await _db.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateAsync(EventInfo info)
		{
			bool shouldDeleteProducts = info.Products != null;
			bool result = true;
			
			if(shouldDeleteProducts)
			{
				// Delete the products that din't exist in the request
				var originalProducts = await _productsService.GetForEventAsync(info.EventInfoId);
				var producstToDelete = originalProducts.Where(op => !info.Products.Any(p => p.ProductId == op.ProductId));
				_db.Products.RemoveRange(producstToDelete);
				result &= await _db.SaveChangesAsync() == producstToDelete.Count();
			}

			// Save the updates
			_db.EventInfos.Update(info);
			result &= await _db.SaveChangesAsync() > 0;

			return result;
		}
	}
}
