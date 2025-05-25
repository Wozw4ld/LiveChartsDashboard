using System.Collections.Generic;
using System.Data.SQLite;

public class DatabaseManager
{
	private const string ConnectionString = "Data Source=crypto.db;Version=3;";

	public DatabaseManager()
	{
		InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		using var conn = new SQLiteConnection(ConnectionString);
		conn.Open();

		const string query = @"
            CREATE TABLE IF NOT EXISTS History (
                CoinId TEXT,
                Timestamp DATETIME,
                Price DECIMAL,
                Volume DECIMAL
            )";

		new SQLiteCommand(query, conn).ExecuteNonQuery();
	}

	public void SaveData(List<PricePoint> data, string coinId)
	{
		using var conn = new SQLiteConnection(ConnectionString);
		conn.Open();

		foreach (var point in data)
		{
			const string query = @"
                INSERT INTO History 
                (CoinId, Timestamp, Price, Volume)
                VALUES (@coinId, @ts, @price, @vol)";

			var cmd = new SQLiteCommand(query, conn);
			cmd.Parameters.AddWithValue("@coinId", coinId);
			cmd.Parameters.AddWithValue("@ts", point.Timestamp);
			cmd.Parameters.AddWithValue("@price", point.Price);
			cmd.Parameters.AddWithValue("@vol", point.Volume);
			cmd.ExecuteNonQuery();
		}
	}
}