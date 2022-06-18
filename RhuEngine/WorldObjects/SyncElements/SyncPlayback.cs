using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.DataStructure;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public class SyncPlayback : Sync<Playback>, ISyncMember
	{

		public event Func<double> StateChange;

		public override Playback StartingValue => new(){ Speed = 1f, Looping = true, Playing = false, Offset = World.WorldTime };
		public override void UpdatedValue() {
			ClipLength = StateChange?.Invoke() ?? double.NegativeInfinity;
		}
		[Exposed]
		public double ClipLength { get; set; } = double.NegativeInfinity;
		[Exposed]
		public double Position
		{
			get => ProccessPosition();
			set => Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = Value.Playing, Speed = Value.Speed, Position = value };
		}

		[Exposed]
		public bool Playing => ((RawPos() < ClipLength) || Looping || Stream) && Value.Playing;
		[Exposed]
		public bool Stream => ClipLength >= double.PositiveInfinity;

		public double ProccessPosition() {
			return Value.Playing ? Stream ? 1f : Looping ? RawPos() % ClipLength : RawPos() > ClipLength ? ClipLength : RawPos() : Value.Position;
		}
		public double RawPos() {
			return ((World.WorldTime - Value.Offset) * Speed) + Value.Position;
		}

		public override void OnLoad(SyncObjectDeserializerObject networked) {
			if (networked.hasNewRefIDs) {
				Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = Value.Playing, Speed = Value.Speed, Position = Value.Position };
			}
		}
		public override Playback OnSave(SyncObjectSerializerObject serializerObject) {
			return new Playback { Looping = Value.Looping, Offset = 0f, Playing = Value.Playing, Speed = Value.Speed, Position = RawPos() };
		}
		[Exposed]
		public bool Looping
		{
			get => Value.Looping;
			set => Value = new Playback { Looping = value, Offset = Value.Offset, Playing = Value.Playing, Speed = Value.Speed, Position = Value.Position };
		}
		[Exposed]
		public float Speed
		{
			get => Value.Speed;
			set => Value = new Playback { Looping = Value.Looping, Offset = Value.Offset, Playing = Value.Playing, Speed = value, Position = Value.Position };
		}

		[Exposed]
		public void Play() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = true, Speed = Value.Speed, Position = 0f };
		}
		[Exposed]
		public void Stop() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = false, Speed = Value.Speed, Position = 0f };
		}
		[Exposed]
		public void Pause() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = false, Speed = Value.Speed, Position = RawPos() };
		}
		[Exposed]
		public void Resume() {
			Value = new Playback { Looping = Value.Looping, Offset = World.WorldTime, Playing = true, Speed = Value.Speed, Position = Value.Position };
		}
	}
}
