﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public enum RClockDirection : long
	{
		Clockwise,
		Counterclockwise,
		None
	}

	/// <summary>Severity of a log item.</summary>
	public enum LogLevel
	{
		None = 0,
		/// <summary>This is for diagnostic information, where you need to know
		/// details about what -exactly- is going on in the system. This
		/// info doesn't surface by default.</summary>
		Diagnostic,
		/// <summary>This is non-critical information, just to let you know what's
		/// going on.</summary>
		Info,
		/// <summary>Something bad has happened, but it's still within the realm of
		/// what's expected.</summary>
		Warning,
		/// <summary>Danger Will Robinson! Something really bad just happened and
		/// needs fixing!</summary>
		Error,
	}


	/// <summary>When rendering content, you can filter what you're rendering by the
	/// RenderLayer that they're on. This allows you to draw items that are
	/// visible in one render, but not another. For example, you may wish
	/// to draw a player's avatar in a 'mirror' rendertarget, but not in
	/// the primary display. See `Renderer.LayerFilter` for configuring what
	/// the primary display renders.</summary>
	[Flags]
	public enum RenderLayer
	{
		MainLayer = 1 << 0,
		/// <summary>Render layer 1.</summary>
		Text = 1 << 1,
		/// <summary>Render layer 2.</summary>
		UI = 1 << 2,
		/// <summary>Render layer 3.</summary>
		Layer3 = 1 << 3,
		/// <summary>Render layer 4.</summary>
		Layer4 = 1 << 4,
		/// <summary>Render layer 5.</summary>
		Layer5 = 1 << 5,
		/// <summary>Render layer 6.</summary>
		Layer6 = 1 << 6,
		/// <summary>Render layer 7.</summary>
		Layer7 = 1 << 7,
		/// <summary>Render layer 8.</summary>
		Layer8 = 1 << 8,
		/// <summary>Render layer 9.</summary>
		Layer9 = 1 << 9,
		/// <summary>Render layer 10.</summary>
		Layer10 = 1 << 10,
		/// <summary>Render layer 11.</summary>
		Layer11 = 1 << 11,
		/// <summary>Render layer 12.</summary>
		Layer12 = 1 << 12,
		/// <summary>Render layer 13.</summary>
		Layer13 = 1 << 13,
		/// <summary>Render layer 14.</summary>
		Layer14 = 1 << 14,
		/// <summary>Render layer 15.</summary>
		Layer15 = 1 << 15,
		/// <summary>Render layer 16.</summary>
		Layer16 = 1 << 16,
		/// <summary>Render layer 17.</summary>
		Layer17 = 1 << 17,
		/// <summary>Render layer 18.</summary>
		Layer18 = 1 << 18,
		/// <summary>Render layer 19.</summary>
		Layer19 = 1 << 19,
		/// <summary>Render layer 20.</summary>
		Layer20 = 1 << 20,
		/// <summary>This is a flag that specifies all possible layers. If you
		/// want to render all layers, then this is the layer filter
		/// you would use. This is the default for render filtering.</summary>
		MainCam = MainLayer | Text | UI | Layer3 | Layer4 | Layer5 | Layer6,
	}

	/// <summary>Also known as 'alpha' for those in the know. But there's
	/// actually more than one type of transparency in rendering! The
	/// horrors. We're keepin' it fairly simple for now, so you get three
	/// options!</summary>
	public enum Transparency
	{
		/// <summary>Not actually transparent! This is opaque! Solid! It's
		/// the default option, and it's the fastest option! Opaque objects
		/// write to the z-buffer, the occlude pixels behind them, and they
		/// can be used as input to important Mixed Reality features like
		/// Late Stage Reprojection that'll make your view more stable!</summary>
		None = 1,
		/// <summary>This will blend with the pixels behind it. This is 
		/// transparent! You may not want to write to the z-buffer, and it's
		/// slower than opaque materials.</summary>
		Blend,
		/// <summary>This will straight up add the pixel color to the color
		/// buffer! This usually looks -really- glowy, so it makes for good
		/// particles or lighting effects.</summary>
		Add,
	}


	/// <summary>An enum for indicating which hand to use!</summary>
	public enum Handed
	{
		/// <summary>Left hand.</summary>
		Left = 0,
		/// <summary>Right hand.</summary>
		Right = 1,
		/// <summary>The number of hands one generally has, this is much nicer
		/// than doing a for loop with '2' as the condition! It's much clearer
		/// when you can loop Hand.Max times instead.</summary>
		Max = 2,
	}

	public enum MouseKeys 
	{
		/// <summary>Left mouse button.</summary>
		MouseLeft = 0x01,
		/// <summary>Right mouse button.</summary>
		MouseRight = 0x02,
		/// <summary>Center mouse button.</summary>
		MouseCenter = 0x04,
		/// <summary>Mouse forward button.</summary>
		MouseForward = 0x05,
		/// <summary>Mouse back button.</summary>
		MouseBack = 0x06,
	}


	/// <summary>A collection of system key codes, representing keyboard
	/// characters and mouse buttons. Based on VK codes.</summary>
	public enum Key
	{
		None = 0x00,
		/// <summary>Left mouse button.</summary>
		MouseLeft = 0x01,
		/// <summary>Right mouse button.</summary>
		MouseRight = 0x02,
		/// <summary>Center mouse button.</summary>
		MouseCenter = 0x04,
		/// <summary>Mouse forward button.</summary>
		MouseForward = 0x05,
		/// <summary>Mouse back button.</summary>
		MouseBack = 0x06,
		/// <summary>Backspace</summary>
		Backspace = 0x08,
		/// <summary>Tab</summary>
		Tab = 0x09,
		/// <summary>Return, or Enter.</summary>
		Return = 0x0D,
		/// <summary>Left or right Shift.</summary>
		Shift = 0x10,
		/// <summary>Left or right Control key.</summary>
		Ctrl = 0x11,
		/// <summary>Left or right Alt key.</summary>
		Alt = 0x12,
		/// <summary>
		/// PauseBreak
		/// </summary>
		Pause = 0x13,
		/// <summary>This behaves a little differently! This tells the toggle
		/// state of caps lock, rather than the key state itself.</summary>
		CapsLock = 0x14,
		/// <summary>Escape</summary>
		Esc = 0x1B,
		/// <summary>Space</summary>
		Space = 0x20,
		/// <summary>End</summary>
		End = 0x23,
		/// <summary>Home</summary>
		Home = 0x24,
		/// <summary>Left arrow key.</summary>
		Left = 0x25,
		/// <summary>Right arrow key.</summary>
		Right = 0x27,
		/// <summary>Up arrow key.</summary>
		Up = 0x26,
		/// <summary>Down arrow key.</summary>
		Down = 0x28,
		/// <summary>Page up</summary>
		PageUp = 0x21,
		/// <summary>Page down</summary>
		PageDown = 0x22,
		/// <summary>Printscreen</summary>
		Printscreen = 0x2A,
		/// <summary>Any Insert key.</summary>
		Insert = 0x2D,
		/// <summary>Any Delete key.</summary>
		Del = 0x2E,
		/// <summary>Keyboard top row 0, with shift is ')'.</summary>
		N0 = 0x30,
		/// <summary>Keyboard top row 1, with shift is '!'.</summary>
		N1 = 0x31,
		/// <summary>Keyboard top row 2, with shift is '@'.</summary>
		N2 = 0x32,
		/// <summary>Keyboard top row 3, with shift is '#'.</summary>
		N3 = 0x33,
		/// <summary>Keyboard top row 4, with shift is '$'.</summary>
		N4 = 0x34,
		/// <summary>Keyboard top row 5, with shift is '%'.</summary>
		N5 = 0x35,
		/// <summary>Keyboard top row 6, with shift is '^'.</summary>
		N6 = 0x36,
		/// <summary>Keyboard top row 7, with shift is '&amp;'.</summary>
		N7 = 0x37,
		/// <summary>Keyboard top row 8, with shift is '*'.</summary>
		N8 = 0x38,
		/// <summary>Keyboard top row 9, with shift is '('.</summary>
		N9 = 0x39,
		/// <summary>a/A</summary>
		A = 0x41,
		/// <summary>b/B</summary>
		B = 0x42,
		/// <summary>c/C</summary>
		C = 0x43,
		/// <summary>d/D</summary>
		D = 0x44,
		/// <summary>e/E</summary>
		E = 0x45,
		/// <summary>f/F</summary>
		F = 0x46,
		/// <summary>g/G</summary>
		G = 0x47,
		/// <summary>h/H</summary>
		H = 0x48,
		/// <summary>i/I</summary>
		I = 0x49,
		/// <summary>j/J</summary>
		J = 0x4A,
		/// <summary>k/K</summary>
		K = 0x4B,
		/// <summary>l/L</summary>
		L = 0x4C,
		/// <summary>m/M</summary>
		M = 0x4D,
		/// <summary>n/N</summary>
		N = 0x4E,
		/// <summary>o/O</summary>
		O = 0x4F,
		/// <summary>p/P</summary>
		P = 0x50,
		/// <summary>q/Q</summary>
		Q = 0x51,
		/// <summary>r/R</summary>
		R = 0x52,
		/// <summary>s/S</summary>
		S = 0x53,
		/// <summary>t/T</summary>
		T = 0x54,
		/// <summary>u/U</summary>
		U = 0x55,
		/// <summary>v/V</summary>
		V = 0x56,
		/// <summary>w/W</summary>
		W = 0x57,
		/// <summary>x/X</summary>
		X = 0x58,
		/// <summary>y/Y</summary>
		Y = 0x59,
		/// <summary>z/Z</summary>
		Z = 0x5A,
		App = 0x5B,
		/// <summary>0 on the numpad, when numlock is on.</summary>
		Num0 = 0x60,
		/// <summary>1 on the numpad, when numlock is on.</summary>
		Num1 = 0x61,
		/// <summary>2 on the numpad, when numlock is on.</summary>
		Num2 = 0x62,
		/// <summary>3 on the numpad, when numlock is on.</summary>
		Num3 = 0x63,
		/// <summary>4 on the numpad, when numlock is on.</summary>
		Num4 = 0x64,
		/// <summary>5 on the numpad, when numlock is on.</summary>
		Num5 = 0x65,
		/// <summary>6 on the numpad, when numlock is on.</summary>
		Num6 = 0x66,
		/// <summary>7 on the numpad, when numlock is on.</summary>
		Num7 = 0x67,
		/// <summary>8 on the numpad, when numlock is on.</summary>
		Num8 = 0x68,
		/// <summary>9 on the numpad, when numlock is on.</summary>
		Num9 = 0x69,
		/// <summary>Function key F1.</summary>
		F1 = 0x70,
		/// <summary>Function key F2.</summary>
		F2 = 0x71,
		/// <summary>Function key F3.</summary>
		F3 = 0x72,
		/// <summary>Function key F4.</summary>
		F4 = 0x73,
		/// <summary>Function key F5.</summary>
		F5 = 0x74,
		/// <summary>Function key F6.</summary>
		F6 = 0x75,
		/// <summary>Function key F7.</summary>
		F7 = 0x76,
		/// <summary>Function key F8.</summary>
		F8 = 0x77,
		/// <summary>Function key F9.</summary>
		F9 = 0x78,
		/// <summary>Function key F10.</summary>
		F10 = 0x79,
		/// <summary>Function key F11.</summary>
		F11 = 0x7A,
		/// <summary>Function key F12.</summary>
		F12 = 0x7B,
		/// <summary>
		/// ScrollLock
		/// </summary>
		ScrollLock = 0x91,
		/// <summary>,/&lt;</summary>
		Comma = 0xBC,
		/// <summary>./&gt;</summary>
		Period = 0xBE,
		/// <summary>/</summary>
		SlashFwd = 0xBF,
		/// <summary>\</summary>
		SlashBack = 0xDC,
		/// <summary>;/:</summary>
		Semicolon = 0xBA,
		/// <summary>'/"</summary>
		Apostrophe = 0xDE,
		/// <summary>[/{</summary>
		BracketOpen = 0xDB,
		/// <summary>]/}</summary>
		BracketClose = 0xDD,
		/// <summary>-/_</summary>
		Minus = 0xBD,
		/// <summary>=/+</summary>
		Equals = 0xBB,
		/// <summary>`/~</summary>
		Backtick = 0xc0,
		/// <summary>The Windows/Mac Command button on the left side of the keyboard.</summary>
		LCmd = 0x5B,
		/// <summary>The Windows/Mac Command button on the right side of the keyboard.</summary>
		RCmd = 0x5C,
		/// <summary>Numpad '*', NOT the same as number row '*'.</summary>
		Multiply = 0x6A,
		/// <summary>Numpad '+', NOT the same as number row '+'.</summary>
		Add = 0x6B,
		/// <summary>Numpad '-', NOT the same as number row '-'.</summary>
		Subtract = 0x6D,
		/// <summary>Numpad '.', NOT the same as character '.'.</summary>
		Decimal = 0x6E,
		/// <summary>Numpad '/', NOT the same as character '/'.</summary>
		Divide = 0x6F,
		/// <summary>Maximum value for key codes.</summary>
		MAX = 0xFF,
	}
}
