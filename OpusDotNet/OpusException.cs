using System;

namespace OpusDotNet
{
	/// <summary>
	/// The exception that is thrown when an Opus error occurs.
	/// </summary>
	public sealed class OpusException : Exception
	{
		/// <summary>
		/// Initializes a new <see cref="OpusException"/> instance, with the specified Opus error code.
		/// </summary>
		/// <param name="errorCode">The Opus error code.</param>
		public OpusException(int errorCode) : base(GetMessage((OpusError)errorCode)) {
			Error = (OpusError)errorCode;
		}

		/// <summary>
		/// The Opus error.
		/// </summary>
		public OpusError Error { get; }

		private static string GetMessage(OpusError error) {
			return error switch {
				OpusError.BadArg => "One or more invalid/out of range arguments.",
				OpusError.BufferTooSmall => "Not enough bytes allocated in the buffer.",
				OpusError.InternalError => "An internal error was detected.",
				OpusError.InvalidPacket => "The compressed data passed is corrupted.",
				OpusError.Unimplemented => "Invalid/unsupported request number.",
				OpusError.InvalidState => "An encoder or decoder structure is invalid or already freed.",
				OpusError.AllocFail => "Memory allocation has failed.",
				_ => "An unknown error has occurred.",
			};
		}
	}
}
