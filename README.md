

# Crypto Analyzer

WPF-приложение для анализа криптовалют с неоновым дизайном и интерактивными графиками

![image_alt](https://github.com/Wozw4ld/LiveChartsDashboard/blob/main/docs/image1.png?raw=true)

## Основные возможности
- Выбор криптовалюты из списка (BTC, ETH и др.)
- Отображение данных в 4 типах графиков:
  - Историческая цена
  - Торговые объемы
  - Скользящее среднее (SMA)
  - Процентные изменения за периоды
- Неоновый UI с анимациями
- Локальное кэширование данных в SQLite
- Автоматическое обновление графиков

## Технологии
- WPF (.NET Framework 4.8+)
- LiveCharts.Wpf для визуализации
- CoinGecko API
- SQLite без Entity Framework
- MVVM-паттерн
![image_alt](https://github.com/Wozw4ld/LiveChartsDashboard/blob/main/docs/image2.png?raw=true)
## Файловая структура

### Подробное описание:

1. **Services/**
   - **CryptoService.cs**  
     - Запросы к CoinGecko API через HttpClient  
     - Парсинг JSON-ответов в модели данных  
     - Обработка ошибок сетевых запросов

   - **DatabaseManager.cs**  
     - Создание/обновление SQLite-базы  
     - Локальное кэширование исторических данных  
     - CRUD-операции без использования Entity Framework

2. **ViewModels/**
   - **MainViewModel.cs**  
     - Реализация INotifyPropertyChanged  
     - Команды для кнопок (ICommand)  
     - Логика обновления графиков  
     - Расчет технических показателей (SMA, проценты)

3. **Views/**
   - **MainWindow.xaml**  
     - Основная разметка интерфейса  
     - Привязки к свойствам ViewModel  
     - Стилизация элементов управления

4. **Models/**
   - **CryptoData.cs**  
     - Структура ответа API (цены, объемы, даты)  
     - JSON-парсинг через Newtonsoft.Json

   - **PricePoint.cs**  
     - Хранение данных одной точки:  
       `DateTime Timestamp`  
       `decimal Price`  
       `decimal Volume`

5. **Assets/**
   - Хранит статические ресурсы:  
     - Векторные иконки (.ico, .png)  
     - Стили XAML (если используются)  
     - Шрифты (для неоновых надписей)

6. **App.xaml**  
   - Глобальные стили приложения  
   - Ресурсы, доступные во всех окнах  
   - Настройка темы и цветовой схемы
