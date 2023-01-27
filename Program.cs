using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using GoogleSheetAndCsharp.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffMonitor.Helpers;

namespace GoogleSheetAndCsharp
{
    public class Program
    {
        private static readonly string ApplicationName = "TinkoffMonitor";

        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        private static IConfigurationRoot configuration;

        private static MarketSnapshot market;

        private static SandboxContext context;

        private static SheetsService service;

        private static string TinkoffToken => configuration.GetSection("TinkoffToken").Value;

        private static string SpreadsheetId => configuration.GetSection("SpreadsheetId").Value;

        public static void Main(string[] args)
        {
            Console.WriteLine("Loading...");

            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            InitialRun().Wait();

            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose an option from the list: ");
            Console.WriteLine("1. Portfolio return");
            Console.WriteLine("0. Exit");
            Console.Write("\r\nYour choice: ");

            var result = Console.ReadKey();
            switch (result.KeyChar)
            {
                case '1':
                    AllProfitabilityByPortfolios();
                    break;
                case '0':
                    return false;
            }

            return true;
        }

        private static async Task<bool> InitialRun()
        {
            // Sandbox
            var connection = ConnectionFactory.GetSandboxConnection(TinkoffToken);
            context = connection.Context;

            // Google Sheets
            GoogleCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            market = new MarketSnapshot();
            market.Portfolios = new Dictionary<string, List<PortfolioItem>>();

            var sheetMetadata = service.Spreadsheets.Get(SpreadsheetId).Execute();

            foreach (var item in sheetMetadata.Sheets.Select(it => it.Properties.Title))
            {
                market.Portfolios.Add(item, UploadPortfolio(item));
            }

            return true;
        }

        public static void AllProfitabilityByPortfolios()
        {
            Console.Clear();

            foreach (var item in market.Portfolios)
            {
                ProfitabilityByPortfolios(item.Key, item.Value);
            }

            Console.ReadLine();
        }

        public static async void ProfitabilityByPortfolios(string title, List<PortfolioItem> portfolios)
        {
            Console.WriteLine($"List: {title}");

            foreach (var item in portfolios)
            {
                var instruments = await context.MarketSearchByTickerAsync(item.Ticker);

                if (instruments.Instruments.Any())
                {
                    var first = instruments.Instruments.First();

                    var candles = await context.MarketCandlesAsync(instruments.Instruments[0].Figi, DateTime.Now.AddMonths(-1), DateTime.Now, CandleInterval.Day);

                    if (candles.Candles.Any())
                    {
                        var currentPrice = (double)candles.Candles.LastOrDefault().Close;
                        var name = $"{Strings.Truncate(first.Name, 25)} ({ first.Ticker})";
                        var profit = (currentPrice * item.Count) - (item.Price * item.Count);

                        Console.WriteLine($"{name.PadRight(30)} price: {currentPrice} profit: {Math.Round(profit)}");
                    }
                    else
                    {
                        Console.WriteLine($"{item.Ticker} not found candles");
                    }
                }
                else
                {
                    Console.WriteLine($"{item.Ticker} not found");
                }
            }

            Console.WriteLine("");
        }

        public static List<PortfolioItem> UploadPortfolio(string sheet)
        {
            var result = new List<PortfolioItem>();

            var range = $"{sheet}!A1:D1000";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            var values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    var item = new PortfolioItem();
                    item.Ticker = Strings.Get<string>(row[0]);
                    item.Count = Strings.Get<int>(row[1].ToString().Replace(',', '.'));
                    item.Price = Strings.Get<double>(row[2].ToString().Replace(',', '.'));

                    result.Add(item);
                }
            }
            else
            {
                Console.WriteLine("No data found");
            }

            return result;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
