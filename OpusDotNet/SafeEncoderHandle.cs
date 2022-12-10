using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace OpusDotNet
{
	internal sealed class SafeEncoderHandle : SafeHandle
	{
		public SafeEncoderHandle() : base(IntPtr.Zero, true) {
		}

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle() {
			API.opus_encoder_destroy(handle);
			return true;
		}
	}
}
