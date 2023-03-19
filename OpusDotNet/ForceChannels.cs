namespace OpusDotNet
{
	/// <summary>
	/// Specifies the modes for forced mono/stereo.
	/// </summary>
	public enum ForceChannels
	{
		/// <summary>
		/// Not forced.
		/// </summary>
		None = -1000,
		/// <summary>
		/// Forced mono.
		/// </summary>
		Mono = 1,
		/// <summary>
		/// Forced stereo.
		/// </summary>
		Stereo = 2
	}
}
