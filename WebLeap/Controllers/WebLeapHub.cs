using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WebLeap.Controllers
{
	[HubName("webleap")]
	public class WebLeapHub : Hub
	{
		public void Coordinates(string coordinates)
		{
			Clients.All.coordinates(coordinates);
		}
	}
}