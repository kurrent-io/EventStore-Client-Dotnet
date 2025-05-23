namespace KurrentDB.Client {
	static class WriteResultExtensions {
		public static IWriteResult OptionallyThrowWrongExpectedVersionException(
			this IWriteResult writeResult,
			AppendToStreamOptions options
		) =>
			(options.ThrowOnAppendFailure, writeResult) switch {
				(true, WrongExpectedVersionResult wrongExpectedVersionResult)
					=> throw new WrongExpectedVersionException(
						wrongExpectedVersionResult.StreamName,
						writeResult.NextExpectedStreamState,
						wrongExpectedVersionResult.ActualStreamState
					),
				_ => writeResult
			};
	}
}
