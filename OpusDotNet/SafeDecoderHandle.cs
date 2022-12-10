using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace OpusDotNet
{
	internal sealed class SafeDecoderHandle : SafeHandle
	{
		public SafeDecoderHandle() : base(IntPtr.Zero, true) {
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle() {
			API.opus_decoder_destroy(handle);
			return true;
		}
	}
}
