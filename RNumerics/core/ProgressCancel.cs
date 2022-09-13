using System;

namespace RNumerics
{
	/// <summary>
	/// interface that provides a cancel function
	/// </summary>
	public interface ICancelSource
	{
		bool Cancelled();
	}


	/// <summary>
	/// Just wraps a func<bool> as an ICancelSource
	/// </summary>
	public sealed class CancelFunction : ICancelSource
	{
		public Func<bool> CancelF;
		public CancelFunction(in Func<bool> cancelF) {
			CancelF = cancelF;
		}
		public bool Cancelled() { return CancelF(); }
	}


	/// <summary>
	/// This class is intended to be passed to long-running computes to 
	///  1) provide progress info back to caller (not implemented yet)
	///  2) allow caller to cancel the computation
	/// </summary>
	public sealed class ProgressCancel
	{
		public ICancelSource Source;

		bool _wasCancelled = false;  // will be set to true if CancelF() ever returns true

		public ProgressCancel(in ICancelSource source) {
			Source = source;
		}
		public ProgressCancel(in Func<bool> cancelF) {
			Source = new CancelFunction(cancelF);
		}

		/// <summary>
		/// Check if client would like to cancel
		/// </summary>
		public bool Cancelled() {
			if (_wasCancelled) {
				return true;
			}

			_wasCancelled = Source.Cancelled();
			return _wasCancelled;
		}
	}
}
