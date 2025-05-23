using System.Text.Encodings.Web;
using System.Threading.Channels;
using Grpc.Core;
using KurrentDB.Client.Core.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KurrentDB.Client {
	/// <summary>
	/// The client used to manage persistent subscriptions in the KurrentDB.
	/// </summary>
	public sealed partial class KurrentDBPersistentSubscriptionsClient : KurrentDBClientBase {
		static BoundedChannelOptions ReadBoundedChannelOptions = new (1) {
			SingleReader = true,
			SingleWriter = true,
			AllowSynchronousContinuations = true
		};

		readonly ILogger            _log;
		readonly IMessageSerializer _messageSerializer;

		/// <summary>
		/// Constructs a new <see cref="KurrentDBPersistentSubscriptionsClient"/>.
		/// </summary>
		public KurrentDBPersistentSubscriptionsClient(KurrentDBClientSettings? settings) : base(settings,
			new Dictionary<string, Func<RpcException, Exception>> {
				[Constants.Exceptions.PersistentSubscriptionDoesNotExist] = ex => new
					PersistentSubscriptionNotFoundException(
						ex.Trailers.First(x => x.Key == Constants.Exceptions.StreamName).Value,
						ex.Trailers.FirstOrDefault(x => x.Key == Constants.Exceptions.GroupName)?.Value ?? "", ex),
				[Constants.Exceptions.MaximumSubscribersReached] = ex => new
					MaximumSubscribersReachedException(
						ex.Trailers.First(x => x.Key == Constants.Exceptions.StreamName).Value,
						ex.Trailers.First(x => x.Key == Constants.Exceptions.GroupName).Value, ex),
				[Constants.Exceptions.PersistentSubscriptionDropped] = ex => new
					PersistentSubscriptionDroppedByServerException(
						ex.Trailers.First(x => x.Key == Constants.Exceptions.StreamName).Value,
						ex.Trailers.First(x => x.Key == Constants.Exceptions.GroupName).Value, ex)
			}) {
			_log = Settings.LoggerFactory?.CreateLogger<KurrentDBPersistentSubscriptionsClient>()
			       ?? new NullLogger<KurrentDBPersistentSubscriptionsClient>();

			_messageSerializer = MessageSerializer.From(settings?.Serialization);
		}
		
		private static string UrlEncode(string s) {
			return UrlEncoder.Default.Encode(s);
		}
	}
}
