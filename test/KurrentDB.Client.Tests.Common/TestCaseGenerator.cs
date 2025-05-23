namespace KurrentDB.Client.Tests;
using System.Collections;
using Bogus;

public abstract class TestCaseGenerator<T> : ClassDataAttribute, IEnumerable<object[]> {
	protected TestCaseGenerator() : base(typeof(T)) {
		Faker = new Faker();

		// ReSharper disable once VirtualMemberCallInConstructor
		Generated.AddRange(Data());

		if (Generated.Count == 0)
			throw new InvalidOperationException($"TestDataGenerator<{typeof(T).Name}> must provide at least one test case.");
	}

	protected Faker Faker { get; }

	List<object[]> Generated { get; } = [];

	public IEnumerator<object[]> GetEnumerator() => Generated.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	protected abstract IEnumerable<object[]> Data();
}
