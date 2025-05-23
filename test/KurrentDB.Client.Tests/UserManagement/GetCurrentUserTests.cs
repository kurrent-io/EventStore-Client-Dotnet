using KurrentDB.Client;

namespace KurrentDB.Client.Tests;

[Trait("Category", "Target:UserManagement")]
public class GetCurrentUserTests(ITestOutputHelper output, KurrentDBPermanentFixture fixture) : KurrentPermanentTests<KurrentDBPermanentFixture>(output, fixture) {
	[Fact]
	public async Task returns_the_current_user() {
		var user = await Fixture.DbUsers.GetCurrentUserAsync(TestCredentials.Root);
		user.LoginName.ShouldBe(TestCredentials.Root.Username);
	}
}
