using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RNumerics
{

	public sealed class BlockTimer
	{
		public Stopwatch Watch;
		public string Label;
		public TimeSpan Accumulated;

		public BlockTimer(in string label, in bool bStart) {
			Label = label;
			Watch = new Stopwatch();
			if (bStart) {
				Watch.Start();
			}

			Accumulated = TimeSpan.Zero;
		}
		public void Start() {
			Watch.Start();
		}
		public void Stop() {
			Watch.Stop();
		}
		public bool Running => Watch.IsRunning;

		public void Accumulate(in bool bReset = false) {
			Watch.Stop();
			Accumulated += Watch.Elapsed;
			if (bReset) {
				Watch.Reset();
			}
		}
		public void Reset() {
			Watch.Stop();
			Watch.Reset();
			Watch.Start();
		}

		public string AccumulatedString => string.Format(TimeFormatString(Accumulated), Accumulated);
		public override string ToString() {
			var t = Watch.Elapsed;
			return string.Format(TimeFormatString(Accumulated), Watch.Elapsed);
		}

		public static string TimeFormatString(in TimeSpan span) {
			return span.Minutes > 0 ? MINUTE_FORMAT : SECOND_FORMAT;
		}
		const string MINUTE_FORMAT = "{0:mm}:{0:ss}.{0:fffffff}";
		const string SECOND_FORMAT = "{0:ss}.{0:fffffff}";
	}



	public sealed class LocalProfiler : IDisposable
	{
		readonly Dictionary<string, BlockTimer> _timers = new();
		readonly List<string> _order = new();

		public BlockTimer Start(in string label) {
			if (_timers.ContainsKey(label)) {
				_timers[label].Reset();
			}
			else {
				_timers[label] = new BlockTimer(label, true);
				_order.Add(label);
			}
			return _timers[label];
		}


		public BlockTimer StopAllAndStartNew(in string label) {
			StopAll();
			return Start(label);
		}

		public BlockTimer Get(in string label) {
			return _timers[label];
		}


		public void Stop(in string label) {
			_timers[label].Stop();
		}

		public void StopAll() {
			foreach (var t in _timers.Values) {
				if (t.Running) {
					t.Stop();
				}
			}
		}


		public void StopAndAccumulate(in string label, in bool bReset = false) {
			_timers[label].Accumulate(bReset);
		}

		public void Reset(in string label) {
			_timers[label].Reset();
		}

		public void ResetAccumulated(in string label) {
			_timers[label].Accumulated = TimeSpan.Zero;
		}

		public void ResetAllAccumulated() {
			foreach (var t in _timers.Values) {
				t.Accumulated = TimeSpan.Zero;
			}
		}

		public void DivideAllAccumulated(in int div) {
			foreach (var t in _timers.Values) {
				t.Accumulated = new TimeSpan(t.Accumulated.Ticks / div);
			}
		}


		public string Elapsed(in string label) {
			return _timers[label].ToString();
		}
		public string Accumulated(in string label) {
			var accum = _timers[label].Accumulated;
			return string.Format(BlockTimer.TimeFormatString(accum), accum);
		}

		public string AllTicks(in string prefix = "Times:") {
			var b = new StringBuilder();
			b.Append(prefix + " ");
			foreach (var label in _order) {
				b.Append(label + ": " + _timers[label].ToString() + " ");
			}
			return b.ToString();
		}

		public string AllAccumulatedTicks(in string prefix = "Times:") {
			var b = new StringBuilder();
			b.Append(prefix + " ");
			foreach (var label in _order) {
				b.Append(label + ": " + Accumulated(label) + " ");
			}
			return b.ToString();
		}



		public string AllTimes(in string prefix = "Times:", in string separator = " ") {
			var b = new StringBuilder();
			b.Append(prefix + " ");
			foreach (var label in _order) {
				var span = _timers[label].Watch.Elapsed;
				b.Append(label + ": " + string.Format(BlockTimer.TimeFormatString(span), span) + separator);
			}
			return b.ToString();
		}

		public string AllAccumulatedTimes(in string prefix = "Times:", in string separator = " ") {
			var b = new StringBuilder();
			b.Append(prefix + " ");
			foreach (var label in _order) {
				var span = _timers[label].Accumulated;
				b.Append(label + ": " + string.Format(BlockTimer.TimeFormatString(span), span) + separator);
			}
			return b.ToString();
		}



		public void Dispose() {
			foreach (var timer in _timers.Values) {
				timer.Stop();
			}

			_timers.Clear();
		}
	}




}
