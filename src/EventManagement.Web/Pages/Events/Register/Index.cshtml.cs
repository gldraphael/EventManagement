﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using losol.EventManagement.Domain;
using losol.EventManagement.Pages.Account;
using losol.EventManagement.Services;
using losol.EventManagement.ViewModels;
using losol.EventManagement.Web.Services;
using losol.EventManagement.Services.Extensions;
using System.Text;

namespace losol.EventManagement.Web.Pages.Events.Register
{
	public class EventRegistrationModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<EventRegistrationModel> _logger;
		private readonly IEventInfoService _eventsService;
		private readonly IPaymentMethodService _paymentMethodService;
		private readonly IRegistrationService _registrationService;
		private readonly RegistrationEmailSender _registrationEmailSender;

		public EventRegistrationModel(
			UserManager<ApplicationUser> userManager,
			RegistrationEmailSender registrationEmailSender,
			ILogger<EventRegistrationModel> logger,
			IEventInfoService eventsService,
			IPaymentMethodService paymentMethodService,
			IRegistrationService registrationService
		)
		{
			_userManager = userManager;
			_logger = logger;
			_eventsService = eventsService;
			_paymentMethodService = paymentMethodService;
			_registrationService = registrationService;
			_registrationEmailSender = registrationEmailSender;
		}

		[BindProperty]
		public RegisterVM Registration { get; set; }
		public EventInfo EventInfo { get; set; }
		public List<PaymentMethod> PaymentMethods { get; set; }
		public List<Product> Products => EventInfo.Products;
		public int DefaultPaymentMethod => _paymentMethodService.GetDefaultPaymentMethodId();

		public async Task<IActionResult> OnGetAsync(int id)
		{

			EventInfo = await _eventsService.GetWithProductsAsync(id);

			if (EventInfo == null)
			{
				return NotFound();
			}

			PaymentMethods = _paymentMethodService.GetActivePaymentMethods();
			Registration = new RegisterVM(EventInfo, DefaultPaymentMethod);

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(int id)
		{

			if (!ModelState.IsValid)
			{
				PaymentMethods = _paymentMethodService.GetActivePaymentMethods();
				return Page();
			}

			EventInfo = await _eventsService.GetWithProductsAsync(id);
			if (EventInfo == null) return NotFound();
			Registration.EventInfoId = EventInfo.EventInfoId;

			// Check if user exists with email registered
			var user = await _userManager.FindByEmailAsync(Registration.Email);
			if (user != null)
			{
				Registration.UserId = user.Id;
				_logger.LogInformation("Found existing user.");
				// Any registrations for this user on this event?
				var existingRegistration = await _registrationService.GetAsync(user.Id, Registration.EventInfoId);
				if (existingRegistration != null)
				{
					// The user has already registered for the event.
					await _registrationEmailSender.SendRegistrationAsync(user.Email, "Du var allerede påmeldt!", "Vi hadde allerede en registrering for deg.", existingRegistration.RegistrationId);
					return RedirectToPage("/Info/EmailSent");
				}
			}
			else
			{
				// Create new user
				var newUser = new ApplicationUser { UserName = Registration.Email, Name = Registration.ParticipantName, Email = Registration.Email, PhoneNumber = (Registration.PhoneCountryCode + Registration.Phone) };
				var result = await _userManager.CreateAsync(newUser);

				if (result.Succeeded)
				{
					_logger.LogInformation("User created a new account with password.");

					Registration.UserId = newUser.Id;
					user = newUser;
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					PaymentMethods = _paymentMethodService.GetActivePaymentMethods();
					return Page();
				}
			}

			// If we came here, we should enter our new participant into the database!
			_logger.LogInformation("Starting new registration:");

			var newRegistration = Registration.Adapt<Registration>();
			newRegistration.VerificationCode = PasswordHelper.GeneratePassword(6);
			await _registrationService.CreateRegistration(newRegistration, Registration.SelectedProducts);
            await _registrationEmailSender.SendRegistrationAsync(user.Email, "Velkommen på kurs!", "Vi fikk registreringen din", newRegistration.RegistrationId);


			return RedirectToPage("/Info/EmailSent");
		}
	}
}
