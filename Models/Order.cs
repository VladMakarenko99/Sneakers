using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace practice.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Town { get; set; }

        public string? Address { get; set; } 

        public Delivery? Delivery { get; set; }

        public List<Item>? Items { get; set; }

        public int? TotalPrice { get; set; }   
    }

    public enum Delivery{
        Fedex,
        Dhl
    }
}