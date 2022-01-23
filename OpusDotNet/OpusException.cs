using System;

namespace OpusDotNet
{
	/// <summary>
	/// The exception that is thrown when an Opus error occurs.
	/// </summary>
	public class OpusException : Exception
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
			switch (error) {
				case OpusError.BadArg:
					return "One or more invalid/out of range arguments.";
				case OpusError.BufferTooSmall:
					return "Not enough bytes allocated in the buffer.";
				case OpusError.InternalError:
					return "An internal error was detected.";
				case OpusError.InvalidPacket:
					return "The compressed data passed is corrupted.";
				case OpusError.Unimplemented:
					return "Invalid/unsupported request number.";
				case OpusError.InvalidState:
					return "An encoder or decoder structure is invalid or already freed.";
				case OpusError.AllocFail:
					return "Memory allocation has failed.";
				default:
					return "An unknown error has occurred.";
			}
		}
	}
}
