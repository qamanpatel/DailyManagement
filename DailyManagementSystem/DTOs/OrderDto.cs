using System;

namespace DailyManagementSystem.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int ClientId { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? OrderName { get; set; }
        
        // Extended Properties for View
        public string ClientName { get; set; } = string.Empty;
        public bool IsClientActive { get; set; }

        // --- Work Order Specific Fields ---
        public string? Size { get; set; }
        public string? UOM { get; set; }
        public int Quantity { get; set; } = 1;
        public string? MaterialNo { get; set; }
        public string? CostingLayer { get; set; }
        public string? Color { get; set; }

        // Production Specs
        public string? MaterialSpec { get; set; }
        public string? PaintSpec { get; set; }
        public string? QualitySpec { get; set; }
        public string? WorkNatureSpec { get; set; }
        public string? DurabilitySpec { get; set; }

        // Significant Dates
        public DateTime? ModelingLastDate { get; set; }
        public DateTime? FiberStartDate { get; set; }

        // Sign-offs
        public string? OrderBy { get; set; }
        public string? ModelingBy { get; set; }
        public string? FiberBy { get; set; }
        public string? ImagePath { get; set; }
    }
}
