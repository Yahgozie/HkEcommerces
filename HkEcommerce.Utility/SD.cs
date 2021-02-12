using System;
using System.Collections.Generic;
using System.Text;

namespace HkEcommerce.Utility
{
	public class SD
	{
		//These are static roles required for the website

		public const string Role_User_Indi = "Individual Customer";
		public const string Role_User_Comp = "Company Customer";
		public const string Role_Admin = "Admin";//Manage the content of the website, like to add new product, prices....
		public const string Role_Employee = "Employee"; //Able to change the status of the packages and take payments......

		public const string ssShoppingCart = "Shoping Cart Session";


		public const string StatusPending = "Pending";
		public const string StatusApproved = "Approved";
		public const string StatusInProcess = "Processing";
		public const string StatusShipped = "Shipped";
		public const string StatusCancelled = "Cancelled";
		public const string StatusRefunded = "Refunded";

		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected = "Rejected";


		//The constant below is for calculating the price of products based on quantity selected
		public static decimal GetPriceBasedOnQuantity(decimal quantity, decimal price, decimal price50, decimal price100)
		{
			if(quantity < 50)
			{
				return price;
			}
			else
			{
				if(quantity < 100)
				{
					return price50;
				}
				else
				{
					return price100;
				}
			}
		}

		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}

	}
}
