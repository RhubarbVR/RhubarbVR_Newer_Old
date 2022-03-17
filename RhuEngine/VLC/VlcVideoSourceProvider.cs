using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Vlc.DotNet.Core;
using System.ComponentModel;
using StereoKit;

namespace RhuEngine.VLC
{

	/// <summary>
	/// The class that can provide a Wpf Image Source to display the video.
	/// </summary>
	public class VlcVideoSourceProvider : INotifyPropertyChanged, IDisposable
	{
        /// <summary>
        /// The memory mapped file that contains the picture data
        /// </summary>
        private MemoryMappedFile _memoryMappedFile;

        /// <summary>
        /// The view that contains the pointer to the buffer that contains the picture data
        /// </summary>
        private MemoryMappedViewAccessor _memoryMappedView;

		private bool _isAlphaChannelEnabled;
		private bool _playerCreated;

		/// <summary>
		/// The media player instance. You must call <see cref="CreatePlayer"/> before using this.
		/// </summary>
		public VlcMediaPlayer MediaPlayer { get; private set; }

		/// <summary>
		/// Defines if <see cref="VideoSource"/> pixel format is <see cref="PixelFormats.Bgr32"/> or <see cref="PixelFormats.Bgra32"/>
		/// </summary>
		public bool IsAlphaChannelEnabled
		{
			get => _isAlphaChannelEnabled;

			set {
				_isAlphaChannelEnabled = !_playerCreated
					? value
					:                    throw new InvalidOperationException("IsAlphaChannelEnabled property should be changed only before CreatePlayer method is called.");
			}
		}

		/// <summary>
		/// Creates the player. This method must be called before using <see cref="MediaPlayer"/>
		/// </summary>
		/// <param name="vlcLibDirectory">The directory where to find the vlc library</param>
		/// <param name="vlcMediaPlayerOptions">The initialization options to be given to libvlc</param>
		public void CreatePlayer(DirectoryInfo vlcLibDirectory, params string[] vlcMediaPlayerOptions) {
			var directoryInfo = vlcLibDirectory ?? throw new ArgumentNullException(nameof(vlcLibDirectory));

			MediaPlayer = new VlcMediaPlayer(directoryInfo, vlcMediaPlayerOptions);

			MediaPlayer.SetVideoFormatCallbacks(VideoFormat, CleanupVideo);
			MediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo, IntPtr.Zero);

			_playerCreated = true;
		}

		/// <summary>
		/// Aligns dimension to the next multiple of mod
		/// </summary>
		/// <param name="dimension">The dimension to be aligned</param>
		/// <param name="mod">The modulus</param>
		/// <returns>The aligned dimension</returns>
		private uint GetAlignedDimension(uint dimension, uint mod) {
			var modResult = dimension % mod;
			return modResult == 0 ? dimension : dimension + mod - (dimension % mod);
		}

		#region Vlc video callbacks
		/// <summary>
		/// Called by vlc when the video format is needed. This method allocats the picture buffers for vlc and tells it to set the chroma to RV32
		/// </summary>
		/// <param name="userdata">The user data that will be given to the <see cref="LockVideo"/> callback. It contains the pointer to the buffer</param>
		/// <param name="chroma">The chroma</param>
		/// <param name="width">The visible width</param>
		/// <param name="height">The visible height</param>
		/// <param name="pitches">The buffer width</param>
		/// <param name="lines">The buffer height</param>
		/// <returns>The number of buffers allocated</returns>
		private uint VideoFormat(out IntPtr userdata, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines) {
			var pixelFormat = TexFormat.Bgra32;
			FourCCConverter.ToFourCC("RV32", chroma);

			//Correct video width and height according to TrackInfo
			var md = MediaPlayer.GetMedia();
			foreach (var track in md.Tracks) {
				if (track.Type == MediaTrackTypes.Video) {
					var trackInfo = (VideoTrack)track.TrackInfo;
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

			pitches = GetAlignedDimension((uint)(width * 32) / 8, 32);
			lines = GetAlignedDimension(height, 32);

			var size = pitches * lines;
            _memoryMappedFile = MemoryMappedFile.CreateNew(null, size);
			var handle = _memoryMappedFile.SafeMemoryMappedFileHandle.DangerousGetHandle();
			var args = new {
				width = width,
				height = height,
				pixelFormat = pixelFormat,
				pitches = pitches
			};

			//this.dispatcher.Invoke((Action)(() => {
			//	this.VideoSource = (InteropBitmap)Imaging.CreateBitmapSourceFromMemorySection(handle,
			//		(int)args.width, (int)args.height, args.pixelFormat, (int)args.pitches, 0);
			//}));
			_memoryMappedView = _memoryMappedFile.CreateViewAccessor();
			var viewHandle = _memoryMappedView.SafeMemoryMappedViewHandle.DangerousGetHandle();
			userdata = viewHandle;
			return 1;
		}
		/// <summary>
		/// Called by Vlc when it requires a cleanup
		/// </summary>
		/// <param name="userdata">The parameter is not used</param>
		private void CleanupVideo(ref IntPtr userdata) {
			// This callback may be called by Dispose in the Dispatcher thread, in which case it deadlocks if we call RemoveVideo again in the same thread.
			//if (!_disposedValue) {
			//	this.dispatcher.Invoke(RemoveVideo);
			//}
		}

		/// <summary>
		/// Called by libvlc when it wants to acquire a buffer where to write
		/// </summary>
		/// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
		/// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
		/// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
		private IntPtr LockVideo(IntPtr userdata, IntPtr planes) {
			Marshal.WriteIntPtr(planes, userdata);
			return userdata;
		}

		/// <summary>
		/// Called by libvlc when the picture has to be displayed.
		/// </summary>
		/// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
		/// <param name="picture">The pointer returned by the <see cref="LockVideo"/> callback. This is not used.</param>
		private void DisplayVideo(IntPtr userdata, IntPtr picture) {
			// Invalidates the bitmap
			//this.dispatcher.BeginInvoke((Action)(() => {
			//	(this.VideoSource as InteropBitmap)?.Invalidate();
			//}));
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
		protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				_disposedValue = true;
				MediaPlayer?.Dispose();
				MediaPlayer = null;
				//this.dispatcher.BeginInvoke((Action)this.RemoveVideo);
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

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
