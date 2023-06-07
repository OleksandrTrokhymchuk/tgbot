using Microsoft.VisualBasic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KURSOVA
{
    public class Program
    {
        static void Main(string[] args)
        {

            TGBotBlaBlaCar tGBotBlaBlaCar = new TGBotBlaBlaCar();
            tGBotBlaBlaCar.Start();

            Console.ReadLine();


        }
    }
}