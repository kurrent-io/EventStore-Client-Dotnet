namespace KurrentDB.Client {
	/// <summary>
	/// The exception that is thrown when an invalid setting is found in an KurrentDB connection string.
	/// </summary>
	public class InvalidSettingException : ConnectionStringParseException {
		/// <summary>
		/// Constructs a new <see cref="InvalidSettingException"/>.
		/// </summary>
		/// <param name="message"></param>
		public InvalidSettingException(string message) : base(message) { }
	}
}
