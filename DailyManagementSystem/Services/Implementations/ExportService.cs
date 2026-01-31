using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DailyManagementSystem.Services.Implementations
{
    public class ExportService : IExportService
    {
        private readonly string _logoPath = "/Users/aryanpatel/.gemini/antigravity/brain/3f39247d-36c4-4b5c-8f3f-6d9802ea0ce2/uploaded_media_1769275466818.png";

        public ExportService()
        {
            // QuestPDF License - Community
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private string GetPeriodText(int? startYear, int? startMonth, int? endYear, int? endMonth)
        {
            if (!startYear.HasValue) return "All Time";
            string startPart = startMonth.HasValue ? new DateTime(startYear.Value, startMonth.Value, 1).ToString("MMM yyyy") : startYear.Value.ToString();
            if (!endYear.HasValue) return startMonth.HasValue ? startPart : $"Year {startPart}";
            string endPart = endMonth.HasValue ? new DateTime(endYear.Value, endMonth.Value, 1).ToString("MMM yyyy") : endYear.Value.ToString();
            return $"{startPart} to {endPart}";
        }

        public async Task ExportReportToPdfAsync(string filePath, ReportData data)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Inch);
                        page.Content().Column(col =>
                        {
                            col.Item().AlignCenter().Width(150).Image(_logoPath);
                            col.Item().PaddingTop(20).AlignCenter().Text("RAM TIRATH ART").FontSize(32).FontColor(Colors.Grey.Medium).Bold();
                            col.Item().PaddingTop(10).AlignCenter().Text("FINANCIAL REPORT").FontSize(24).FontColor(Colors.Grey.Medium);
                            string period = GetPeriodText(data.StartYear, data.StartMonth, data.EndYear, data.EndMonth);
                            col.Item().PaddingTop(50).AlignCenter().Text(period).FontSize(18);
                            col.Item().AlignCenter().Text($"Generated on {DateTime.Now:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Text("RAM TIRATH ART").FontSize(12).FontColor(Colors.Grey.Medium).SemiBold();
                            row.RelativeItem().AlignRight().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
                        });
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Item().Text("Summary Information").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
                                table.Cell().Text("Description"); table.Cell().Text("Amount");
                                table.Cell().Text("Total Orders"); table.Cell().Text(data.Summary.TotalOrders.ToString());
                                table.Cell().Text("Total Billed"); table.Cell().Text($"{data.Summary.TotalOrderAmount:C}");
                                table.Cell().Text("Total Received"); table.Cell().Text($"{data.Summary.TotalReceivedAmount:C}");
                                table.Cell().Text("Total Expenses"); table.Cell().Text($"{data.Summary.TotalExpenses:C}");
                                table.Cell().Text("Total Pending"); table.Cell().Text($"{data.Summary.TotalPendingAmount:C}");
                                table.Cell().BorderTop(1).Text("Net Profit").SemiBold(); table.Cell().BorderTop(1).Text($"{data.Summary.Profit:C}").SemiBold();
                            });

                            if (data.Orders != null && data.Orders.Any())
                            {
                                col.Item().PaddingTop(20).Text("Detailed Orders").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns => { columns.ConstantColumn(80); columns.RelativeColumn(); columns.ConstantColumn(80); columns.ConstantColumn(80); });
                                    table.Header(header => { header.Cell().Text("Date").SemiBold(); header.Cell().Text("Client").SemiBold(); header.Cell().Text("Amount").SemiBold(); header.Cell().Text("Status").SemiBold(); });
                                    foreach (var o in data.Orders) { table.Cell().Text(o.Date.ToString("dd MMM yyyy")); table.Cell().Text(o.ClientName); table.Cell().Text($"{o.Amount:C}"); table.Cell().Text(o.Status); }
                                });
                            }

                            if (data.Payments != null && data.Payments.Any())
                            {
                                col.Item().PaddingTop(20).Text("Detailed Receipts").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns => { columns.ConstantColumn(80); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.ConstantColumn(80); });
                                    table.Header(header => { header.Cell().Text("Date").SemiBold(); header.Cell().Text("Client").SemiBold(); header.Cell().Text("Bank").SemiBold(); header.Cell().Text("Order").SemiBold(); header.Cell().Text("Amount").SemiBold(); });
                                    foreach (var p in data.Payments) { table.Cell().Text(p.Date.ToString("dd MMM yyyy")); table.Cell().Text(p.ClientName); table.Cell().Text(p.BankName ?? "N/A"); table.Cell().Text(p.OrderName); table.Cell().Text($"{p.Amount:C}"); }
                                });
                            }

                            if (data.Expenses != null && data.Expenses.Any())
                            {
                                col.Item().PaddingTop(20).Text("Detailed Expenses").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns => { columns.ConstantColumn(80); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.ConstantColumn(80); });
                                    table.Header(header => { header.Cell().Text("Date").SemiBold(); header.Cell().Text("Category").SemiBold(); header.Cell().Text("Spent By").SemiBold(); header.Cell().Text("Description").SemiBold(); header.Cell().Text("Amount").SemiBold(); });
                                    foreach (var e in data.Expenses) { table.Cell().Text(e.Date.ToString("dd MMM yyyy")); table.Cell().Text(e.Category); table.Cell().Text(e.SpentBy ?? "N/A"); table.Cell().Text(e.Description); table.Cell().Text($"{e.Amount:C}"); }
                                });
                            }
                        });
                    });

                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Inch);
                        page.Content().Column(col =>
                        {
                            col.Item().PaddingTop(100).AlignCenter().Text("AUTHORIZATION & SIGNATURES").FontSize(18).SemiBold();
                            col.Item().PaddingTop(10).AlignCenter().Text("This document is a formal record of financial transactions for the specified period.").FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(150).Row(row =>
                            {
                                row.RelativeItem().Column(c => { c.Item().BorderTop(1).PaddingTop(5).AlignCenter().Text("Satyanam Patel").SemiBold(); c.Item().AlignCenter().Text("Manager").FontSize(8); });
                                row.ConstantItem(50);
                                row.RelativeItem().Column(c => { c.Item().BorderTop(1).PaddingTop(5).AlignCenter().Text("Ram Tirath Patel").SemiBold(); c.Item().AlignCenter().Text("Proprietor").FontSize(8); });
                                row.ConstantItem(50);
                                row.RelativeItem().Column(c => { c.Item().BorderTop(1).PaddingTop(5).AlignCenter().Text("Sandeep Patel").SemiBold(); c.Item().AlignCenter().Text("Partner").FontSize(8); });
                            });
                        });
                    });
                });
                document.GeneratePdf(filePath);
            });
        }

        public async Task ExportWorkOrderToPdfAsync(string filePath, Order order)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(0.5f, Unit.Centimetre);
                        page.Header().Column(col =>
                        {
                            // Top Red Banner
                            col.Item().Background("#D32F2F").PaddingHorizontal(10).PaddingVertical(5).Row(row =>
                            {
                                row.RelativeItem().Text("GSTIN: 09COWPP5425G1ZA").FontSize(10).FontColor(Colors.White).SemiBold();
                                row.RelativeItem().AlignRight().Text("📞 9450686765, 9161410260, 8887732619").FontSize(10).FontColor(Colors.White).SemiBold();
                            });

                            // Branding
                            col.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Width(80).Image(_logoPath);
                                });
                                row.RelativeItem(3).Column(c =>
                                {
                                    c.Item().AlignCenter().Text("Ram Tirath Art").FontSize(38).FontColor("#D32F2F").Bold();
                                    c.Item().AlignCenter().Row(r =>
                                    {
                                        r.AutoItem().Text("www.ramtirathart.com").FontSize(10).FontColor(Colors.Grey.Medium);
                                        r.AutoItem().PaddingLeft(20).Text("📧 info@ramtirathart.com").FontSize(10).FontColor(Colors.Grey.Medium);
                                    });
                                });
                            });

                            // Decorative Quote/Tagline in red bar
                            col.Item().PaddingTop(5).Background("#D32F2F").PaddingVertical(3).AlignCenter()
                                .Text("All Kinds of Sculpture, Murals, Wallarts, Landscaping, Fountain & Other Art Works etc.")
                                .FontSize(9).FontColor(Colors.White).Bold();
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            // Order Info Row
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"SN-{order.OrderId}").FontSize(12).Bold();
                                    c.Item().PaddingTop(10).Text("To,").FontSize(11).Bold();
                                    c.Item().PaddingLeft(10).Text(order.Client?.ClientName ?? "N/A").FontSize(12).SemiBold();
                                });

                                row.RelativeItem().AlignCenter().Column(c =>
                                {
                                    c.Item().PaddingTop(10).Text("Work Order").FontSize(22).Bold().Underline();
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Booking Date - {order.OrderDate:dd MMM yyyy}").FontSize(10);
                                    c.Item().Text($"Mod Lst Date - {order.ModelingLastDate?.ToString("dd MMM yyyy") ?? "-"}").FontSize(10);
                                    c.Item().PaddingTop(10).Text($"Fibr Start Date - {order.FiberStartDate?.ToString("dd MMM yyyy") ?? "-"}").FontSize(10);
                                    c.Item().Text($"Delivery Date - {order.DeliveredDate?.ToString("dd MMM yyyy") ?? "-"}").FontSize(10);
                                });
                            });

                            // Main Items Table
                            col.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // SN
                                    columns.RelativeColumn(3); // NOMENCLATURE
                                    columns.RelativeColumn(); // SIZE
                                    columns.RelativeColumn(); // UOM
                                    columns.RelativeColumn(); // QTY
                                    columns.RelativeColumn(); // RATE
                                    columns.RelativeColumn(); // COST
                                });

                                table.Header(header =>
                                {
                                    var headerStyle = TextStyle.Default.FontSize(11).Bold();
                                    header.Cell().Border(1).AlignCenter().Text("SN").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("NOMENCLATURE").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("SIZE").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("UOM").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("QTY").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("RATE").Style(headerStyle);
                                    header.Cell().Border(1).AlignCenter().Text("COST").Style(headerStyle);
                                });

                                // Add Order Row
                                table.Cell().Border(1).AlignCenter().Text("1.").FontSize(10);
                                table.Cell().Border(1).Padding(5).Column(c =>
                                {
                                    c.Item().Text(order.OrderName ?? "Art Work").FontSize(11).Bold();
                                    
                                    // Embed Image if exists
                                    if (!string.IsNullOrEmpty(order.ImagePath) && File.Exists(order.ImagePath))
                                    {
                                        c.Item().PaddingVertical(5).AlignCenter().Width(120).Image(order.ImagePath);
                                    }

                                    if (!string.IsNullOrEmpty(order.MaterialNo)) c.Item().PaddingTop(5).Text($"Mat no - {order.MaterialNo}").FontSize(10);
                                    if (!string.IsNullOrEmpty(order.CostingLayer)) c.Item().Text($"Costing Layer - {order.CostingLayer}").FontSize(10);
                                    if (!string.IsNullOrEmpty(order.Color)) c.Item().Text($"Coloure - {order.Color}").FontSize(10);
                                });
                                table.Cell().Border(1).AlignCenter().Text(order.Size ?? "-").FontSize(10);
                                table.Cell().Border(1).AlignCenter().Text(order.UOM ?? "PC").FontSize(10);
                                table.Cell().Border(1).AlignCenter().Text(order.Quantity.ToString()).FontSize(10);
                                table.Cell().Border(1).AlignCenter().Text("-").FontSize(10);
                                table.Cell().Border(1).AlignCenter().Text("-").FontSize(10);

                                // Total Row
                                table.Cell().ColumnSpan(5).Border(0).Text("");
                                table.Cell().ColumnSpan(2).Border(1).AlignRight().PaddingRight(5).Text($"Total Amt = {order.OrderAmount:C}").Bold().FontSize(12);
                            });

                            // Specifications & Sign-off Section
                            col.Item().PaddingTop(30).Row(row =>
                            {
                                // Specs (Left)
                                row.RelativeItem(2).Column(c =>
                                {
                                    c.Item().Text("Specifications:").FontSize(14).Bold().Underline();
                                    
                                    void AddSpecLine(string label, string? value)
                                    {
                                        c.Item().PaddingTop(2).Row(r =>
                                        {
                                            r.ConstantItem(80).Text(label).FontSize(10).Bold();
                                            r.ConstantItem(10).Text(":").FontSize(10);
                                            r.RelativeItem().Text(value ?? "-").FontSize(10);
                                        });
                                    }

                                    AddSpecLine("Material", order.MaterialSpec);
                                    AddSpecLine("Paint", order.PaintSpec);
                                    AddSpecLine("Quality", order.QualitySpec);
                                    AddSpecLine("Work Nature", order.WorkNatureSpec);
                                    AddSpecLine("Durability", order.DurabilitySpec);
                                });

                                // Sign-offs (Right)
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Order by").FontSize(9).Bold();
                                    c.Item().Text(order.OrderBy ?? "Sandeep Patel").FontSize(10).SemiBold();

                                    c.Item().PaddingTop(30).Text($"Modling........................").FontSize(9);
                                    c.Item().PaddingTop(5).Text(order.ModelingBy ?? "").FontSize(9).AlignCenter();

                                    c.Item().PaddingTop(30).Text($"Fiber............................").FontSize(9);
                                    c.Item().PaddingTop(5).Text(order.FiberBy ?? "").FontSize(9).AlignCenter();
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text("Add: Near Ibrahimabad Bridge Maulabad Post-Garhi, Block-Trivediganj, Barabanki (U.P.)- 225126")
                            .FontSize(9).SemiBold();
                    });
                });
                document.GeneratePdf(filePath);
            });
        }
    }
}
