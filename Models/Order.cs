namespace Sneakers.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Town { get; set; }

        public string? Address { get; set; } 
        
        public string? ItemsJson { get; set; }

        public int? TotalPrice { get; set; }   

        public int UserId { get; set; } 

        public string? SuccessUrl { get; set; }
    }
}