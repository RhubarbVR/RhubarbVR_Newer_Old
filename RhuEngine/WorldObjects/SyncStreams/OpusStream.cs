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
		[Default(48000)]
		[OnChanged(nameof(UpateOpusCodec))]
		public readonly Sync<int> SampleRate;
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

		public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(SampleRate.Value, Channels.Value);

		public int GetSampleAmountForBufferSize { get; private set; }

		public AudioQueueWaveProvider AudioQueueWaveProvider = new();

		public IWaveProvider Value => AudioQueueWaveProvider;

		public bool Loaded => true;

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
			AudioQueueWaveProvider.Enqueue(amoutUsed);
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
			AudioQueueWaveProvider.WaveFormat = WaveFormat;
			var sampilesASecond = SampleRate.Value * Channels.Value;
			GetSampleAmountForBufferSize = BufferSize.Value switch {
				FrameSize.time_2_5ms => (int)(sampilesASecond * 0.0025f),
				FrameSize.time_5ms => (int)(sampilesASecond * 0.005f),
				FrameSize.time_10ms => (int)(sampilesASecond * 0.01f),
				FrameSize.time_20ms => (int)(sampilesASecond * 0.02f),
				FrameSize.time_40ms => (int)(sampilesASecond * 0.04f),
				_ => (int)(sampilesASecond * 0.06f),
			};
			if (LocalUser == Owner) {
				opusEncoder = new OpusEncoder(TargetApplication, SampleRate, Channels) {
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
				opusDecoder = new OpusDecoder(SampleRate, Channels);
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

		public void LoadMainInput() {
			//Wave in
			var e = new WaveInEvent {
				WaveFormat = WaveFormat,
				BufferMilliseconds = BufferMilliseconds,
			};
			LoadAudioInput(e);

			// Todo fix bugs with godot input
			//Godot 
			//var inputDevices = RAudio.Inst.EngineAudioInputDevices();
			//foreach (var item in inputDevices) {
			//	RLog.Info("Mic input" + item);
			//	if (item.Contains("Line In")) {
			//		RAudio.Inst.CurrentAudioInputDevice = item;
			//	}
			//}
			//RAudio.Inst.EngineInputAudio.BufferSizeMilliseconds = BufferMilliseconds;
			//LoadAudioInput(RAudio.Inst.EngineInputAudio);
		}

		private IWaveIn _waveIn;

		public void LoadAudioInput(IWaveIn waveIn) {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (_waveIn is not null) {
				_waveIn.DataAvailable -= WaveIn_DataAvailable;
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
			AudioQueueWaveProvider.Enqueue(e.Buffer);
			var amountUsed = opusEncoder.EncodeFloat(e.Buffer, e.BytesRecorded / sizeof(float), _opusBuffer, _opusBuffer.Length);
			var amoutUsed = new byte[amountUsed];
			for (var i = 0; i < amountUsed; i++) {
				amoutUsed[i] = _opusBuffer[i];
			}
			World.BroadcastDataToAllStream(this, new DataNode<byte[]>(amoutUsed), LiteNetLib.DeliveryMethod.Unreliable);
		}
	}
}
