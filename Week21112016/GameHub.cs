using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Week21112016
{
    
    public class GameHub : Hub
    {
        public static int WorldX = 2000;
        public static int WorldY = 2000;
        public void Hello()
        {
            Clients.All.hello();
        }

        public void join()
        {
            Clients.Caller.joined(WorldX,WorldY);
        }
    }
}