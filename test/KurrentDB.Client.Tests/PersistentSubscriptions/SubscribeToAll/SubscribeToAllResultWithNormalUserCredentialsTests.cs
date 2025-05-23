using KurrentDB.Client.Tests.TestNode;
using KurrentDB.Client.Tests;

namespace KurrentDB.Client.Tests.PersistentSubscriptions;

[Trait("Category", "Target:PersistentSubscriptions")]
public class SubscribeToAllResultWithNormalUserCredentialsTests(ITestOutputHelper output, KurrentDBTemporaryFixture fixture)
	: KurrentTemporaryTests<KurrentDBTemporaryFixture>(output, fixture) {
	[RetryFact]
	public async Task returns_result_with_normal_user_credentials() {
		var group  = Fixture.GetGroupName();
		var stream = Fixture.GetStreamName();

		const int streamSubscriptionCount    = 4;
		const int allStreamSubscriptionCount = 3;

		for (var i = 0; i < streamSubscriptionCount; i++)
			await Fixture.Subscriptions.CreateToStreamAsync(
				stream,
				group + i,
				new(),
				userCredentials: TestCredentials.Root
			);

		for (var i = 0; i < allStreamSubscriptionCount; i++)
			await Fixture.Subscriptions.CreateToAllAsync(
				group + i,
				new(),
				userCredentials: TestCredentials.Root
			);

		var result = await Fixture.Subscriptions.ListToAllAsync(userCredentials: TestCredentials.Root);
		Assert.Equal(allStreamSubscriptionCount, result.Count());
	}
}
