﻿using KurrentDB.Client;
using Microsoft.AspNetCore.Mvc;

namespace setting_up_dependency_injection.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class EventStoreController : ControllerBase {
		#region using-dependency
		private readonly KurrentDBClient _KurrentClient;

		public EventStoreController(KurrentDBClient KurrentDBClient) {
			_KurrentClient = KurrentDBClient;
		}

		[HttpGet]
		public IAsyncEnumerable<ResolvedEvent> Get() {
			return _KurrentClient.ReadAllAsync();
		}
		#endregion using-dependency
	}
}
