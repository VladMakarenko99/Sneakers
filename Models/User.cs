using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace Sneakers.Models
{
	public class User
	{

		public int Id { get; set; }

		public string? FirstName { get; set; }

		public string? LastName { get; set; }

        public string? Email { get; set; } 
        
        public string? Password { get; set; }
        
        public string? ConfirmedPass { get; set; } 
        
        public string? Salt { get; set; }

		public string? CartItemsJson { get; set; } 

		public byte[]? ProfilePhoto { get; set; }	

        public User()
		{
		}

        public User(string firstName, string lastName, string email, string password, string confirmedPass, string salt)
        {
	        FirstName = firstName;
			LastName = lastName;
	        Email = email;
	        Password = password;
	        ConfirmedPass = confirmedPass;
	        Salt = salt;
        }
	}
}

