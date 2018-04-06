﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace losol.EventManagement.Services
{
	public class RegistrationService : IRegistrationService
	{
		private readonly ApplicationDbContext _db;


		public RegistrationService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<Registration> GetAsync(int id)
		{
			return await _db.Registrations
				            .FindAsync(id);
		}

		public Task<Registration> GetAsync(string userId, int eventId)
		{
			return _db.Registrations.Where(a => a.UserId == userId && a.EventInfoId == eventId)
				      .SingleOrDefaultAsync();
		}

		public async Task<Registration> GetWithEventInfoAsync(int id)
		{
			return await _db.Registrations
				            .Where(x => x.RegistrationId == id)
							.Include(r => r.EventInfo)
			       		    .SingleOrDefaultAsync();
		}

		public async Task<List<Registration>> GetVerifiedRegistrations(int eventId)
		{
			return await _db.Registrations
							.Where(r => r.EventInfoId == eventId && r.Verified)
							.Include(r => r.EventInfo)
							.ToListAsync();
		}

		public async Task<int> CreateRegistration(Registration registration, int[] productIds, int[] variantIds)
		{
			// Check if registration exists
			var existingRegistration = await GetAsync(registration.UserId, registration.EventInfoId);
			if(existingRegistration != null)
			{
				throw new InvalidOperationException("The user can only register once!");
			}

			// Create orders if productIds is not null
			if (productIds != null)
			{
				var products = await _db.Products
										.Where(p => productIds.Contains(p.ProductId))
										.Include(p => p.ProductVariants)
										.AsNoTracking()
										.ToListAsync();

				// Create an order for the registration
				registration.CreateOrder(
					products,
					products.SelectMany(p => p.ProductVariants)
							.Where(v => variantIds?.Contains(v.ProductVariantId) ?? false)
				);
			}

			// Create the registration
			await _db.Registrations.AddAsync(registration);
			return await _db.SaveChangesAsync();
		}

		public Task<int> CreateRegistration(Registration registration) =>
			CreateRegistration(registration, null, null);

		public async Task<int> SetRegistrationAsVerified(int id)
		{
			var registration = await GetAsync(id);
			registration.Verify();
			return await _db.SaveChangesAsync();
		}

        public async Task<List<Certificate>> CreateNewCertificates(int eventId, string issuedByUsername)
        {
			_ = issuedByUsername ?? throw new ArgumentNullException(paramName:nameof(issuedByUsername));
            var infoQueryable = _db.EventInfos
						.Where(e => e.EventInfoId == eventId);
			var eventInfo = await infoQueryable.AsNoTracking().SingleOrDefaultAsync();
			_ = eventInfo ?? throw new ArgumentException("Not event corresponds to that eventId", paramName: nameof(eventId));

			var user = await _db.ApplicationUsers
								.Where(u => issuedByUsername == u.UserName)
								.SingleOrDefaultAsync();
			_ = user ?? throw new ArgumentException("Invalid userId", paramName: nameof(issuedByUsername));

			var certs = await infoQueryable.SelectMany(i => i.Registrations)
								.Where(r => r.Verified && r.Attended && r.Certificate == null)
								.Select(r => new Certificate {
									CertificateId = r.RegistrationId,
									RecipientName = r.ParticipantName,
									RecipientUserId = r.UserId,

									Title = eventInfo.Title,
									Description = eventInfo.CertificateDescription,

									Issuer = new Certificate.CertificateIssuer {
										OrganizationId = 0,
										OrganizationName = null,
										OrganizationLogoUrl = null,
										IssuedByUserId = user.Id,
										IssuedByName = user.Name,
										IssuedInCity = eventInfo.City
									}
								}).ToListAsync();
			_db.Certificates.AddRange(certs);
			await _db.SaveChangesAsync();
			
			var newIds = certs.Select(c => c.CertificateId);
			return await _db.Certificates
							.Where(c => newIds.Contains(c.CertificateId))
							.Include(c => c.RecipientUser)
							.ToListAsync();
        }
    }
}
