using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine
{
	public class RhubarbAtlasSheet : IDisposable
	{
		private RTexture2D _atlas;
		public RTexture2D Atlas { get => _atlas; set { _atlas = value; UpdateAtlas(); } }
		private Vector2i _gridSize;
		private Vector2i _lastGridSize;

		public Vector2i GridSize { get => _gridSize; set { _gridSize = value; UpdateGridSize(); } }

		private readonly List<List<RAtlasTexture>> _atlises = new();

		private void UpdateGridSize() {
			var sizeDef = _gridSize - _lastGridSize;
			if (sizeDef.y != 0) {
				if (sizeDef.y > 0) {
					for (var i = 0; i < sizeDef.y; i++) {
						var newRow = new List<RAtlasTexture>();
						for (var x = 0; x < _gridSize.x; x++) {
							var texture = new RAtlasTexture(null) {
								Atlas = _atlas
							};
							newRow.Add(texture);
						}
						_atlises.Add(newRow);
					}
				}
				else {
					for (var i = 0; i < -sizeDef.y; i++) {
						var location = _lastGridSize.y - i;
						foreach (var item in _atlises[location]) {
							item.Dispose();
						}
						_atlises.RemoveAt(location);
					}
				}
			}
			if (sizeDef.x != 0) {
				if (sizeDef.x > 0) {
					foreach (var item in _atlises) {
						for (var i = 0; i < sizeDef.x; i++) {
							var texture = new RAtlasTexture(null) {
								Atlas = _atlas
							};
							item.Add(texture);
						}
					}
				}
				else {
					foreach (var item in _atlises) {
						for (var i = 0; i < -sizeDef.x; i++) {
							var location = _lastGridSize.x - i;
							item.RemoveAt(location);
						}
					}
				}
			}
			_lastGridSize = GridSize;
			UpdateSizes();
		}


		private void UpdateAtlas() {
			foreach (var group in _atlises) {
				foreach (var item in group) {
					item.Atlas = _atlas;
				}
			}
			UpdateSizes();
		}

		private void UpdateSizes() {
			var width = _atlas.Width;
			var hight = _atlas.Height;
			var x_ = 0f;
			if (GridSize.x != 0) {
				x_ = width / GridSize.x;
			}
			var y_ = 0f;
			if (GridSize.y != 0) {
				y_ = hight / GridSize.y;
			}
			var sizeOfEach = new Vector2f(x_, y_);
			for (var x = 0; x < GridSize.x; x++) {
				for (var y = 0; y < GridSize.y; y++) {
					var min = new Vector2f(sizeOfEach.x * x, sizeOfEach.y * y);
					_atlises[y][x].RegionPos = min;
					_atlises[y][x].RegionScale = sizeOfEach;
				}
			}
		}
		public void Dispose() {
			foreach (var group in _atlises) {
				foreach (var item in group) {
					item?.Dispose();
				}
			}
			GC.SuppressFinalize(this);
		}
		public enum RhubarbIcons
		{
			Settings = 0,
			Folder = 1,
			Bell = 2,
			Mic = 3,
			NoAudio = 4,
			AudioLow = 5,
			AudioNormal = 6,
			AudioHigh = 7,
			NoNotifications = 8,
			World = 9,
			User = 10,
			Users = 11,
			Shutdown = 12,
			RhubarbRedWhite = 13,
			NoMic = 14,
			Mute = 15,
			RhubarbVR = 16,
			NoWorld = 17,
			Minimize = 18,
			Uncollapse = 19,
			Close = 20,
			RhubarbVRWhiteNegative = 21,
			RhubarbPieWhite = 22,
			RhubarbRed = 23,
			RhubarbPieRed = 24,
			RhubarbVRWhite = 25,
			Collapse = 26,
			CheckCircle = 27,
			CheckCircleSet = 28,
			Play = 29,
			Pause = 30,
			Stop = 31,
			UpArrow = 32,
			DownArrow = 33,
			Plus = 34,
			Minus = 35,
			Mail = 36,
			AddUser = 37,
			RemoveUser = 38,
			Undo = 39,
			Redo = 40,
			Search = 41,
			Worlds = 42,
			AddWorld = 43,
			RemoveWorld = 44,
			File = 45,
			AddFile = 46,
			RemoveFile = 47,
			Mouse = 48,
			Share = 49,
			Trash = 50,
			Recycle = 51,
			UpArrowSlim = 52,
			DownArrowSlim = 53,
			Pencil = 54,
			Pen = 55,
			Share2 = 56,
			Send = 57,
			Link = 58,
			Unlink = 59,
			Eraser = 60,
			TrashOpen = 61,
			PutInTrash = 62,
			Brush = 63,
			RecycleMan = 64,
			PaintBucket = 65,
			Floodlight = 66,
			Code = 67,
			BallFill = 68,
			BallStroke = 69,
			Save = 70,
			FloppyDisk = 71,
			Star = 72,
			Bookmark = 73,
			Phone = 74,
			PhoneButBroken = 75,
			Xut = 76,
			Camera = 77,
			Cancel = 78,
			Backspace = 79,
			SignalHigh = 80,
			SignalLow = 81,
			Flashlight = 82,
			FluorescentLamp = 83,
			Copy = 84,
			Key = 85,
			Options = 86,
			Cloud = 87,
			ShoppingCart = 88,
			Login = 89,
			FastFood = 90,
			Message = 91,
			Share3 = 92,
			LightingBolt = 93,
			Wrench = 94,
			Picture = 95,
			ThreeDotsVertical = 96,
			Triangle = 97,
			CheckMark = 98,
			Airplane = 99,
			PhoneStop = 100,
			Coffee = 101,
			Clock = 102,
			Potato = 103,
			Location = 104,
			List = 105,
			SignalMid = 106,
			TikiTorch = 107,
			FluorescentLightbulb = 108,
			Egg = 109,
			Warning = 110,
			SignUp = 111,
			SignDown = 112,
			Mirror = 113,
			PhoneForward = 114,
			IncreaseIndent = 115,
			DecreaseIndent = 116,
			BarcodeScanner = 117,
			ThreeDotsHorizontal = 118,
			House = 119,
			Mousehardware = 120,
			Sun = 121,
			Moon = 122,
			Archive = 123,
			Tree = 124,
			Sliders = 125,
			Music = 126,
			Battery5 = 127,
			Battery4 = 128,
			Battery3 = 129,
			ZoomIn = 130,
			ArrowUpDown = 131,
			RecordingCamera = 132,
			LED = 133,
			QuestionMark = 134,
			ExclamationMark = 135,
			Comma = 136,
			MissingFile = 137,
			LunaVR = 138,
			Robot = 139,
			CursorMove = 140,
			CursorSelect = 141,
			CursorCircle = 142,
			CursorGear = 143,
			CursorScale = 144,
			Cursor = 145,
			VRHeadset = 146,
			OnlineStatus=147,
			IdleStatus = 148,
			DoNotDistrubeStatus = 149,
			StreamingStatus = 150,
			OfflineStatus = 151,
            LogOut=152,
			/*=153,*/
			Battery1 = 154,
			Battery2 = 155,
			ZoomOut = 156,
			MenuBar = 157,
			StopSign = 158,
			CropIcon = 159,
			Terminal = 160,
			Unlock = 161,
			Lock = 162,
			MilkSnake = 163,
		}

		public RTexture2D GetElement(RhubarbIcons icons) {
			var number = (int)icons;
			return GetElement(number % 26, number / 26);
		}

		public RTexture2D GetElement(Vector2i value) {
			return _atlises[value.y][value.x];
		}
		public RTexture2D GetElement(int x, int y) {
			return _atlises[y][x];
		}
	}
}
