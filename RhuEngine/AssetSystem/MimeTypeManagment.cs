using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;

namespace RhuEngine
{

	 static class CustomMimeTypes
	{
		private const string DEFAULT_FALLBACK_MIME_TYPE = "application/octet-stream";
		private static string _fallbackMimeType;


		[AllowNull]
		public static string FallbackMimeType
		{
			get => _fallbackMimeType;
			set => _fallbackMimeType = value ?? DEFAULT_FALLBACK_MIME_TYPE;
		}

		private static readonly Dictionary<string, string> _typeMap;

		static CustomMimeTypes() {
			_fallbackMimeType = DEFAULT_FALLBACK_MIME_TYPE;

			_typeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
			//	{ "123", "application/vnd.lotus-1-2-3" },
			};
		}

		/// <summary>
		/// Attempts to fetch all available file extensions for a MIME-type.
		/// </summary>
		/// <param name="mimeType">The name of the MIME-type</param>
		/// <returns>All available extensions for the given MIME-type</returns>
		public static IEnumerable<string> GetMimeTypeExtensions(string mimeType) {
			if (mimeType is null) {
				throw new ArgumentNullException(nameof(mimeType));
			}

			return _typeMap
				.Where(keyPair => string.Equals(keyPair.Value, mimeType, StringComparison.OrdinalIgnoreCase))
				.Select(keyPair => keyPair.Key);
		}

		/// <summary>
		/// Tries to get the MIME-type for the given file name.
		/// </summary>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="mimeType">The MIME-type for the given file name.</param>
		/// <returns><c>true</c> if a MIME-type was found, <c>false</c> otherwise.</returns>
		public static bool TryGetMimeType(string? fileName, [NotNullWhen(true)] out string? mimeType) {
			if (fileName is null) {
				mimeType = null;
				return false;
			}

			var dotIndex = fileName.LastIndexOf('.');

			if (dotIndex != -1 && fileName.Length > dotIndex + 1) {
				return _typeMap.TryGetValue(fileName.Substring(dotIndex + 1), out mimeType);
			}

			mimeType = null;
			return false;
		}

		/// <summary>
		/// Gets the MIME-type for the given file name,
		/// or <see cref="FallbackMimeType"/> if a mapping doesn't exist.
		/// </summary>
		/// <param name="fileName">The name of the file.</param>
		/// <returns>The MIME-type for the given file name.</returns>
		public static string GetMimeType(string fileName) {
			if (fileName is null) {
				throw new ArgumentNullException(nameof(fileName));
			}

			return TryGetMimeType(fileName, out var result) ? result : FallbackMimeType;
		}
	}


	public static class MimeTypeManagment
	{

		/// <summary>
		/// Attempts to fetch all available file extensions for a MIME-type.
		/// </summary>
		/// <param name="mimeType">The name of the MIME-type</param>
		/// <returns>All available extensions for the given MIME-type</returns>
		public static IEnumerable<string> GetMimeTypeExtensions(string mimeType) {
			foreach (var item in CustomMimeTypes.GetMimeTypeExtensions(mimeType)) {
				yield return item;
			}
			foreach (var item in MimeTypes.GetMimeTypeExtensions(mimeType)) {
				yield return item;
			}
		}

		/// <summary>
		/// Tries to get the MIME-type for the given file name.
		/// </summary>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="mimeType">The MIME-type for the given file name.</param>
		/// <returns><c>true</c> if a MIME-type was found, <c>false</c> otherwise.</returns>
		public static bool TryGetMimeType(string? fileName, [NotNullWhen(true)] out string? mimeType) {
			return CustomMimeTypes.TryGetMimeType(fileName, out mimeType) || MimeTypes.TryGetMimeType(fileName, out mimeType);
		}

		/// <summary>
		/// Gets the MIME-type for the given file name,
		/// or <see cref="FallbackMimeType"/> if a mapping doesn't exist.
		/// </summary>
		/// <param name="fileName">The name of the file.</param>
		/// <returns>The MIME-type for the given file name.</returns>
		public static string GetMimeType(string fileName) {
			return TryGetMimeType(fileName,out var mimeType) ? mimeType : MimeTypes.FallbackMimeType;
		}
	}
}
