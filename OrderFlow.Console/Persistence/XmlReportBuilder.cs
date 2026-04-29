using System.Globalization;
using System.Xml.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public sealed class XmlReportBuilder
{
    public XDocument BuildReport(IEnumerable<Order> orders)
    {
        var orderList = orders.ToList();
        var byStatus = orderList
            .GroupBy(o => o.Status)
            .OrderBy(g => g.Key.ToString())
            .Select(g => new XElement("status",
                new XAttribute("name", g.Key.ToString()),
                new XAttribute("count", g.Count()),
                new XAttribute("revenue", g.Sum(o => o.TotalAmount).ToString("0.00", CultureInfo.InvariantCulture))));

        var byCustomer = orderList
            .GroupBy(o => new { o.CustomerId, Name = o.Customer?.Name ?? $"Customer-{o.CustomerId}", IsVip = o.Customer?.IsVip ?? false })
            .OrderBy(g => g.Key.Name)
            .Select(g => new XElement("customer",
                new XAttribute("id", g.Key.CustomerId),
                new XAttribute("name", g.Key.Name),
                new XAttribute("isVip", g.Key.IsVip),
                new XElement("orderCount", g.Count()),
                new XElement("totalSpent", g.Sum(o => o.TotalAmount).ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("orders",
                    g.OrderBy(o => o.Id)
                        .Select(o => new XElement("orderRef",
                            new XAttribute("id", o.Id),
                            new XAttribute("total", o.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture)))))));

        return new XDocument(
            new XElement("report",
                new XAttribute("generated", DateTime.UtcNow.ToString("O")),
                new XElement("summary",
                    new XAttribute("totalOrders", orderList.Count),
                    new XAttribute("totalRevenue", orderList.Sum(o => o.TotalAmount).ToString("0.00", CultureInfo.InvariantCulture))),
                new XElement("byStatus", byStatus),
                new XElement("byCustomer", byCustomer)));
    }

    public async Task SaveReportAsync(XDocument report, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await report.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(string reportPath, decimal threshold)
    {
        if (!File.Exists(reportPath))
            return Enumerable.Empty<int>();

        await using var stream = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var report = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        return report
            .Descendants("orderRef")
            .Where(x =>
            {
                var total = x.Attribute("total")?.Value;
                return decimal.TryParse(total, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed > threshold;
            })
            .Select(x => int.TryParse(x.Attribute("id")?.Value, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id.GetValueOrDefault())
            .ToList();
    }
}
