﻿using HkEcommerce.DataAccess.Repository.IRepository;
using HkEcommerce.Model;
using HkEcommerce.Model.ViewModels;
using HkEcommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace HkEcommerce.Areas.Customer.Controllers
{	
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailSender _emailSender;
		private TwilioSettings _twilioOptions { get; set; }
		private readonly UserManager<IdentityUser> _userManager;

		[BindProperty]	
		public ShoppingCartVM ShoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender,UserManager<IdentityUser> userManager, IOptions<TwilioSettings> twilioOptions)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
			_userManager = userManager;
			_twilioOptions = twilioOptions.Value;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				OrderHeader = new Model.OrderHeader(),
				ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties:"Product")
			};
			ShoppingCartVM.OrderHeader.OrderTotal = 0;
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
				.GetFirstOrDefault(u => u.Id == claim.Value,
				includeProperties: "Company");
			//The code below will be used to calculate the total amount of product
			foreach (var list in ShoppingCartVM.ListCart)
			{
				//Remember the price property is not mapped because you will need to calculate it for each different products.
				list.Price = SD.GetPriceBasedOnQuantity
					(
						list.Count,
						list.Product.Price, 
						list.Product.Price50,
						list.Product.Price100
					);
				//ShoppingCartVM.OrderHeader.OrderTotal = (list.Price * list.Count) + ShoppingCartVM.OrderHeader.OrderTotal;
				ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
				//The code below is used to show the description of the products
				list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
				if (list.Product.Description.Length > 100)
				{
					list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
				}
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Index")]
		public async Task<IActionResult> IndexPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			//This line of code retrieves the user from the database
			var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
			if(user == null)
			{
				ModelState.AddModelError(string.Empty, "Verification Email is empty!");
			}

			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = Url.Page(
				"/Account/ConfirmEmail",
				pageHandler: null,
				values: new { area = "Identity", userId = user.Id, code = code},
				protocol: Request.Scheme);

			await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
				$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

			ModelState.AddModelError(string.Empty, "Verification Email is sent. Please check your email.");
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");
			cart.Count += 1;
			cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");
			if(cart.Count == 1)
			{
				var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
				_unitOfWork.ShoppingCart.Remove(cart);
				_unitOfWork.Save();
				HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);				
			}
			else
			{
				cart.Count -= 1;
				cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
				_unitOfWork.Save();
			}			
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");
			
				var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
				_unitOfWork.ShoppingCart.Remove(cart);
				_unitOfWork.Save();
				HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				OrderHeader = new Model.OrderHeader(),
				ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "Product")
			};
			
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties:"Company");

			foreach (var list in ShoppingCartVM.ListCart)
			{
				//Remember the price property is not mapped because you will need to calculate it for each different products.
				list.Price = SD.GetPriceBasedOnQuantity
					(
						list.Count,
						list.Product.Price,
						list.Product.Price50,
						list.Product.Price100
					);
				ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
			}
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;

			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;

			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;

			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			return View(ShoppingCartVM);
		}


		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]

	//	public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
		public IActionResult SummaryPost(string stripeToken)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
				.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties:"Company");

			ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "Product");

			ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

		//The code below was never used.
		//	List<OrderDetails> orderDetailsList = new List<OrderDetails>();
			foreach (var item in ShoppingCartVM.ListCart)
			{
				item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
				OrderDetails orderDetails = new OrderDetails()
				{
					ProductId = item.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = item.Price,
					Count = item.Count
				};
				ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
				_unitOfWork.OrderDetails.Add(orderDetails);

			}
			_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
			_unitOfWork.Save();
			HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

			if(stripeToken == null)
			{
				//order will be created for delayed payment authorized company
				ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			else
			{
				//Process the payment
				var options = new ChargeCreateOptions
				{
					Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
					Currency = "usd",
					Description = "Order ID: " + ShoppingCartVM.OrderHeader.Id,
					Source = stripeToken
				};
				var service = new ChargeService();
				Charge charge = service.Create(options);

				if(charge.BalanceTransactionId == null)
				{
					ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
				}
				else
				{
					ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
				}
				if(charge.Status.ToLower() == "succeeded")
				{
					ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
					ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
					ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
				}
			}
			_unitOfWork.Save();
			return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
			TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
			//This will execute if the phone number is correct and incorrect
			try
			{
				var message = MessageResource.Create(
					body: "Order placed on HkEcommerce. Your Id is:" + id,
					from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
					to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
					);
			}
			catch (Exception)
			{
				throw;
			}
			return View(id);
		}


	}
}
