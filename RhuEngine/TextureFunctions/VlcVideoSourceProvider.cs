﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LibVLCSharp;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.ComponentModel;
using RhuEngine.Linker;
using LibVLCSharp.Shared;

namespace RhuEngine.VLC
{
	public sealed class VlcVideoSourceProvider : INotifyPropertyChanged, IDisposable
	{
		public VlcVideoSourceProvider() {
			VideoImage = new RImage(null);
			VideoSource = new RImageTexture2D(VideoImage);
		}
		/// <summary>
		/// The memory mapped file that contains the picture data
		/// </summary>
		private MemoryMappedFile _memoryMappedFile;

		/// <summary>
		/// The view that contains the pointer to the buffer that contains the picture data
		/// </summary>
		private MemoryMappedViewAccessor _memoryMappedView;

		/// <summary>
		/// The media player instance. You must call <see cref="CreatePlayer"/> before using this.
		/// </summary>
		public MediaPlayer MediaPlayer { get; private set; }

		public event Action RelaodTex;
		public RImage VideoImage { get; private set; }

		public RTexture2D VideoSource { get; private set; }

		public int Pitches { get; private set; }

		public uint ChannelCount { get; set; }

		/// <summary>
		/// Creates the player. This method must be called before using <see cref="MediaPlayer"/>
		/// </summary>
		public void LoadPlayer(MediaPlayer mediaPlayer) {
			MediaPlayer = mediaPlayer;
			MediaPlayer.EnableHardwareDecoding = true;
			MediaPlayer.SetVideoFormatCallbacks(VideoFormat, null);
			MediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo);
			MediaPlayer.SetAudioFormat("S16L",48000,ChannelCount);
			//_bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, (int)ChannelCount)) {
			//	DiscardOnBufferOverflow = true
			//};
			mediaPlayer.SetAudioCallbacks(PlayAudio, null, null, null, null);
		}

		//BufferedWaveProvider _bufferedWaveProvider;

		//public IWaveProvider Audio => _bufferedWaveProvider;


		public unsafe void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts) {
			//if (_bufferedWaveProvider is null) {
			//	return;
			//}
			//var amount = count * 2;
			//var buffer = new byte[amount];
			//Marshal.Copy(samples, buffer, 0, (int)amount);
			//_bufferedWaveProvider.AddSamples(buffer, 0, (int)amount);
		}

		/// <summary>
		/// Aligns dimension to the next multiple of mod
		/// </summary>
		/// <param name="dimension">The dimension to be aligned</param>
		/// <param name="mod">The modulus</param>
		/// <returns>The aligned dimension</returns>
		private static uint GetAlignedDimension(uint dimension, uint mod) {
			var modResult = dimension % mod;
			return modResult == 0 ? dimension : dimension + mod - (dimension % mod);
		}

		#region Vlc video callbacks
		/// <summary>
		/// Called by vlc when the video format is needed. This method allocats the picture buffers for vlc and tells it to set the chroma to RV24
		/// </summary>
		/// <param name="userdata">The user data that will be given to the <see cref="LockVideo"/> callback. It contains the pointer to the buffer</param>
		/// <param name="chroma">The chroma</param>
		/// <param name="width">The visible width</param>
		/// <param name="height">The visible height</param>
		/// <param name="pitches">The buffer width</param>
		/// <param name="lines">The buffer height</param>
		/// <returns>The number of buffers allocated</returns>

		private uint VideoFormat(ref IntPtr userdata, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines) {
			FourCCConverter.ToFourCC("RV24", chroma);

			//Correct video width and height according to TrackInfo
			var md = MediaPlayer.Media;
			foreach (var track in md.Tracks) {
				if (track.TrackType == TrackType.Video) {
					var trackInfo = track.Data.Video;
					if (trackInfo.Width > 0 && trackInfo.Height > 0) {
						width = trackInfo.Width;
						height = trackInfo.Height;
						if (trackInfo.SarDen != 0) {
							width = width * trackInfo.SarNum / trackInfo.SarDen;
						}
					}

					break;
				}
			}

			pitches = GetAlignedDimension((uint)(width * 24) / 8, 24);
			lines = GetAlignedDimension(height, 24);

			var size = pitches * lines;
			_memoryMappedFile = MemoryMappedFile.CreateNew(null, size);
			var handle = _memoryMappedFile.SafeMemoryMappedFileHandle.DangerousGetHandle();
			//VideoSource.SetSize((int)width, (int)height);
			Pitches = (int)pitches;
			_memoryMappedView = _memoryMappedFile.CreateViewAccessor();
			var viewHandle = _memoryMappedView.SafeMemoryMappedViewHandle.DangerousGetHandle();
			userdata = viewHandle;
			RelaodTex?.Invoke();
			return 1;
		}

		public byte[] rgbaData = null;

		unsafe void Convert(int pixelCount, IntPtr rgbData) {
			if ((rgbaData?.Length ?? 0) != pixelCount * sizeof(uint)) {
				rgbaData = new byte[pixelCount * sizeof(uint)];
			}
			fixed (byte* rgbaP = &rgbaData[0]) {
				var rgbP = (byte*)rgbData;
				for (long i = 0, offsetRgb = 0; i < pixelCount - 1; i++, offsetRgb += 3) {
					((uint*)rgbaP)[i] = *(uint*)(rgbP + offsetRgb) | 0xff000000;
				}
			}
		}

		/// <summary>
		/// Called by libvlc when it wants to acquire a buffer where to write
		/// </summary>
		/// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
		/// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
		/// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
		private IntPtr LockVideo(IntPtr userdata, IntPtr planes) {
			Marshal.WriteIntPtr(planes, userdata);
			return planes;
		}

		/// <summary>
		/// Called by libvlc when the picture has to be displayed.
		/// </summary>
		/// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
		/// <param name="picture">The pointer returned by the <see cref="LockVideo"/> callback. This is not used.</param>
		private void DisplayVideo(IntPtr userdata, IntPtr picture) {
			if (EngineHelpers.MainEngine.IsCloseing) {
				return;
			}
			if (VideoSource != null) {
				Convert((int)VideoSource.Width * (int)VideoSource.Height, userdata);
				if (rgbaData.Length < VideoSource.Width * VideoSource.Height) {
					return;
				}
				VideoImage.SetColors((int)VideoSource.Width, (int)VideoSource.Height, rgbaData, false);
			}
		}
		#endregion

		/// <summary>
		/// Removes the video (must be called from the Dispatcher thread)
		/// </summary>
		private void RemoveVideo() {
			_memoryMappedView?.Dispose();
			_memoryMappedView = null;
			_memoryMappedFile?.Dispose();
			_memoryMappedFile = null;
		}

		#region IDisposable Support
		private bool _disposedValue = false;

		/// <summary>
		/// Disposes the control.
		/// </summary>
		/// <param name="disposing">The parameter is not used.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		private void Dispose(bool disposing) {
			if (!_disposedValue) {
				_disposedValue = true;
				MediaPlayer?.Dispose();
				MediaPlayer = null;
				RemoveVideo();
			}
		}

		/// <summary>
		/// The destructor
		/// </summary>
		~VlcVideoSourceProvider() {
			Dispose(false);
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
