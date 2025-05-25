using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class CryptoService
{
	private readonly HttpClient _client = new HttpClient();
	private const string ApiUrl = "https://api.coingecko.com/api/v3/coins/";

	public async Task<List<PricePoint>> GetHistoricalData(string coinId, int days)
	{
		if (string.IsNullOrEmpty(coinId))
			throw new ArgumentException("Coin ID не может быть пустым");

		var response = await _client.GetStringAsync($"{ApiUrl}{coinId}/market_chart?vs_currency=usd&days={days}");

		if (string.IsNullOrEmpty(response))
			throw new Exception("Пустой ответ от API");

		var data = JsonConvert.DeserializeObject<CryptoData>(response);

		return data?.Prices?
			.Where(p => p != null && p.Length >= 2) 
			.Select(p => new PricePoint
			{
				Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)p[0]).DateTime,
				Price = p[1],
				Volume = data.TotalVolumes?
					.FirstOrDefault(v => v?[0] == p[0])?[1] ?? 0
			})
			.ToList() ?? new List<PricePoint>(); 
	}
}

public class CryptoData
{
	public decimal[][] Prices { get; set; }
	public decimal[][] TotalVolumes { get; set; }
}

public class PricePoint
{
	public DateTime Timestamp { get; set; }
	public decimal Price { get; set; }
	public decimal Volume { get; set; }
}