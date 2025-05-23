namespace KurrentDB.Client {
	/// <summary>
	/// The exception that is thrown when an invalid <see cref="UserCredentials"/> is specified in the KurrentDB connection string.
	/// </summary>
	public class InvalidUserCredentialsException : ConnectionStringParseException {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="userInfo"></param>
		public InvalidUserCredentialsException(string userInfo)
			: base($"Invalid user credentials: '{userInfo}'. Username & password must be delimited by a colon") { }
	}
}
