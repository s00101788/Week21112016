using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Reflection;
using System.IO;
using CsvHelper;
using System.Text;
using GameData;
using System.Timers;

namespace Week21112016
{
    
    public class GameHub : Hub
    {
        public GameHub() : base()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Week21112016.randomNameswithscores.csv";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    CsvReader csvReader = new CsvReader(reader);
                    csvReader.Configuration.HasHeaderRecord = false;
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    Players  = csvReader.GetRecords<PlayerData>().ToList();
                }
            }

        }
        public static List<PlayerData> Players = new List<PlayerData>();
        public static int WorldX = 2000;
        public static int WorldY = 2000;
        public static TimeSpan countDown = new TimeSpan(0,0,0,10);
        public static Timer TimeToStart = new Timer(1000);
        public void Hello()
        {
            Clients.All.hello();
        }

        public void join()
        {
            Clients.Caller.joined(WorldX,WorldY);
        }

        public void getPlayer(string FirstName, string SecondName)
        {
            var player = Players.FirstOrDefault(
                            p => p.FirstName == FirstName &&
                            p.SecondName == SecondName);
            if (player != null)
            {
                Clients.Caller.recievePlayer(player);
                TimeToStart.Elapsed += TimeToStart_Elapsed;
                Clients.All.recieveCountDown(countDown.TotalSeconds);
                TimeToStart.Start();
              
            }
            else
                Clients.Caller.error(" Player does not exist "
                                           + FirstName + " " + SecondName);
        }

        private void TimeToStart_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (countDown.TotalSeconds > 0)
            {
                countDown = countDown.Subtract(new TimeSpan(0, 0, 0, 1));
                Clients.All.recieveCountDown(countDown.TotalSeconds);
            }
            else
            {
                TimeToStart.Stop();
                Clients.All.Start();
            }
            
        }
    }
}