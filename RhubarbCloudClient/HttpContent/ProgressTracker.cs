using System;

namespace RhubarbCloudClient.HttpContent
{
	public class ProgressTracker
	{
		public ProgressState CurrentState { get; private set; }

		public event Action<ProgressState> StateChange;
		public event Action<long> AmountOfBytesChanged;

		private long _totalBytes;

		public long Bytes
		{
			get => _totalBytes;
			set {
				_totalBytes = value;
				AmountOfBytesChanged?.Invoke(_totalBytes);
			}
		}

		public void ChangeState(ProgressState progressState) {
			CurrentState = progressState;
			StateChange?.Invoke(progressState);
		}

	}
}