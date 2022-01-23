using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace OpusDotNet
{
	internal sealed class SafeDecoderHandle : SafeHandle
	{
		private SafeDecoderHandle() : base(IntPtr.Zero, true) {
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected override bool ReleaseHandle() {
			API.opus_decoder_destroy(handle);
			return true;
		}
	}
}
