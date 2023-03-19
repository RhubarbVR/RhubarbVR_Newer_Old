using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace OpusDotNet
{
	internal sealed class SafeEncoderHandle : SafeHandle
	{
		private SafeEncoderHandle() : base(IntPtr.Zero, true) {
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected override bool ReleaseHandle() {
			API.opus_encoder_destroy(handle);
			return true;
		}
	}
}
