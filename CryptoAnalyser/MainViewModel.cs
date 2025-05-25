using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using CryptoAnalyser;
using LiveCharts.Defaults;
using System.Windows.Media;

public class MainViewModel : INotifyPropertyChanged
{
	public SeriesCollection ChangesSeries { get; set; }
	public string[] PeriodLabels { get; } = { "Неделя", "Месяц", "Год" };
	public Brush[] ChartColors { get; private set; }

	private void UpdateChangesChart(List<PricePoint> data)
	{
		var changes = new double[3];

		if (data.Count > 1)
		{
			changes[0] = CalculateChange(data[0].Price, data[Math.Min(6, data.Count - 1)].Price);
			changes[1] = CalculateChange(data[0].Price, data[Math.Min(29, data.Count - 1)].Price);
			changes[2] = CalculateChange(data[0].Price, data.Last().Price);
		}

		ChartColors = changes.Select(c => c >= 0 ? Brushes.Green : Brushes.Red).ToArray();

		ChangesSeries = new SeriesCollection
		{
			new LineSeries
			{
				Values = new ChartValues<double>(changes),
				PointGeometry = DefaultGeometries.Circle,
				Stroke = Brushes.Gray
			}
		};

		OnPropertyChanged(nameof(ChangesSeries));
		OnPropertyChanged(nameof(ChartColors));
	}

	private double CalculateChange(decimal start, decimal end)
	{
		return start == 0 ? 0 : (double)((end - start) / start * 100);
	}
	public MainViewModel()
	{
		WeeklyChangeSeries = new SeriesCollection();
		MonthlyChangeSeries = new SeriesCollection();
		YearlyChangeSeries = new SeriesCollection();
	}
	public event PropertyChangedEventHandler PropertyChanged;
	public Func<double, string> PercentageFormatter { get; } =
	value => value.ToString("N2") + " %";
	public List<string> CoinList { get; } = new List<string> { "bitcoin", "ethereum", "cardano" };
	
	private string _selectedCoin;
	public string SelectedCoin
	{
		get => _selectedCoin;
		set
		{
			_selectedCoin = value;
			OnPropertyChanged(nameof(SelectedCoin));
		}
	}
	public ICommand AnalyzeCommand => new RelayCommand(async _ =>
	{
		if (string.IsNullOrEmpty(SelectedCoin))
		{
			MessageBox.Show("Выберите монету!");
			return;
		}
		await LoadData();
	});
	
	public SeriesCollection WeeklyChangeSeries { get; set; }
	public SeriesCollection MonthlyChangeSeries { get; set; }
	public SeriesCollection YearlyChangeSeries { get; set; }
	public SeriesCollection PriceSeries { get; set; }
	public SeriesCollection VolumeSeries { get; set; }
	public SeriesCollection SMASeries { get; set; }

	public Func<double, string> DateFormatter { get; set; } =
		value => new DateTime((long)value).ToString("dd MMM");

	

	private async Task LoadData()
	{
		try
		{
			var service = new CryptoService();
			var data = await service.GetHistoricalData(SelectedCoin, 30);

			var db = new DatabaseManager();
			db.SaveData(data, SelectedCoin);

			UpdateCharts(data);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Ошибка: {ex.Message}");
		}
	}

	private void UpdateCharts(List<PricePoint> data)
	{
		PriceSeries = new SeriesCollection
		{
			new LineSeries
			{
				Title = "Цена",
				Values = new ChartValues<DateTimePoint>(
					data.Select(p => new DateTimePoint(p.Timestamp, (double)p.Price)))

			}
		};

		VolumeSeries = new SeriesCollection
		{
			new ColumnSeries
			{
				Title = "Объем",
				Values = new ChartValues<double>(data.Select(p => (double)p.Volume))
			}
		};

		var sma = CalculateSMA(data, 5);
		SMASeries = new SeriesCollection
		{
			new LineSeries
			{
				Title = "SMA 5",
				Values = new ChartValues<DateTimePoint>(
					sma.Select(p => new DateTimePoint(p.Timestamp, (double)p.Price)))

			}
		};
		var changes = CalculatePriceChanges(data);
		UpdatePercentageCharts(changes);
		OnPropertyChanged(nameof(PriceSeries));
		OnPropertyChanged(nameof(VolumeSeries));
		OnPropertyChanged(nameof(SMASeries));
	}
	private void UpdatePercentageCharts(PriceChanges changes)
	{
		WeeklyChangeSeries = new SeriesCollection
		{
			new RowSeries
			{
				Title = "Неделя",
				Values = new ChartValues<double> { changes.WeeklyChange }
			}
		};

		MonthlyChangeSeries = new SeriesCollection
		{
			new RowSeries
			{
				Title = "Месяц",
				Values = new ChartValues<double> { changes.MonthlyChange }
			}
		};

		YearlyChangeSeries = new SeriesCollection
		{
			new RowSeries
			{
				Title = "Год",
				Values = new ChartValues<double> { changes.YearlyChange }
			}
		};

		OnPropertyChanged(nameof(WeeklyChangeSeries));
		OnPropertyChanged(nameof(MonthlyChangeSeries));
		OnPropertyChanged(nameof(YearlyChangeSeries));
	}
	private List<PricePoint> CalculateSMA(List<PricePoint> data, int period)
	{
		return data
			.Skip(period - 1)
			.Select((x, i) => new PricePoint
			{
				Timestamp = x.Timestamp,
				Price = data.Skip(i).Take(period).Average(p => p.Price)
			})
			.ToList();
	}

	protected void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
	private PriceChanges CalculatePriceChanges(List<PricePoint> data)
	{
		if (data == null || data.Count < 2)
			return new PriceChanges();

		var latest = data.Last().Price;

		return new PriceChanges
		{
			WeeklyChange = CalculateChange(latest, data, 7),
			MonthlyChange = CalculateChange(latest, data, 30),
			YearlyChange = CalculateChange(latest, data, 365)
		};
	}

	private double CalculateChange(decimal latest, List<PricePoint> data, int daysBack)
	{
		var oldData = data.FirstOrDefault(p => p.Timestamp <= DateTime.Now.AddDays(-daysBack));
		if (oldData?.Price == 0 || oldData == null) return 0;

		return (double)((latest - oldData.Price) / oldData.Price * 100);
	}
}

public class PriceChanges
{
	public double WeeklyChange { get; set; }
	public double MonthlyChange { get; set; }
	public double YearlyChange { get; set; }
}


public class RelayCommand : ICommand
{
	private readonly Action<object> _execute;
	public event EventHandler CanExecuteChanged;

	public RelayCommand(Action<object> execute) => _execute = execute;
	public bool CanExecute(object parameter) => true;
	public void Execute(object parameter) => _execute(parameter);
}