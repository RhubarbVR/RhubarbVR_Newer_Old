using System;
using System.Linq;

using NAudio.Wave;

using OpusDotNet;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public sealed partial class OpusStream : SyncStream, INetworkedObject, IAssetProvider<IWaveProvider>
	{
		[OnChanged(nameof(UpateOpusCodec))]
		[Default(Application.VoIP)]
		public readonly Sync<Application> TargetApplication;
		[Default(64000)]
		public readonly Sync<int> Bitrate;
		[Default(1)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<int> Channels;
		[Default(Bandwidth.FullBand)]
		public readonly Sync<Bandwidth> MaxBandwidth;
		[Default(true)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<bool> FEC;
		[Default(1)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<int> ExpectedPacketLoss;
		[Default(true)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<bool> VariableBitrate;
		[Default(true)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<bool> DTX;
		public enum FrameSize
		{
			time_2_5ms,
			time_5ms,
			time_10ms,
			time_20ms,
			time_40ms,
			time_60ms
		}

		[Default(FrameSize.time_20ms)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<FrameSize> BufferSize;

		public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(48000, Channels.Value);

		public int GetSampleAmountForBufferSize { get; private set; }

		private AudioQueueWaveProvider _audioQueueWaveProvider;

		public IWaveProvider Value => _audioQueueWaveProvider;

		public bool Loaded => _audioQueueWaveProvider is not null;

		public event Action<IWaveProvider> OnAssetLoaded;

		public OpusEncoder opusEncoder;

		public OpusDecoder opusDecoder;
		private byte[] _audioBuffer = Array.Empty<byte>();

		public override void Received(Peer sender, IDataNode data) {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (opusDecoder is null) {
				return;
			}
			var opusPacked = ((DataNode<byte[]>)data).Value;
			var pcmDataLength = opusDecoder.DecodeFloat(opusPacked, opusPacked.Length, _audioBuffer, _audioBuffer.Length / sizeof(float));
			var amoutUsed = new byte[pcmDataLength * sizeof(float)];
			for (var i = 0; i < (pcmDataLength * sizeof(float)); i++) {
				amoutUsed[i] = _audioBuffer[i];
			}
			_audioQueueWaveProvider.Enqueue(amoutUsed);
		}

		private byte[] _opusBuffer = Array.Empty<byte>();

		public void UpateOpusCodec() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (opusEncoder is not null) {
				opusEncoder.Dispose();
				opusEncoder = null;
			}
			if (opusDecoder is not null) {
				opusDecoder.Dispose();
				opusDecoder = null;
			}
			_audioQueueWaveProvider = null;
			_audioQueueWaveProvider = new(WaveFormat);
			OnAssetLoaded?.Invoke(_audioQueueWaveProvider);
			var sampilesASecond = 48000 * Channels.Value;
			GetSampleAmountForBufferSize = BufferSize.Value switch {
				FrameSize.time_2_5ms => (int)(sampilesASecond * 0.0025f),
				FrameSize.time_5ms => (int)(sampilesASecond * 0.005f),
				FrameSize.time_10ms => (int)(sampilesASecond * 0.01f),
				FrameSize.time_20ms => (int)(sampilesASecond * 0.02f),
				FrameSize.time_40ms => (int)(sampilesASecond * 0.04f),
				_ => (int)(sampilesASecond * 0.06f),
			};
			if (LocalUser == Owner) {
				opusEncoder = new OpusEncoder(TargetApplication, 48000, Channels) {
					MaxBandwidth = MaxBandwidth.Value,
					FEC = FEC,
					ExpectedPacketLoss = ExpectedPacketLoss,
					VBR = VariableBitrate,
					DTX = DTX
				};
				var opusLength = opusEncoder.GetMaxDataBytes(GetSampleAmountForBufferSize, Bitrate.Value);
				_opusBuffer = new byte[opusLength];
			}
			else {
				_audioBuffer = new byte[GetSampleAmountForBufferSize * sizeof(float)];
				opusDecoder = new OpusDecoder(48000, Channels);
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			UpateOpusCodec();
			OnAssetLoaded?.Invoke(Value);
		}

		public int BufferMilliseconds => BufferSize.Value switch { FrameSize.time_2_5ms => 2, FrameSize.time_5ms => 5, FrameSize.time_10ms => 10, FrameSize.time_20ms => 20, FrameSize.time_40ms => 40, _ => 60, } / 2;

		private string _selectedAudioDevices = null;

		public void LoadMainInput() {
			LoadAudioInput("2- High Definition Audio Device"); // todo get audio input from settings
		}
		public void LoadAudioInput(string selected = "default") {
			_selectedAudioDevices = selected;
			LoadAudioInput(RAudio.GetWaveIn(WaveFormat, BufferMilliseconds, _selectedAudioDevices));
		}

		private IWaveIn _waveIn;

		private void LoadAudioInput(IWaveIn waveIn) {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (_waveIn is not null) {
				_waveIn.StopRecording();
				_waveIn.DataAvailable -= WaveIn_DataAvailable;
				_waveIn?.Dispose();
				_waveIn = null;
			}
			_waveIn = waveIn;
			if (_waveIn is not null) {
				_waveIn.DataAvailable += WaveIn_DataAvailable;
				_waveIn.StartRecording();
			}
		}

		private unsafe void WaveIn_DataAvailable(object sender, WaveInEventArgs e) {
			if (opusEncoder is null) {
				return;
			}
			if (_waveIn is null) {
				RLog.Err("Wave in is null");
				return;
			}
			_audioQueueWaveProvider.Enqueue(e.Buffer);
			var amountUsed = opusEncoder.EncodeFloat(e.Buffer, e.BytesRecorded / sizeof(float), _opusBuffer, _opusBuffer.Length);
			var amoutUsed = new byte[amountUsed];
			for (var i = 0; i < amountUsed; i++) {
				amoutUsed[i] = _opusBuffer[i];
			}
			World.BroadcastDataToAllStream(this, new DataNode<byte[]>(amoutUsed), LiteNetLib.DeliveryMethod.Unreliable);
		}
	}
}
