using System;
using System.Runtime.InteropServices;
using System.Threading;

using NAudio.Wave;

namespace RhuEngine
{
	public unsafe sealed class ALSACapture : IWaveIn
	{
		private Snd_pcm_t* _pcm;
		private Snd_pcm_hw_params_t* _hwparams;
		private ulong _frames;

		public string DeviceName { get; set; } = "default";
		public int BufferMilliseconds { get; set; }
		public int NumberOfBuffers { get; set; }

		public WaveFormat WaveFormat
		{
			get; set;
		}
		public event EventHandler<WaveInEventArgs> DataAvailable;
		public event EventHandler<StoppedEventArgs> RecordingStopped;
		public ALSACapture() {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 1);
			BufferMilliseconds = 100;
			NumberOfBuffers = 2;
		}

		public void StartRecording() {
			if (WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat) {
				throw new Exception("Did not know format");
			}
			int rc;
			var rate = (uint)WaveFormat.SampleRate;
			ulong size;

			rc = snd_pcm_open(out _pcm, DeviceName, SND_PCM_STREAM_CAPTURE, 0);
			if (rc < 0) {
				throw new Exception(string.Format("unable to open pcm device: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_malloc(out _hwparams);
			if (rc < 0) {
				throw new Exception(string.Format("unable to allocate hardware parameter structure: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_any(_pcm, _hwparams);
			if (rc < 0) {
				throw new Exception(string.Format("unable to initialize hardware parameter structure: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_set_access(_pcm, _hwparams, SND_PCM_ACCESS_RW_INTERLEAVED);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set access type: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_set_format(_pcm, _hwparams, (int)SndPcmFormatT.SND_PCM_FORMAT_FLOAT);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set sample format: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_set_channels(_pcm, _hwparams, WaveFormat.Channels);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set channel count: {0}", snd_strerror(rc)));
			}

			size = (ulong)(BufferMilliseconds * rate / 1000);
			_frames = size * (ulong)NumberOfBuffers;

			rc = snd_pcm_hw_params_set_buffer_size_near(_pcm, _hwparams, ref size);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set buffer size: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_set_period_size_near(_pcm, _hwparams, ref size, out var dir);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set period size: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_set_rate_near(_pcm, _hwparams, ref rate, ref dir);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set sample rate: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params(_pcm, _hwparams);
			if (rc < 0) {
				throw new Exception(string.Format("unable to set parameters: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_hw_params_get_period_size(_hwparams, ref size, out dir);
			if (rc < 0) {
				throw new Exception(string.Format("snd_pcm_hw_params_get_period_size: {0}", snd_strerror(rc)));
			}

			var bufferSize = (int)size * WaveFormat.BitsPerSample * WaveFormat.Channels; // 2 bytes per sample
			var buffer = new byte[bufferSize];

			rc = snd_pcm_prepare(_pcm);
			if (rc < 0) {
				throw new Exception(string.Format("unable to prepare audio interface for use: {0}", snd_strerror(rc)));
			}

			rc = snd_pcm_start(_pcm);
			if (rc < 0) {
				throw new Exception(string.Format("unable to start audio capture: {0}", snd_strerror(rc)));
			}

			// start background thread to read audio data from ALSA and fire DataAvailable event
			ThreadPool.QueueUserWorkItem(state => {
				while (true) {
					int read;
					fixed (byte* p = buffer) {
						read = snd_pcm_readi(_pcm, p, (uint)size);
					}
					if (read < 0) {
						throw new Exception(string.Format("error reading from audio interface: {0}", snd_strerror(read)));
					}

					if (DataAvailable != null) {
						var data = new byte[read * WaveFormat.BitsPerSample * WaveFormat.Channels];
						Array.Copy(buffer, data, read * WaveFormat.BitsPerSample * WaveFormat.Channels);
						DataAvailable(this, new WaveInEventArgs(data, data.Length));
					}
				}
			});
		}

		public void StopRecording() {
			if (_pcm != null) {
				var rc = snd_pcm_drain(_pcm);
				if (rc < 0) {
					throw new Exception(string.Format("error stopping audio capture: {0}", snd_strerror(rc)));
				}

				rc = snd_pcm_close(_pcm);
				if (rc < 0) {
					throw new Exception(string.Format("error closing audio interface: {0}", snd_strerror(rc)));
				}

				_pcm = null;
			}

			RecordingStopped?.Invoke(this, new StoppedEventArgs());
		}

		public void Dispose() {
			StopRecording();
		}

		// ALSA constants and structs
		private const int SND_PCM_ACCESS_RW_INTERLEAVED = 3;

		public enum SndPcmFormatT : int
		{
			SND_PCM_FORMAT_UNKNOWN = -1, SND_PCM_FORMAT_S8 = 0, SND_PCM_FORMAT_U8, SND_PCM_FORMAT_S16_LE,
			SND_PCM_FORMAT_S16_BE, SND_PCM_FORMAT_U16_LE, SND_PCM_FORMAT_U16_BE, SND_PCM_FORMAT_S24_LE,
			SND_PCM_FORMAT_S24_BE, SND_PCM_FORMAT_U24_LE, SND_PCM_FORMAT_U24_BE, SND_PCM_FORMAT_S32_LE,
			SND_PCM_FORMAT_S32_BE, SND_PCM_FORMAT_U32_LE, SND_PCM_FORMAT_U32_BE, SND_PCM_FORMAT_FLOAT_LE,
			SND_PCM_FORMAT_FLOAT_BE, SND_PCM_FORMAT_FLOAT64_LE, SND_PCM_FORMAT_FLOAT64_BE, SND_PCM_FORMAT_IEC958_SUBFRAME_LE,
			SND_PCM_FORMAT_IEC958_SUBFRAME_BE, SND_PCM_FORMAT_MU_LAW, SND_PCM_FORMAT_A_LAW, SND_PCM_FORMAT_IMA_ADPCM,
			SND_PCM_FORMAT_MPEG, SND_PCM_FORMAT_GSM, SND_PCM_FORMAT_S20_LE, SND_PCM_FORMAT_S20_BE,
			SND_PCM_FORMAT_U20_LE, SND_PCM_FORMAT_U20_BE, SND_PCM_FORMAT_SPECIAL = 31, SND_PCM_FORMAT_S24_3LE = 32,
			SND_PCM_FORMAT_S24_3BE, SND_PCM_FORMAT_U24_3LE, SND_PCM_FORMAT_U24_3BE, SND_PCM_FORMAT_S20_3LE,
			SND_PCM_FORMAT_S20_3BE, SND_PCM_FORMAT_U20_3LE, SND_PCM_FORMAT_U20_3BE, SND_PCM_FORMAT_S18_3LE,
			SND_PCM_FORMAT_S18_3BE, SND_PCM_FORMAT_U18_3LE, SND_PCM_FORMAT_U18_3BE, SND_PCM_FORMAT_G723_24,
			SND_PCM_FORMAT_G723_24_1B, SND_PCM_FORMAT_G723_40, SND_PCM_FORMAT_G723_40_1B, SND_PCM_FORMAT_DSD_U8,
			SND_PCM_FORMAT_DSD_U16_LE, SND_PCM_FORMAT_DSD_U32_LE, SND_PCM_FORMAT_DSD_U16_BE, SND_PCM_FORMAT_DSD_U32_BE,
			SND_PCM_FORMAT_LAST = SND_PCM_FORMAT_DSD_U32_BE, SND_PCM_FORMAT_S16 = SND_PCM_FORMAT_S16_LE, SND_PCM_FORMAT_U16 = SND_PCM_FORMAT_U16_LE, SND_PCM_FORMAT_S24 = SND_PCM_FORMAT_S24_LE,
			SND_PCM_FORMAT_U24 = SND_PCM_FORMAT_U24_LE, SND_PCM_FORMAT_S32 = SND_PCM_FORMAT_S32_LE, SND_PCM_FORMAT_U32 = SND_PCM_FORMAT_U32_LE, SND_PCM_FORMAT_FLOAT = SND_PCM_FORMAT_FLOAT_LE,
			SND_PCM_FORMAT_FLOAT64 = SND_PCM_FORMAT_FLOAT64_LE, SND_PCM_FORMAT_IEC958_SUBFRAME = SND_PCM_FORMAT_IEC958_SUBFRAME_LE, SND_PCM_FORMAT_S20 = SND_PCM_FORMAT_S20_LE, SND_PCM_FORMAT_U20 = SND_PCM_FORMAT_U20_LE
		}

		private const int SND_PCM_STREAM_CAPTURE = 1;

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_rate_near(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* prams, ref uint val, ref int dir);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_get_period_size(Snd_pcm_hw_params_t* prams, ref ulong stream, out int dir);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_open(out Snd_pcm_t* pcm, string name, int stream, int mode);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_malloc(out Snd_pcm_hw_params_t* ptr);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_any(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_access(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams, int access);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_format(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams, int format);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_channels(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams, int channels);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_buffer_size_near(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams, ref ulong size);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params_set_period_size_near(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams, ref ulong size, out int dir);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_hw_params(Snd_pcm_t* pcm, Snd_pcm_hw_params_t* hwparams);

		[DllImport("libasound.so")]
		private static extern void snd_pcm_hw_params_free(Snd_pcm_hw_params_t* ptr);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_prepare(Snd_pcm_t* pcm);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_start(Snd_pcm_t* pcm);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_readi(Snd_pcm_t* pcm, byte* buffer, uint size);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_drain(Snd_pcm_t* pcm);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_close(Snd_pcm_t* pcm);

		[DllImport("libasound.so")]
		private static extern string snd_strerror(int errnum);

		private struct Snd_pcm_t { }

		private struct Snd_pcm_hw_params_t { }

	}
}

