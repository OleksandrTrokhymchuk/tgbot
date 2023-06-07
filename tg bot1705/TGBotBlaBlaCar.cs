using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using tg_bot1705;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Globalization;
using System.Threading;

//бот

namespace KURSOVA
{
    public class TGBotBlaBlaCar
    {




        TelegramBotClient botClient = new TelegramBotClient("6169669274:AAGv3BpF6rhGrk2xGHHg9RR4cf05tEsWF0k");
        //SearchTrip trip = new SearchTrip();
        //SearchCity city = new SearchCity();
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        SearchTripClient searchTripClient = new SearchTripClient();
        SearchCityClient searchCityClient = new SearchCityClient();

        private Dictionary<long, UserParams> userParams = new Dictionary<long, UserParams>();

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadLine();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот API:\n{apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
        }



        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {

            if (!userParams.ContainsKey(message.Chat.Id))
            {
                userParams.Add(message.Chat.Id, new UserParams());
            }

            UserParams userParameters = userParams[message.Chat.Id];

            Console.WriteLine($"{message.Text}");
            foreach (var pair in userParams)
            {
                Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
            }

            if (message.Type == MessageType.Text && !Regex.IsMatch(message.Text, @"[\uD83C-\uDBFF\uDC00-\uDFFF]+"))
            {
                if (message.Text == "/start" || userParameters.operation == "start" || message.Text == "Розпочати пошук спочатку")
                {
                    userParameters.operation = null;
                    userParameters.requestByUser = null;
                    ReplyKeyboardMarkup replyKeyboardMarkup = new
                      (
                      new[]
                          {
                        new KeyboardButton[] { "Конкретний день", "Одразу на декілька днів" },
                          }
                      )
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ласкаво просимо у телеграм-бот BlaBlaCar!\n"
                        + "Бажаєте знайти поїздку на конкретний день, чи можливо переглянути поїздки одразу на декілька днів?\n"
                        + "Для цього виберіть один з пунктів меню: ", replyMarkup: replyKeyboardMarkup);
                }
                else if (message.Text == "Конкретний день")
                {
                    userParameters.requestByUser = "FindTripsOnConcreteDate";
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть місто відправлення");
                    userParameters.operation = "EnterFinishCity";
                }
                else if (message.Text == "Одразу на декілька днів")
                {
                    userParameters.requestByUser = "FindTripsOnSomeDates";
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть місто відправлення");
                    userParameters.operation = "EnterFinishCity";
                }
                else if (userParameters.requestByUser == "FindTripsOnConcreteDate")
                {
                    if (userParameters.operation == "EnterFinishCity")
                    {
                        userParameters.coor1 = userParameters.StartCity = message.Text;
                        string checkingMessage = message.Text.Replace("'", "").Replace("-", "");
                        bool containsDigitsOrSymbols = Regex.IsMatch(checkingMessage, @"[\d\p{P}]");
                        if (containsDigitsOrSymbols)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ввели некоректні дані. Спробуйте ще раз");
                        }
                        else
                        {
                            await searchCityClient.SearchSomeCityAsync(userParameters.coor1, message.Chat.Id);
                            List<CityMain> lcm = searchCityClient.GetStatistCityAsync(message.Chat.Id).Result;

                            foreach (CityMain i in lcm)
                            {
                                userParameters.coor1 = i.Coordinates;
                            }
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Вкажіть місто прибуття");
                            userParameters.operation = "EnterMonth";
                        }
                    }

                    else if (userParameters.operation == "EnterMonth")
                    {
                        bool TrueOrFalse = true;
                        if (message.Text != "Змінити дату поїздки")
                        {
                            userParameters.coor2 = userParameters.FinishCity = message.Text;

                            string checkingMessage = message.Text.Replace("'", "").Replace("-", "");
                            bool containsDigitsOrSymbols = Regex.IsMatch(checkingMessage, @"[\d\p{P}]");
                            if (containsDigitsOrSymbols)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ввели некоректні дані. Спробуйте ще раз");
                                TrueOrFalse = false;
                            }
                            else
                            {
                                await searchCityClient.SearchSomeCityAsync(userParameters.coor2, message.Chat.Id);
                                List<CityMain> lcm = searchCityClient.GetStatistCityAsync(message.Chat.Id).Result;

                                foreach (CityMain i in lcm)
                                {
                                    userParameters.coor2 = i.Coordinates;
                                }
                            }
                        }
                        if (TrueOrFalse)
                        {
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                            (
                            new[]
                                {
                          new KeyboardButton[] { "Січень", "Лютий", "Березень", "Квітень" },
                          new KeyboardButton[] { "Травень", "Червень", "Липень", "Серпень" },
                          new KeyboardButton[] { "Вересень", "Жовтень", "Листопад", "Грудень" },
                                }
                            )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Чудово! Визначимось із датою поїздки. Для початку оберіть потрібний Вам місяць:", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "EnterDay";
                        }

                    }
                    else if (userParameters.operation == "EnterDay")
                    {
                        string[] months = { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" };
                        if (months.Contains(message.Text))
                        {
                            userParameters.monthOnConcreteDateOnTrip = message.Text;
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                               (
                               new[]
                                   {
                           new KeyboardButton[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" },
                           new KeyboardButton[] { "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                           new KeyboardButton[] { "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" },
                               }
                               )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"І на яке число?", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "ShowRequest";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний місяць. Спробуйте ще раз");
                        }
                    }
                    else if (userParameters.operation == "ShowRequest")
                    {
                        string[] days = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
                                      "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                      "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };

                        if (days.Contains(message.Text))
                        {
                            userParameters.dayOnConcreteDateOnTrip = message.Text;
                            if (userParameters.monthOnConcreteDateOnTrip == "Січень") { userParameters.DateTimeOnConcreteDateOnTrip = $"01-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Лютий") { userParameters.DateTimeOnConcreteDateOnTrip = $"02-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Березень") { userParameters.DateTimeOnConcreteDateOnTrip = $"03-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Квітень") { userParameters.DateTimeOnConcreteDateOnTrip = $"04-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Травень") { userParameters.DateTimeOnConcreteDateOnTrip = $"05-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Червень") { userParameters.DateTimeOnConcreteDateOnTrip = $"06-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Липень") { userParameters.DateTimeOnConcreteDateOnTrip = $"07-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Серпень") { userParameters.DateTimeOnConcreteDateOnTrip = $"08-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Вересень") { userParameters.DateTimeOnConcreteDateOnTrip = $"09-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Жовтень") { userParameters.DateTimeOnConcreteDateOnTrip = $"10-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else if (userParameters.monthOnConcreteDateOnTrip == "Листопад") { userParameters.DateTimeOnConcreteDateOnTrip = $"11-{userParameters.dayOnConcreteDateOnTrip}"; }
                            else { userParameters.DateTimeOnConcreteDateOnTrip = $"12-{userParameters.dayOnConcreteDateOnTrip}"; }

                            DateTime dateOnConcreteTrip = new DateTime(2023, int.Parse(userParameters.DateTimeOnConcreteDateOnTrip.Split('-')[0]), int.Parse(userParameters.dayOnConcreteDateOnTrip));

                            if (dateOnConcreteTrip < DateTime.Now)
                            {
                                ReplyKeyboardMarkup replyKeyboardMarkup = new
                                            (
                                            new[]
                                                {
                                             new KeyboardButton[] { "Змінити дату поїздки", "Розпочати пошук спочатку" },
                                                   }
                                                   )
                                {

                                    ResizeKeyboard = true

                                };
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"На жаль, Ви ввели не правильну дату. Поїздки за минулі дати шукати не можна, спробуйте інші дати", replyMarkup: replyKeyboardMarkup);
                                userParameters.operation = "EnterMonth";
                            }
                            else
                            {
                                await searchTripClient.SearchSomeTripAsync(userParameters.coor1, userParameters.coor2, userParameters.DateTimeOnConcreteDateOnTrip, message.Chat.Id);
                                List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                                bool showRequest = true;
                                bool TrueOrFalse = true;

                                string result = null;
                                string resultFirstTen = null;
                                string resultSecondTen = null;
                                string resultThirdTen = null;
                                string resultFourthTen = null;
                                string resultFifthTen = null;
                                string resultSixthTen = null;
                                string resultSeventhTen = null;
                                string resultEighthTen = null;
                                string resultNinethTen = null;
                                string resultTenthTen = null;

                                foreach (TripMain i in ltm)
                                {

                                    if (i.InfoAboutTrip == "0 поїздок")
                                    {
                                        ReplyKeyboardMarkup replyKeyboardMarkup = new
                                            (
                                            new[]
                                                {
                                             new KeyboardButton[] { "Змінити дату поїздки", "Розпочати пошук спочатку" },
                                                   }
                                                   )
                                        {
                                            ResizeKeyboard = true
                                        };
                                        await botClient.SendTextMessageAsync(message.Chat.Id, $"На жаль, поїздок на вказану дату не знайдено. Будь ласка, спробуйте іншу дату", replyMarkup: replyKeyboardMarkup);
                                        userParameters.operation = "EnterMonth";
                                        TrueOrFalse = false;
                                    }
                                    else
                                    {
                                        if (showRequest)
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"{i.StatusOfRequest}");
                                            showRequest = false;
                                        }
                                        result += $"{i.InfoAboutTrip}Ціна: {i.Price} грн\n\n\n";
                                        if (result.Contains("Поїздка №9")) { resultFirstTen = result; result = ""; }
                                        if (result.Contains("Поїздка №19")) { resultSecondTen = result; result = ""; }
                                        if (result.Contains("Поїздка №29")) { resultThirdTen = result; result = ""; }
                                        if (result.Contains("Поїздка №39")) { resultFourthTen = result; result = ""; }
                                        if (result.Contains("Поїздка №49")) { resultFifthTen = result; result = ""; }
                                        if (result.Contains("Поїздка №59")) { resultSixthTen = result; result = ""; }
                                        if (result.Contains("Поїздка №69")) { resultSeventhTen = result; result = ""; }
                                        if (result.Contains("Поїздка №79")) { resultEighthTen = result; result = ""; }
                                        if (result.Contains("Поїздка №89")) { resultNinethTen = result; result = ""; }
                                        if (result.Contains("Поїздка №99")) { resultTenthTen = result; result = ""; }
                                        TrueOrFalse = true;
                                    }
                                }
                                if (TrueOrFalse)
                                {
                                    if (resultFirstTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFirstTen); }
                                    if (resultSecondTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSecondTen); }
                                    if (resultThirdTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultThirdTen); }
                                    if (resultFourthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFourthTen); }
                                    if (resultFifthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFifthTen); }
                                    if (resultSixthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSixthTen); }
                                    if (resultSeventhTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSeventhTen); }
                                    if (resultEighthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultEighthTen); }
                                    if (resultNinethTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultNinethTen); }
                                    if (resultTenthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultTenthTen); }
                                    if (result != "") { await botClient.SendTextMessageAsync(message.Chat.Id, result); }
                                }
                                if (TrueOrFalse)
                                {
                                    ReplyKeyboardMarkup replyKeyboardMarkup = new
                                          (
                                          new[]
                                              {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                              }
                                          )
                                    {
                                        ResizeKeyboard = true
                                    };

                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ось результати пошуку :)\n"
                                        + "Для продовження натисніть на відповідну кнопку в меню.", replyMarkup: replyKeyboardMarkup);
                                    userParameters.operation = "SomeOperations";
                                }
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильне число місяця. Спробуйте ще раз");
                        }
                    }
                    else if (userParameters.operation == "SomeOperations")
                    {
                        if (message.Text == "Розпочати пошук спочатку")
                        {
                            userParameters.operation = "start";
                        }
                        else if (message.Text == "Відсортувати поїздки по цінам")
                        {
                            bool showRequest = true;
                            List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                            List<int> price = new List<int>();
                            List<string> infoAboutTrip = new List<string>();
                            foreach (TripMain i in ltm)
                            {
                                price.Add(i.Price);
                                infoAboutTrip.Add(i.InfoAboutTrip);
                            }
                            List<string> sortedTrips = infoAboutTrip.Zip(price, (trip, p) => new { Trip = trip, Price = p })
                                                                    .OrderBy(x => x.Price)
                                                                    .Select(x => x.Trip)
                                                                    .ToList();
                            price.Sort();


                            string result = null;
                            string resultFirstTen = null;
                            string resultSecondTen = null;
                            string resultThirdTen = null;
                            string resultFourthTen = null;
                            string resultFifthTen = null;
                            string resultSixthTen = null;
                            string resultSeventhTen = null;
                            string resultEighthTen = null;
                            string resultNinethTen = null;
                            string resultTenthTen = null;

                            for (int i = 0; i < infoAboutTrip.Count; i++)
                            {
                                //await botClient.SendTextMessageAsync(message.Chat.Id, sortedTrips[i] + "Ціна: " + price[i] + " грн");
                                result += sortedTrips[i] + "Ціна: " + price[i] + " грн\n\n\n";
                                if (i == 9) { resultFirstTen = result; result = ""; }
                                if (i == 19) { resultSecondTen = result; result = ""; }
                                if (i == 29) { resultThirdTen = result; result = ""; }
                                if (i == 39) { resultFourthTen = result; result = ""; }
                                if (i == 49) { resultFifthTen = result; result = ""; }
                                if (i == 59) { resultSixthTen = result; result = ""; }
                                if (i == 69) { resultSeventhTen = result; result = ""; }
                                if (i == 79) { resultEighthTen = result; result = ""; }
                                if (i == 89) { resultNinethTen = result; result = ""; }
                                if (i == 99) { resultTenthTen = result; result = ""; }

                            }
                            if (resultFirstTen != null) await botClient.SendTextMessageAsync(message.Chat.Id, resultFirstTen);
                            if (resultSecondTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSecondTen); }
                            if (resultThirdTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultThirdTen); }
                            if (resultFourthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFourthTen); }
                            if (resultFifthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFifthTen); }
                            if (resultSixthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSixthTen); }
                            if (resultSeventhTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSeventhTen); }
                            if (resultEighthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultEighthTen); }
                            if (resultNinethTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultNinethTen); }
                            if (resultTenthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultTenthTen); }
                            if (result != null) { await botClient.SendTextMessageAsync(message.Chat.Id, result); }

                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                         (
                                         new[]
                                             {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                             }
                                         )
                            {
                                ResizeKeyboard = true
                            };

                            await botClient.SendTextMessageAsync(message.Chat.Id, "Поїздки відсортовані по цінам (спочатку найдешевші на кожний день)! Для продовження, натисність на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else if (message.Text == "Переглянути збережені поїздки")
                        {
                            bool TrueOrFalse = true;
                            List<FavoriteTripMain> lftm = searchTripClient.GetStatistFavoriteTripAsync(message.Chat.Id).Result;
                            foreach (FavoriteTripMain i in lftm)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, i.Trip);
                                TrueOrFalse = false;
                            }
                            if (TrueOrFalse)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Наразі список збережених поїздок порожній");
                            }
                            userParameters.operation = "SomeOperations";
                        }
                        else if (message.Text == "Зберегти поїздку")
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть номер поїздки");
                            userParameters.operation = "SaveNumberOfTrip";
                        }
                        else if (message.Text == "Видалити збережені поїздки")
                        {
                            await searchTripClient.DeleteListOfFavoriteTripsAsync(message.Chat.Id);
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                        (
                                        new[]
                                            {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                            }
                                        )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Список збережених поїздок тепер порожній. Для продовження, натисність на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Помилка. Використовуйте кнопки в меню");

                        }
                    }
                    else if (userParameters.operation == "SaveNumberOfTrip")
                    {
                        string[] days = new string[100];

                        for (int i = 1; i < days.Length; i++)
                        {
                            days[i] = i.ToString();
                        }

                        bool TrueOfFalse = true;
                        List<TripMain> ltmforchecking = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                        foreach (TripMain i in ltmforchecking)
                        {
                            if (i.InfoAboutTrip.Contains($"Поїздка №{message.Text}"))
                            {
                                TrueOfFalse = true;
                                break;
                            }
                            else { TrueOfFalse = false; }
                        }


                        if (days.Contains(message.Text) && TrueOfFalse)
                        {

                            DateTime dateTime = DateTime.ParseExact(userParameters.DateTimeOnConcreteDateOnTrip, "MM-dd", null);

                            string dataForTg = dateTime.ToString("d MMMM", new System.Globalization.CultureInfo("uk-UA"));

                            userParameters.numberOfTrip.Add(message.Text);
                            List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                            foreach (TripMain i in ltm)
                            {
                                if (i.InfoAboutTrip.Contains($"Поїздка №{message.Text}"))
                                {
                                    userParameters.favotireTrip = i.InfoAboutTrip + $"Ціна: {i.Price} грн\nПоїздка з міста {userParameters.StartCity} у місто {userParameters.FinishCity} на {dataForTg}";
                                    break;
                                }
                            }

                            await searchTripClient.SaveFavoriteTripAsync(userParameters.favotireTrip, message.Chat.Id);
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                         (
                                         new[]
                                             {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                             }
                                         )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Поїздку №{message.Text} збережено. Для продовження - натисніть на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else
                        {
                            if (!TrueOfFalse)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний номер поїздки. Можливо такої поїздки не існує. Спробуйте ще раз");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний номер поїздки. Просто вкажіть цифру. Наприклад: '4'");
                            }
                        }
                    }

                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Помилка. Використовуйте кнопки в меню");
                    }
                }





                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////






                else if (userParameters.requestByUser == "FindTripsOnSomeDates")
                {
                    if (userParameters.operation == "EnterFinishCity")
                    {
                        userParameters.coor1 = userParameters.StartCity = message.Text;
                        string checkingMessage = message.Text.Replace("'", "").Replace("-", "");
                        bool containsDigitsOrSymbols = Regex.IsMatch(checkingMessage, @"[\d\p{P}]");
                        if (containsDigitsOrSymbols)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ввели некоректні дані. Спробуйте ще раз");
                        }
                        else
                        {
                            await searchCityClient.SearchSomeCityAsync(userParameters.coor1, message.Chat.Id);
                            List<CityMain> lcm = searchCityClient.GetStatistCityAsync(message.Chat.Id).Result;

                            foreach (CityMain i in lcm)
                            {
                                userParameters.coor1 = i.Coordinates;
                            }
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Вкажіть місто прибуття");
                            userParameters.operation = "EnterMonthOnFirstDateOnTrip";
                        }
                    }

                    else if (userParameters.operation == "EnterMonthOnFirstDateOnTrip")
                    {
                        bool TrueOrFalse = true;
                        if (message.Text != "Змінити дату поїздки")
                        {
                            userParameters.coor2 = userParameters.FinishCity = message.Text;

                            string checkingMessage = message.Text.Replace("'", "").Replace("-", "");
                            bool containsDigitsOrSymbols = Regex.IsMatch(checkingMessage, @"[\d\p{P}]");
                            if (containsDigitsOrSymbols)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ввели некоректні дані. Спробуйте ще раз");
                                TrueOrFalse = false;
                            }
                            else
                            {
                                await searchCityClient.SearchSomeCityAsync(userParameters.coor2, message.Chat.Id);
                                List<CityMain> lcm = searchCityClient.GetStatistCityAsync(message.Chat.Id).Result;

                                foreach (CityMain i in lcm)
                                {
                                    userParameters.coor2 = i.Coordinates;
                                }
                            }
                        }
                        if (TrueOrFalse)
                        {


                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                            (
                            new[]
                                {
                          new KeyboardButton[] { "Січень", "Лютий", "Березень", "Квітень" },
                          new KeyboardButton[] { "Травень", "Червень", "Липень", "Серпень" },
                          new KeyboardButton[] { "Вересень", "Жовтень", "Листопад", "Грудень" },
                                }
                            )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Чудово! Потрібно визначитись із початковою та кінцевою датою поїздок. Отож, почнемо з початкової дати. Будь ласка, оберіть потрібний Вам місяць:", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "EnterDayOnFirstDateOnTrip";
                        }
                    }
                    else if (userParameters.operation == "EnterDayOnFirstDateOnTrip")
                    {
                        string[] months = { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" };
                        if (months.Contains(message.Text))
                        {
                            userParameters.monthOnFirstDateOnTrip = message.Text;
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                               (
                               new[]
                                   {
                              new KeyboardButton[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" },
                              new KeyboardButton[] { "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                              new KeyboardButton[] { "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" },
                                   }
                               )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"І на яке число?", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "EnterMonthOnLastDateOnTrip";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний місяць. Спробуйте ще раз");
                        }

                    }

                    else if (userParameters.operation == "EnterMonthOnLastDateOnTrip")
                    {
                        string[] days = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
                                      "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                      "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };

                        if (days.Contains(message.Text))
                        {
                            userParameters.dayOnFirstDateOnTrip = message.Text;
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                               (
                               new[]
                                   {
                              new KeyboardButton[] { "Січень", "Лютий", "Березень", "Квітень" },
                              new KeyboardButton[] { "Травень", "Червень", "Липень", "Серпень" },
                              new KeyboardButton[] { "Вересень", "Жовтень", "Листопад", "Грудень" },
                                   }
                               )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Залишилось обрати кінцеву. Будь ласка, оберіть потрібний Вам місяць:", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "EnterDayOnLastOnTrip";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильне число місяця. Спробуйте ще раз");
                        }
                    }
                    else if (userParameters.operation == "EnterDayOnLastOnTrip")
                    {
                        string[] months = { "Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень" };
                        if (months.Contains(message.Text))
                        {
                            userParameters.monthOnLastDateOnTrip = message.Text;
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                               (
                               new[]
                                   {
                              new KeyboardButton[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" },
                              new KeyboardButton[] { "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                              new KeyboardButton[] { "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" },
                                   }
                               )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"І на яке число?", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "ShowRequest";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний місяць. Спробуйте ще раз");
                        }

                    }


                    else if (userParameters.operation == "ShowRequest")
                    {
                        string[] days = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
                                      "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                      "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };

                        if (days.Contains(message.Text))
                        {
                            userParameters.dayOnLastDateOnTrip = message.Text;

                            if (userParameters.monthOnFirstDateOnTrip == "Січень") { userParameters.numberOfFirstMonth = 1; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Лютий") { userParameters.numberOfFirstMonth = 2; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Березень") { userParameters.numberOfFirstMonth = 3; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Квітень") { userParameters.numberOfFirstMonth = 4; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Травень") { userParameters.numberOfFirstMonth = 5; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Червень") { userParameters.numberOfFirstMonth = 6; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Липень") { userParameters.numberOfFirstMonth = 7; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Серпень") { userParameters.numberOfFirstMonth = 8; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Вересень") { userParameters.numberOfFirstMonth = 9; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Жовтень") { userParameters.numberOfFirstMonth = 10; }
                            else if (userParameters.monthOnFirstDateOnTrip == "Листопад") { userParameters.numberOfFirstMonth = 11; }
                            else { userParameters.numberOfFirstMonth = 12; }

                            if (userParameters.numberOfFirstMonth.ToString().Length == 1)
                            {
                                userParameters.DateTimeOnFirstDateOnTrip = $"0{userParameters.numberOfFirstMonth}-{userParameters.dayOnFirstDateOnTrip}";
                            }
                            else
                            {
                                userParameters.DateTimeOnFirstDateOnTrip = $"{userParameters.numberOfFirstMonth}-{userParameters.dayOnFirstDateOnTrip}";
                            }

                            DateTime startDate = new DateTime(DateTime.Now.Year, userParameters.numberOfFirstMonth, int.Parse(userParameters.dayOnFirstDateOnTrip));


                            if (userParameters.monthOnLastDateOnTrip == "Січень") { userParameters.numberOfLastMonth = 1; }
                            else if (userParameters.monthOnLastDateOnTrip == "Лютий") { userParameters.numberOfLastMonth = 2; }
                            else if (userParameters.monthOnLastDateOnTrip == "Березень") { userParameters.numberOfLastMonth = 3; }
                            else if (userParameters.monthOnLastDateOnTrip == "Квітень") { userParameters.numberOfLastMonth = 4; }
                            else if (userParameters.monthOnLastDateOnTrip == "Травень") { userParameters.numberOfLastMonth = 5; }
                            else if (userParameters.monthOnLastDateOnTrip == "Червень") { userParameters.numberOfLastMonth = 6; }
                            else if (userParameters.monthOnLastDateOnTrip == "Липень") { userParameters.numberOfLastMonth = 7; }
                            else if (userParameters.monthOnLastDateOnTrip == "Серпень") { userParameters.numberOfLastMonth = 8; }
                            else if (userParameters.monthOnLastDateOnTrip == "Вересень") { userParameters.numberOfLastMonth = 9; }
                            else if (userParameters.monthOnLastDateOnTrip == "Жовтень") { userParameters.numberOfLastMonth = 10; }
                            else if (userParameters.monthOnLastDateOnTrip == "Листопад") { userParameters.numberOfLastMonth = 11; }
                            else { userParameters.numberOfFirstMonth = 12; }

                            if (userParameters.numberOfLastMonth.ToString().Length == 1)
                            {
                                userParameters.DateTimeOnLastDateOnTrip = $"0{userParameters.numberOfLastMonth}-{userParameters.dayOnLastDateOnTrip}";
                            }
                            else
                            {
                                userParameters.DateTimeOnLastDateOnTrip = $"{userParameters.numberOfLastMonth}-{userParameters.dayOnLastDateOnTrip}";
                            }

                            DateTime dateTimeFirst = DateTime.ParseExact(userParameters.DateTimeOnFirstDateOnTrip, "MM-dd", null);
                            DateTime dateTimeLast = DateTime.ParseExact(userParameters.DateTimeOnLastDateOnTrip, "MM-dd", null);

                            string DateTimeOnFirstDateOnTrip = dateTimeFirst.ToString("d MMMM", new System.Globalization.CultureInfo("uk-UA"));
                            string DateTimeOnLastDateOnTrip = dateTimeLast.ToString("d MMMM", new System.Globalization.CultureInfo("uk-UA"));



                            DateTime endDate = new DateTime(DateTime.Now.Year, userParameters.numberOfLastMonth, int.Parse(userParameters.dayOnLastDateOnTrip));

                            if (startDate < DateTime.Now || endDate < DateTime.Now || startDate > endDate)
                            {
                                if (startDate > endDate)
                                {
                                    ReplyKeyboardMarkup replyKeyboardMarkup2 = new
                                                (
                                                new[]
                                                    {
                                             new KeyboardButton[] { "Змінити дату поїздки" },
                                                       }
                                                       )
                                    {
                                        ResizeKeyboard = true

                                    };
                                    await botClient.SendTextMessageAsync(message.Chat.Id, $"На жаль, Ви ввели не правильні дати. Вкажіть початкову та кінцеву дати правильно", replyMarkup: replyKeyboardMarkup2);
                                    userParameters.operation = "EnterMonthOnFirstDateOnTrip";
                                }
                                else
                                {
                                    ReplyKeyboardMarkup replyKeyboardMarkup2 = new
                                                (
                                                new[]
                                                    {
                                             new KeyboardButton[] { "Змінити дату поїздки" },
                                                       }
                                                       )
                                    {
                                        ResizeKeyboard = true

                                    };
                                    await botClient.SendTextMessageAsync(message.Chat.Id, $"На жаль, Ви ввели не правильну дату. Поїздки за минулі дати шукати не можна, спробуйте інші дати", replyMarkup: replyKeyboardMarkup2);
                                    userParameters.operation = "EnterMonthOnFirstDateOnTrip";
                                }
                            }

                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Шукаємо поїздки з міста {userParameters.StartCity} у місто {userParameters.FinishCity}\nПерша дата: {userParameters.DateTimeOnFirstDateOnTrip}\nОстання дата: {userParameters.DateTimeOnLastDateOnTrip}");

                                int totalDays = (endDate - startDate).Days;

                                DateTime[] dates = new DateTime[totalDays + 1];

                                for (int i = 0; i <= totalDays; i++)
                                {
                                    dates[i] = startDate.AddDays(i);
                                }

                                foreach (DateTime date in dates)
                                {
                                    string result = null;
                                    string resultFirstTen = null;
                                    string resultSecondTen = null;
                                    string resultThirdTen = null;
                                    string resultFourthTen = null;
                                    string resultFifthTen = null;
                                    string resultSixthTen = null;
                                    string resultSeventhTen = null;
                                    string resultEighthTen = null;
                                    string resultNinethTen = null;
                                    string resultTenthTen = null;
                                    await searchTripClient.SearchSomeTripAsync(userParameters.coor1, userParameters.coor2, date.ToString("MM-dd"), message.Chat.Id);
                                    List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;

                                    bool showRequest = true;
                                    bool showRequest2 = true;

                                    foreach (TripMain i in ltm)
                                    {
                                        if (i.InfoAboutTrip == "0 поїздок")
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"{i.StatusOfRequest}");
                                            showRequest2 = false;
                                        }
                                        else
                                        {
                                            if (showRequest)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, $"{i.StatusOfRequest}");
                                                showRequest = false;
                                            }
                                            result += $"{i.InfoAboutTrip}Ціна: {i.Price} грн\n\n\n";
                                            if (result.Contains("Поїздка №9")) { resultFirstTen = result; result = ""; }
                                            if (result.Contains("Поїздка №19")) { resultSecondTen = result; result = ""; }
                                            if (result.Contains("Поїздка №29")) { resultThirdTen = result; result = ""; }
                                            if (result.Contains("Поїздка №39")) { resultFourthTen = result; result = ""; }
                                            if (result.Contains("Поїздка №49")) { resultFifthTen = result; result = ""; }
                                            if (result.Contains("Поїздка №59")) { resultSixthTen = result; result = ""; }
                                            if (result.Contains("Поїздка №69")) { resultSeventhTen = result; result = ""; }
                                            if (result.Contains("Поїздка №79")) { resultEighthTen = result; result = ""; }
                                            if (result.Contains("Поїздка №89")) { resultNinethTen = result; result = ""; }
                                            if (result.Contains("Поїздка №99")) { resultTenthTen = result; result = ""; }
                                        }

                                    }
                                    if (showRequest2)
                                    {
                                        if (resultFirstTen != null) await botClient.SendTextMessageAsync(message.Chat.Id, resultFirstTen);
                                        if (resultSecondTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSecondTen); }
                                        if (resultThirdTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultThirdTen); }
                                        if (resultFourthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFourthTen); }
                                        if (resultFifthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFifthTen); }
                                        if (resultSixthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSixthTen); }
                                        if (resultSeventhTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSeventhTen); }
                                        if (resultEighthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultEighthTen); }
                                        if (resultNinethTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultNinethTen); }
                                        if (resultTenthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultTenthTen); }
                                        if (result != "") { await botClient.SendTextMessageAsync(message.Chat.Id, result); }
                                    }
                                }

                                ReplyKeyboardMarkup replyKeyboardMarkup = new
                                          (
                                          new[]
                                              {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                              }
                                          )
                                {
                                    ResizeKeyboard = true
                                };

                                await botClient.SendTextMessageAsync(message.Chat.Id, "Ось результати пошуку :)\n"
                                    + "Для продовження натисніть на відповідну кнопку в меню.", replyMarkup: replyKeyboardMarkup);
                                userParameters.operation = "SomeOperations";

                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильне число місяця. Спробуйте ще раз");
                        }

                    }
                    else if (userParameters.operation == "SomeOperations")
                    {
                        if (message.Text == "Відсортувати поїздки по цінам")
                        {
                            string result = null;

                            List<string> listOfSortedTrips = new List<string>();

                            DateTime startDate = new DateTime(2023, userParameters.numberOfFirstMonth, int.Parse(userParameters.dayOnFirstDateOnTrip));

                            DateTime endDate = new DateTime(2023, userParameters.numberOfLastMonth, int.Parse(userParameters.dayOnLastDateOnTrip));

                            int totalDays = (endDate - startDate).Days;

                            DateTime[] dates = new DateTime[totalDays + 1];

                            for (int i = 0; i <= totalDays; i++)
                            {
                                dates[i] = startDate.AddDays(i);
                            }

                            foreach (DateTime date in dates)
                            {
                                result = null;

                                bool TrueOrFalse = true;
                                await searchTripClient.SearchSomeTripAsync(userParameters.coor1, userParameters.coor2, date.ToString("MM-dd"), message.Chat.Id);
                                List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                                foreach (TripMain i in ltm)
                                {
                                    if (i.InfoAboutTrip == "0 поїздок")
                                    {
                                        TrueOrFalse = false;
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, i.StatusOfRequest.Replace("Поїздки", "Найдешевші поїздки"));
                                        break;
                                    }
                                }
                                if (TrueOrFalse)
                                {
                                    List<int> price = new List<int>();
                                    List<string> infoAboutTrip = new List<string>();
                                    foreach (TripMain i in ltm)
                                    {
                                        price.Add(i.Price);
                                        infoAboutTrip.Add(i.InfoAboutTrip);
                                    }
                                    List<string> sortedTrips = infoAboutTrip.Zip(price, (trip, p) => new { Trip = trip, Price = p })
                                                                            .OrderBy(x => x.Price)
                                                                            .Select(x => x.Trip)
                                                                            .ToList();
                                    price.Sort();
                                    for (int i = 0; i < sortedTrips.Count; i++)
                                    {
                                        result += sortedTrips[i] + "Ціна: " + price[i] + " грн\n\n\n";
                                        if (i.ToString().Contains('9') || i == sortedTrips.Count - 1) { listOfSortedTrips.Add(result); result = ""; }
                                        if (listOfSortedTrips.Count == 0)
                                        {
                                            listOfSortedTrips.Add(result); result = "";
                                        }
                                    }

                                }
                                for (int i = 0; i < listOfSortedTrips.Count; i++)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, listOfSortedTrips[i]);
                                }

                            }
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                          (
                                          new[]
                                              {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                              }
                                          )
                            {
                                ResizeKeyboard = true
                            };

                            await botClient.SendTextMessageAsync(message.Chat.Id, "Поїздки відсортовані по цінам (спочатку найдешевші на кожний день)! Для продовження, натисність на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else if (message.Text == "Зберегти поїздку")
                        {
                            var buttons = new List<KeyboardButton>();
                            DateTime startDate = DateTime.ParseExact(userParameters.DateTimeOnFirstDateOnTrip, "MM-dd", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(userParameters.DateTimeOnLastDateOnTrip, "MM-dd", CultureInfo.InvariantCulture);

                            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                            {
                                string buttonText = date.ToString("d MMMM", new CultureInfo("uk-UA"));
                                buttons.Add(new KeyboardButton(buttonText));
                            }

                            var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons.Select(b => new[] { b }));
                            replyKeyboardMarkup.ResizeKeyboard = true;

                            await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть дату:", replyMarkup: replyKeyboardMarkup);


                            userParameters.operation = "ChooseNumberOfTrip";
                        }
                        else if (message.Text == "Переглянути збережені поїздки")
                        {
                            bool TrueOrFalse = true;
                            List<FavoriteTripMain> lftm = searchTripClient.GetStatistFavoriteTripAsync(message.Chat.Id).Result;
                            foreach (FavoriteTripMain i in lftm)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, i.Trip);
                                TrueOrFalse = false;
                            }
                            if (TrueOrFalse)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Наразі список збережених поїздок порожній");
                            }
                            userParameters.operation = "SomeOperations";
                        }
                        else if (message.Text == "Видалити збережені поїздки")
                        {
                            await searchTripClient.DeleteListOfFavoriteTripsAsync(message.Chat.Id);
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                        (
                                        new[]
                                            {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                            }
                                        )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Список збережених поїздок тепер порожній. Для продовження, натисність на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели некоректні дані. Спробуйте ще раз, використовуючи кнопки в меню");
                        }
                    }
                    else if (userParameters.operation == "ChooseNumberOfTrip")
                    {
                        userParameters.dataForSavingTrip = message.Text;
                        string data = userParameters.dataForSavingTrip;
                        DateTime dt;
                        string newData = null;
                        if (DateTime.TryParse(data, new CultureInfo("uk-UA"), DateTimeStyles.None, out dt))
                        {
                            newData = dt.ToString("MM-dd");
                        }



                        string result = null;
                        string resultFirstTen = null;
                        string resultSecondTen = null;
                        string resultThirdTen = null;
                        string resultFourthTen = null;
                        string resultFifthTen = null;
                        string resultSixthTen = null;
                        string resultSeventhTen = null;
                        string resultEighthTen = null;
                        string resultNinethTen = null;
                        string resultTenthTen = null;
                        await searchTripClient.SearchSomeTripAsync(userParameters.coor1, userParameters.coor2, newData, message.Chat.Id);
                        List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;

                        bool showRequest = true;
                        bool showRequest2 = true;

                        foreach (TripMain i in ltm)
                        {
                            if (i.InfoAboutTrip == "0 поїздок")
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"{i.StatusOfRequest}");
                                showRequest2 = false;
                            }
                            else
                            {
                                if (showRequest)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, $"{i.StatusOfRequest}");
                                    showRequest = false;
                                }
                                result += $"{i.InfoAboutTrip}Ціна: {i.Price} грн\n\n\n";
                                if (result.Contains("Поїздка №9")) { resultFirstTen = result; result = ""; }
                                if (result.Contains("Поїздка №19")) { resultSecondTen = result; result = ""; }
                                if (result.Contains("Поїздка №29")) { resultThirdTen = result; result = ""; }
                                if (result.Contains("Поїздка №39")) { resultFourthTen = result; result = ""; }
                                if (result.Contains("Поїздка №49")) { resultFifthTen = result; result = ""; }
                                if (result.Contains("Поїздка №59")) { resultSixthTen = result; result = ""; }
                                if (result.Contains("Поїздка №69")) { resultSeventhTen = result; result = ""; }
                                if (result.Contains("Поїздка №79")) { resultEighthTen = result; result = ""; }
                                if (result.Contains("Поїздка №89")) { resultNinethTen = result; result = ""; }
                                if (result.Contains("Поїздка №99")) { resultTenthTen = result; result = ""; }
                            }

                        }
                        if (showRequest2)
                        {
                            if (resultFirstTen != null) await botClient.SendTextMessageAsync(message.Chat.Id, resultFirstTen);
                            if (resultSecondTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSecondTen); }
                            if (resultThirdTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultThirdTen); }
                            if (resultFourthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFourthTen); }
                            if (resultFifthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultFifthTen); }
                            if (resultSixthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSixthTen); }
                            if (resultSeventhTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultSeventhTen); }
                            if (resultEighthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultEighthTen); }
                            if (resultNinethTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultNinethTen); }
                            if (resultTenthTen != null) { await botClient.SendTextMessageAsync(message.Chat.Id, resultTenthTen); }
                            if (result != "") { await botClient.SendTextMessageAsync(message.Chat.Id, result); }
                        }
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Вкажіть номер поїздки");
                        userParameters.operation = "SaveNumberOfTrip";
                    }
                    else if (userParameters.operation == "SaveNumberOfTrip")
                    {
                        string data = userParameters.dataForSavingTrip;
                        DateTime dt;
                        string newData = null;
                        if (DateTime.TryParse(data, new CultureInfo("uk-UA"), DateTimeStyles.None, out dt))
                        {
                            newData = dt.ToString("MM-dd");
                        }


                        string[] days = new string[100];

                        for (int i = 1; i < days.Length; i++)
                        {
                            days[i] = i.ToString();
                        }

                        bool TrueOfFalse = true;

                        await searchTripClient.SearchSomeTripAsync(userParameters.coor1, userParameters.coor2, newData, message.Chat.Id);
                        List<TripMain> ltmforchecking = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                        foreach (TripMain i in ltmforchecking)
                        {
                            if (i.InfoAboutTrip.Contains($"Поїздка №{message.Text}"))
                            {
                                TrueOfFalse = true;
                                break;
                            }
                            else { TrueOfFalse = false; }
                        }


                        if (days.Contains(message.Text.Split(" ")[0]) && TrueOfFalse)
                        {

                            userParameters.numberOfTrip.Add(message.Text);
                            List<TripMain> ltm = searchTripClient.GetStatistTripAsync(message.Chat.Id).Result;
                            foreach (TripMain i in ltm)
                            {
                                if (i.InfoAboutTrip.Contains($"Поїздка №{message.Text}"))
                                {
                                    userParameters.favotireTrip = i.InfoAboutTrip + $"Ціна: {i.Price} грн\nПоїздка з міста {userParameters.StartCity} у місто {userParameters.FinishCity} на {message.Text}";
                                    break;
                                }
                            }

                            await searchTripClient.SaveFavoriteTripAsync(userParameters.favotireTrip, message.Chat.Id);
                            ReplyKeyboardMarkup replyKeyboardMarkup = new
                                         (
                                         new[]
                                             {
                                          new KeyboardButton[] { "Розпочати пошук спочатку", "Відсортувати поїздки по цінам" },
                                          new KeyboardButton[] { "Зберегти поїздку", "Переглянути збережені поїздки" },
                                          new KeyboardButton[] { "Видалити збережені поїздки" },
                                             }
                                         )
                            {
                                ResizeKeyboard = true
                            };
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Поїздку №{message.Text} збережено. Для продовження - натисніть на відповідну кнопку в меню", replyMarkup: replyKeyboardMarkup);
                            userParameters.operation = "SomeOperations";
                        }
                        else
                        {
                            if (!TrueOfFalse)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний номер поїздки. Можливо такої поїздки не існує. Спробуйте ще раз");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели неправильний номер поїздки. Просто вкажіть цифру. Наприклад: '4'");
                            }
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели некоректні дані. Спробуйте ще раз, використовуючи кнопки в меню");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Будь ласка, використовуйте кнопки в меню");
                }

            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Бот не приймає некоректні повідомлення (фото, відео, стікери та інше)\nЛише текстові повідомлення!");
            }
        }

    }
}

