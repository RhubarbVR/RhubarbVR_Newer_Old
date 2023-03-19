﻿namespace OpusDotNet
{
	/// <summary>
	/// Specifies the possible errors when using Opus.
	/// </summary>
	public enum OpusError
	{
		/// <summary>
		/// One or more invalid/out of range arguments.
		/// </summary>
		BadArg = -1,
		/// <summary>
		/// Not enough bytes allocated in the buffer.
		/// </summary>
		BufferTooSmall = -2,
		/// <summary>
		/// An internal error was detected.
		/// </summary>
		InternalError = -3,
		/// <summary>
		/// The compressed data passed is corrupted.
		/// </summary>
		InvalidPacket = -4,
		/// <summary>
		/// Invalid/unsupported request number.
		/// </summary>
		Unimplemented = -5,
		/// <summary>
		/// An encoder or decoder structure is invalid or already freed.
		/// </summary>
		InvalidState = -6,
		/// <summary>
		/// Memory allocation has failed.
		/// </summary>
		AllocFail = -7,
	}
}
