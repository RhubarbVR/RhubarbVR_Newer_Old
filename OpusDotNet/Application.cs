namespace OpusDotNet
{
	/// <summary>
	/// Specifies the intended applications.
	/// </summary>
	public enum Application
	{
		/// <summary>
		/// Process signal for improved speech intelligibility.
		/// </summary>
		VoIP = 2048,
		/// <summary>
		/// Favor faithfulness to the original input.
		/// </summary>
		Audio = 2049,
		/// <summary>
		/// Configure the minimum possible coding delay by disabling certain modes of operation.
		/// </summary>
		RestrictedLowDelay = 2051
	}
}
