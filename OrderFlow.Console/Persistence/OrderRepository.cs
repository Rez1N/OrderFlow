using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public sealed class OrderRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
    {
        EnsureParentDirectory(path);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, orders.ToList(), JsonOptions);
    }

    public async Task<List<Order>> LoadFromJsonAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var loaded = await JsonSerializer.DeserializeAsync<List<Order>>(stream, JsonOptions);
        return loaded ?? new List<Order>();
    }

    public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
    {
        EnsureParentDirectory(path);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        var serializer = new XmlSerializer(typeof(List<Order>));
        serializer.Serialize(stream, orders.ToList());
        await stream.FlushAsync();
    }

    public async Task<List<Order>> LoadFromXmlAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var serializer = new XmlSerializer(typeof(List<Order>));
        var loaded = serializer.Deserialize(stream) as List<Order>;
        await Task.CompletedTask;
        return loaded ?? new List<Order>();
    }

    private static void EnsureParentDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}
