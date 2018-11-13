namespace DemoService.Core
{
	/// <summary>Configuration keys</summary>
	public static class ConfigurationKeys
	{
		/// <summary>SQL connection string</summary>
		public const string SQLConnection = nameof(SQLConnection);

		/// <summary>JWT secret string</summary>
		public const string JwtSecret = nameof(JwtSecret);

		/// <summary>JWT default expire minutes</summary>
		public const string JwtDefaultExpireMinutes = nameof(JwtDefaultExpireMinutes);
	}
}