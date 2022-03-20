using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public class SyncPlayback : Sync<Playback>
	{

		public event Func<double> StateChange;

		public override Playback StartingValue => new(){ Speed = 1f, Looping = true, Playing = false, Offset = World.WorldTime };
		public override void UpdatedValue() {
			ClipLength = StateChange?.Invoke() ?? double.NegativeInfinity;
		}
		[Exsposed]
		public double ClipLength { get; set; } = double.NegativeInfinity;
		[Exsposed]
		public double Position
		{
			get => ProccessPosition();
			set => Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = Value.Playing, Speed = Value.Speed, Position = value };
		}

		[Exsposed]
		public bool Playing => ((RawPos() < ClipLength) || Looping || Stream) && Value.Playing;
		[Exsposed]
		public bool Stream => ClipLength >= double.PositiveInfinity;

		public double ProccessPosition() {
			return Value.Playing ? Stream ? 1f : Looping ? RawPos() % ClipLength : RawPos() > ClipLength ? ClipLength : RawPos() : Value.Position;
		}
		public double RawPos() {
			return ((Value.Offset - World.WorldTime) * Speed) + Value.Position;
		}

		public override void OnLoad(SyncObjectDeserializerObject networked) {
			if (networked.hasNewRefIDs) {
				Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = Value.Playing, Speed = Value.Speed, Position = Value.Position };
			}
		}
		public override Playback OnSave(SyncObjectSerializerObject serializerObject) {
			return new Playback { Looping = Value.Looping, Offset = 0f, Playing = Value.Playing, Speed = Value.Speed, Position = RawPos() };
		}
		[Exsposed]
		public bool Looping
		{
			get => Value.Looping;
			set => Value = new Playback { Looping = value, Offset = Value.Offset, Playing = Value.Playing, Speed = Value.Speed, Position = Value.Position };
		}
		[Exsposed]
		public float Speed
		{
			get => Value.Speed;
			set => Value = new Playback { Looping = Value.Looping, Offset = Value.Offset, Playing = Value.Playing, Speed = value, Position = Value.Position };
		}

		[Exsposed]
		public void Play() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = true, Speed = Value.Speed, Position = 0f };
		}
		[Exsposed]
		public void Stop() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = false, Speed = Value.Speed, Position = 0f };
		}
		[Exsposed]
		public void Pause() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = false, Speed = Value.Speed, Position = RawPos() };
		}
		[Exsposed]
		public void Resume() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = true, Speed = Value.Speed, Position = Value.Position };
		}
	}
}
