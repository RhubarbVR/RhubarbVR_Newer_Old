using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;

using OpusDotNet;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public class OpusStream : SyncStream, INetworkedObject, IAssetProvider<Sound>, IGlobalStepable
	{
		[OnChanged(nameof(LoadOpus))]
		[Default(Application.VoIP)]
		public Sync<Application> typeOfStream;

		public event Action<Sound> OnAssetLoaded;

		public Sound Value { get; private set; }

		[Default(8000)]
		public Sync<int> BitRate;

		public void Load(Sound data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}
		private Sound _input;

		private Sound _output;

		private bool _loadedDevice = false;
		private OpusDecoder _decoder;
		private OpusEncoder _encoder;

		private void LoadOpus() {
			if (_encoder is not null) {
				_encoder.Dispose();
				_encoder = null;
			}
			if (_decoder is not null) {
				_decoder.Dispose();
				_decoder = null;
			}
			try {
				_encoder = new OpusEncoder(typeOfStream.Value, 48000, 1);
				_decoder = new OpusDecoder(48000, 1);
			}
			catch (Exception ex) {
				Log.Err($"Exception when loading Opus {ex}");
			}
		}

		private const int SAMPLE_FRAME_COUNT = 48000 / 1000 * 40;

		public override void OnLoaded() {
			_output = Sound.CreateStream(5f);
			Load(_output);
			LoadOpus();
		}

		public void LoadInput(string deviceName = null) {
			if (deviceName is null) {
				deviceName = Engine.MainMic;
				Engine.MicChanged += LoadInput;
			}
			if (!Microphone.Start(deviceName)) {
				Log.Err($"Failed to load Mic {deviceName ?? "System Default"}");
				return;
			}
			_input = Microphone.Sound;
			Load(_input);
			_output = null;
			_loadedDevice = true;
		}

		public override void Received(Peer sender, IDataNode data) {
			if (_output is null || _encoder is null || _decoder is null || NoSync) {
				return;
			}
			var @in = ((DataNode<byte[]>)data).Value;
			var dataPacket = new float[SAMPLE_FRAME_COUNT];
			var amount = _decoder.Decode(@in, @in.Length, dataPacket, SAMPLE_FRAME_COUNT);
			_output.WriteSamples(dataPacket, amount);
		}

		public void Step() {
			if (_encoder is null || _decoder is null) {
				return;
			}
			if (!_loadedDevice) {
				return;
			}
			if (_input is null) {
				return;
			}
			if (NoSync) {
				return;
			}
			if (_input.UnreadSamples >= SAMPLE_FRAME_COUNT) {
				var dataPacket = new float[_input.UnreadSamples];
				_input.ReadSamples(ref dataPacket);
				var outpack = new byte[BitRate.Value];
				var amount = _encoder.Encode(dataPacket, SAMPLE_FRAME_COUNT, outpack, outpack.Length);
				Array.Resize(ref outpack, amount);
				World.BroadcastDataToAll(this, new DataNode<byte[]>(outpack), LiteNetLib.DeliveryMethod.Sequenced);
			}
		}
	}
}
