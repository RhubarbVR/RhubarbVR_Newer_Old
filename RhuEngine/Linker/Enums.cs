using System;
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
	public enum Key:long
	{
		//
		// Summary:
		//     Enum value which doesn't correspond to any key. This is used to initialize Godot.Key
		//     properties with a generic state.
		None = 0L,
		//
		// Summary:
		//     Keycodes with this bit applied are non-printable.
		Special = 0x400000L,
		//
		// Summary:
		//     Escape key.
		Escape = 4194305L,
		//
		// Summary:
		//     Tab key.
		Tab = 4194306L,
		//
		// Summary:
		//     Shift + Tab key.
		Backtab = 4194307L,
		//
		// Summary:
		//     Backspace key.
		Backspace = 4194308L,
		//
		// Summary:
		//     Return key (on the main keyboard).
		Enter = 4194309L,
		//
		// Summary:
		//     Enter key on the numeric keypad.
		KpEnter = 4194310L,
		//
		// Summary:
		//     Insert key.
		Insert = 4194311L,
		//
		// Summary:
		//     Delete key.
		Delete = 4194312L,
		//
		// Summary:
		//     Pause key.
		Pause = 4194313L,
		//
		// Summary:
		//     Print Screen key.
		Print = 4194314L,
		//
		// Summary:
		//     System Request key.
		Sysreq = 4194315L,
		//
		// Summary:
		//     Clear key.
		Clear = 4194316L,
		//
		// Summary:
		//     Home key.
		Home = 4194317L,
		//
		// Summary:
		//     End key.
		End = 4194318L,
		//
		// Summary:
		//     Left arrow key.
		Left = 4194319L,
		//
		// Summary:
		//     Up arrow key.
		Up = 4194320L,
		//
		// Summary:
		//     Right arrow key.
		Right = 4194321L,
		//
		// Summary:
		//     Down arrow key.
		Down = 4194322L,
		//
		// Summary:
		//     Page Up key.
		Pageup = 4194323L,
		//
		// Summary:
		//     Page Down key.
		Pagedown = 4194324L,
		//
		// Summary:
		//     Shift key.
		Shift = 4194325L,
		//
		// Summary:
		//     Control key.
		Ctrl = 4194326L,
		//
		// Summary:
		//     Meta key.
		Meta = 4194327L,
		//
		// Summary:
		//     Alt key.
		Alt = 4194328L,
		//
		// Summary:
		//     Caps Lock key.
		Capslock = 4194329L,
		//
		// Summary:
		//     Num Lock key.
		Numlock = 4194330L,
		//
		// Summary:
		//     Scroll Lock key.
		Scrolllock = 4194331L,
		//
		// Summary:
		//     F1 key.
		F1 = 4194332L,
		//
		// Summary:
		//     F2 key.
		F2 = 4194333L,
		//
		// Summary:
		//     F3 key.
		F3 = 4194334L,
		//
		// Summary:
		//     F4 key.
		F4 = 4194335L,
		//
		// Summary:
		//     F5 key.
		F5 = 4194336L,
		//
		// Summary:
		//     F6 key.
		F6 = 4194337L,
		//
		// Summary:
		//     F7 key.
		F7 = 4194338L,
		//
		// Summary:
		//     F8 key.
		F8 = 4194339L,
		//
		// Summary:
		//     F9 key.
		F9 = 4194340L,
		//
		// Summary:
		//     F10 key.
		F10 = 4194341L,
		//
		// Summary:
		//     F11 key.
		F11 = 4194342L,
		//
		// Summary:
		//     F12 key.
		F12 = 4194343L,
		//
		// Summary:
		//     F13 key.
		F13 = 4194344L,
		//
		// Summary:
		//     F14 key.
		F14 = 4194345L,
		//
		// Summary:
		//     F15 key.
		F15 = 4194346L,
		//
		// Summary:
		//     F16 key.
		F16 = 4194347L,
		//
		// Summary:
		//     F17 key.
		F17 = 4194348L,
		//
		// Summary:
		//     F18 key.
		F18 = 4194349L,
		//
		// Summary:
		//     F19 key.
		F19 = 4194350L,
		//
		// Summary:
		//     F20 key.
		F20 = 4194351L,
		//
		// Summary:
		//     F21 key.
		F21 = 4194352L,
		//
		// Summary:
		//     F22 key.
		F22 = 4194353L,
		//
		// Summary:
		//     F23 key.
		F23 = 4194354L,
		//
		// Summary:
		//     F24 key.
		F24 = 4194355L,
		//
		// Summary:
		//     F25 key. Only supported on macOS and Linux due to a Windows limitation.
		F25 = 4194356L,
		//
		// Summary:
		//     F26 key. Only supported on macOS and Linux due to a Windows limitation.
		F26 = 4194357L,
		//
		// Summary:
		//     F27 key. Only supported on macOS and Linux due to a Windows limitation.
		F27 = 4194358L,
		//
		// Summary:
		//     F28 key. Only supported on macOS and Linux due to a Windows limitation.
		F28 = 4194359L,
		//
		// Summary:
		//     F29 key. Only supported on macOS and Linux due to a Windows limitation.
		F29 = 4194360L,
		//
		// Summary:
		//     F30 key. Only supported on macOS and Linux due to a Windows limitation.
		F30 = 4194361L,
		//
		// Summary:
		//     F31 key. Only supported on macOS and Linux due to a Windows limitation.
		F31 = 4194362L,
		//
		// Summary:
		//     F32 key. Only supported on macOS and Linux due to a Windows limitation.
		F32 = 4194363L,
		//
		// Summary:
		//     F33 key. Only supported on macOS and Linux due to a Windows limitation.
		F33 = 4194364L,
		//
		// Summary:
		//     F34 key. Only supported on macOS and Linux due to a Windows limitation.
		F34 = 4194365L,
		//
		// Summary:
		//     F35 key. Only supported on macOS and Linux due to a Windows limitation.
		F35 = 4194366L,
		//
		// Summary:
		//     Multiply (*) key on the numeric keypad.
		KpMultiply = 4194433L,
		//
		// Summary:
		//     Divide (/) key on the numeric keypad.
		KpDivide = 4194434L,
		//
		// Summary:
		//     Subtract (-) key on the numeric keypad.
		KpSubtract = 4194435L,
		//
		// Summary:
		//     Period (.) key on the numeric keypad.
		KpPeriod = 4194436L,
		//
		// Summary:
		//     Add (+) key on the numeric keypad.
		KpAdd = 4194437L,
		//
		// Summary:
		//     Number 0 on the numeric keypad.
		Kp0 = 4194438L,
		//
		// Summary:
		//     Number 1 on the numeric keypad.
		Kp1 = 4194439L,
		//
		// Summary:
		//     Number 2 on the numeric keypad.
		Kp2 = 4194440L,
		//
		// Summary:
		//     Number 3 on the numeric keypad.
		Kp3 = 4194441L,
		//
		// Summary:
		//     Number 4 on the numeric keypad.
		Kp4 = 4194442L,
		//
		// Summary:
		//     Number 5 on the numeric keypad.
		Kp5 = 4194443L,
		//
		// Summary:
		//     Number 6 on the numeric keypad.
		Kp6 = 4194444L,
		//
		// Summary:
		//     Number 7 on the numeric keypad.
		Kp7 = 4194445L,
		//
		// Summary:
		//     Number 8 on the numeric keypad.
		Kp8 = 4194446L,
		//
		// Summary:
		//     Number 9 on the numeric keypad.
		Kp9 = 4194447L,
		//
		// Summary:
		//     Left Super key (Windows key).
		SuperL = 4194368L,
		//
		// Summary:
		//     Right Super key (Windows key).
		SuperR = 4194369L,
		//
		// Summary:
		//     Context menu key.
		Menu = 4194370L,
		//
		// Summary:
		//     Left Hyper key.
		HyperL = 4194371L,
		//
		// Summary:
		//     Right Hyper key.
		HyperR = 4194372L,
		//
		// Summary:
		//     Help key.
		Help = 4194373L,
		//
		// Summary:
		//     Left Direction key.
		DirectionL = 4194374L,
		//
		// Summary:
		//     Right Direction key.
		DirectionR = 4194375L,
		//
		// Summary:
		//     Media back key. Not to be confused with the Back button on an Android device.
		Back = 4194376L,
		//
		// Summary:
		//     Media forward key.
		Forward = 4194377L,
		//
		// Summary:
		//     Media stop key.
		Stop = 4194378L,
		//
		// Summary:
		//     Media refresh key.
		Refresh = 4194379L,
		//
		// Summary:
		//     Volume down key.
		Volumedown = 4194380L,
		//
		// Summary:
		//     Mute volume key.
		Volumemute = 4194381L,
		//
		// Summary:
		//     Volume up key.
		Volumeup = 4194382L,
		//
		// Summary:
		//     Bass Boost key.
		Bassboost = 4194383L,
		//
		// Summary:
		//     Bass up key.
		Bassup = 4194384L,
		//
		// Summary:
		//     Bass down key.
		Bassdown = 4194385L,
		//
		// Summary:
		//     Treble up key.
		Trebleup = 4194386L,
		//
		// Summary:
		//     Treble down key.
		Trebledown = 4194387L,
		//
		// Summary:
		//     Media play key.
		Mediaplay = 4194388L,
		//
		// Summary:
		//     Media stop key.
		Mediastop = 4194389L,
		//
		// Summary:
		//     Previous song key.
		Mediaprevious = 4194390L,
		//
		// Summary:
		//     Next song key.
		Medianext = 4194391L,
		//
		// Summary:
		//     Media record key.
		Mediarecord = 4194392L,
		//
		// Summary:
		//     Home page key.
		Homepage = 4194393L,
		//
		// Summary:
		//     Favorites key.
		Favorites = 4194394L,
		//
		// Summary:
		//     Search key.
		Search = 4194395L,
		//
		// Summary:
		//     Standby key.
		Standby = 4194396L,
		//
		// Summary:
		//     Open URL / Launch Browser key.
		Openurl = 4194397L,
		//
		// Summary:
		//     Launch Mail key.
		Launchmail = 4194398L,
		//
		// Summary:
		//     Launch Media key.
		Launchmedia = 4194399L,
		//
		// Summary:
		//     Launch Shortcut 0 key.
		Launch0 = 4194400L,
		//
		// Summary:
		//     Launch Shortcut 1 key.
		Launch1 = 4194401L,
		//
		// Summary:
		//     Launch Shortcut 2 key.
		Launch2 = 4194402L,
		//
		// Summary:
		//     Launch Shortcut 3 key.
		Launch3 = 4194403L,
		//
		// Summary:
		//     Launch Shortcut 4 key.
		Launch4 = 4194404L,
		//
		// Summary:
		//     Launch Shortcut 5 key.
		Launch5 = 4194405L,
		//
		// Summary:
		//     Launch Shortcut 6 key.
		Launch6 = 4194406L,
		//
		// Summary:
		//     Launch Shortcut 7 key.
		Launch7 = 4194407L,
		//
		// Summary:
		//     Launch Shortcut 8 key.
		Launch8 = 4194408L,
		//
		// Summary:
		//     Launch Shortcut 9 key.
		Launch9 = 4194409L,
		//
		// Summary:
		//     Launch Shortcut A key.
		Launcha = 4194410L,
		//
		// Summary:
		//     Launch Shortcut B key.
		Launchb = 4194411L,
		//
		// Summary:
		//     Launch Shortcut C key.
		Launchc = 4194412L,
		//
		// Summary:
		//     Launch Shortcut D key.
		Launchd = 4194413L,
		//
		// Summary:
		//     Launch Shortcut E key.
		Launche = 4194414L,
		//
		// Summary:
		//     Launch Shortcut F key.
		Launchf = 4194415L,
		//
		// Summary:
		//     Unknown key.
		Unknown = 0xFFFFFFL,
		//
		// Summary:
		//     Space key.
		Space = 0x20L,
		//
		// Summary:
		//     ! key.
		Exclam = 33L,
		//
		// Summary:
		//     " key.
		Quotedbl = 34L,
		//
		// Summary:
		//     # key.
		Numbersign = 35L,
		//
		// Summary:
		//     $ key.
		Dollar = 36L,
		//
		// Summary:
		//     % key.
		Percent = 37L,
		//
		// Summary:
		//     & key.
		Ampersand = 38L,
		//
		// Summary:
		//     ' key.
		Apostrophe = 39L,
		//
		// Summary:
		//     ( key.
		Parenleft = 40L,
		//
		// Summary:
		//     ) key.
		Parenright = 41L,
		//
		// Summary:
		//     * key.
		Asterisk = 42L,
		//
		// Summary:
		//     + key.
		Plus = 43L,
		//
		// Summary:
		//     , key.
		Comma = 44L,
		//
		// Summary:
		//     - key.
		Minus = 45L,
		//
		// Summary:
		//     . key.
		Period = 46L,
		//
		// Summary:
		//     / key.
		Slash = 47L,
		//
		// Summary:
		//     Number 0 key.
		Key0 = 48L,
		//
		// Summary:
		//     Number 1 key.
		Key1 = 49L,
		//
		// Summary:
		//     Number 2 key.
		Key2 = 50L,
		//
		// Summary:
		//     Number 3 key.
		Key3 = 51L,
		//
		// Summary:
		//     Number 4 key.
		Key4 = 52L,
		//
		// Summary:
		//     Number 5 key.
		Key5 = 53L,
		//
		// Summary:
		//     Number 6 key.
		Key6 = 54L,
		//
		// Summary:
		//     Number 7 key.
		Key7 = 55L,
		//
		// Summary:
		//     Number 8 key.
		Key8 = 56L,
		//
		// Summary:
		//     Number 9 key.
		Key9 = 57L,
		//
		// Summary:
		//     : key.
		Colon = 58L,
		//
		// Summary:
		//     ; key.
		Semicolon = 59L,
		//
		// Summary:
		//     < key.
		Less = 60L,
		//
		// Summary:
		//     = key.
		Equal = 61L,
		//
		// Summary:
		//     > key.
		Greater = 62L,
		//
		// Summary:
		//     ? key.
		Question = 0x3FL,
		//
		// Summary:
		//     @ key.
		At = 0x40L,
		//
		// Summary:
		//     A key.
		A = 65L,
		//
		// Summary:
		//     B key.
		B = 66L,
		//
		// Summary:
		//     C key.
		C = 67L,
		//
		// Summary:
		//     D key.
		D = 68L,
		//
		// Summary:
		//     E key.
		E = 69L,
		//
		// Summary:
		//     F key.
		F = 70L,
		//
		// Summary:
		//     G key.
		G = 71L,
		//
		// Summary:
		//     H key.
		H = 72L,
		//
		// Summary:
		//     I key.
		I = 73L,
		//
		// Summary:
		//     J key.
		J = 74L,
		//
		// Summary:
		//     K key.
		K = 75L,
		//
		// Summary:
		//     L key.
		L = 76L,
		//
		// Summary:
		//     M key.
		M = 77L,
		//
		// Summary:
		//     N key.
		N = 78L,
		//
		// Summary:
		//     O key.
		O = 79L,
		//
		// Summary:
		//     P key.
		P = 80L,
		//
		// Summary:
		//     Q key.
		Q = 81L,
		//
		// Summary:
		//     R key.
		R = 82L,
		//
		// Summary:
		//     S key.
		S = 83L,
		//
		// Summary:
		//     T key.
		T = 84L,
		//
		// Summary:
		//     U key.
		U = 85L,
		//
		// Summary:
		//     V key.
		V = 86L,
		//
		// Summary:
		//     W key.
		W = 87L,
		//
		// Summary:
		//     X key.
		X = 88L,
		//
		// Summary:
		//     Y key.
		Y = 89L,
		//
		// Summary:
		//     Z key.
		Z = 90L,
		//
		// Summary:
		//     [ key.
		Bracketleft = 91L,
		//
		// Summary:
		//     \ key.
		Backslash = 92L,
		//
		// Summary:
		//     ] key.
		Bracketright = 93L,
		//
		// Summary:
		//     ^ key.
		Asciicircum = 94L,
		//
		// Summary:
		//     _ key.
		Underscore = 95L,
		//
		// Summary:
		//     ` key.
		Quoteleft = 96L,
		//
		// Summary:
		//     { key.
		Braceleft = 123L,
		//
		// Summary:
		//     | key.
		Bar = 124L,
		//
		// Summary:
		//     } key.
		Braceright = 125L,
		//
		// Summary:
		//     ~ key.
		Asciitilde = 126L,
		//
		// Summary:
		//     Non-breakable space key.
		Nobreakspace = 160L,
		//
		// Summary:
		//     ¡ key.
		Exclamdown = 161L,
		//
		// Summary:
		//     ¢ key.
		Cent = 162L,
		//
		// Summary:
		//     £ key.
		Sterling = 163L,
		//
		// Summary:
		//     ¤ key.
		Currency = 164L,
		//
		// Summary:
		//     ¥ key.
		Yen = 165L,
		//
		// Summary:
		//     ¦ key.
		Brokenbar = 166L,
		//
		// Summary:
		//     § key.
		Section = 167L,
		//
		// Summary:
		//     ¨ key.
		Diaeresis = 168L,
		//
		// Summary:
		//     © key.
		Copyright = 169L,
		//
		// Summary:
		//     ª key.
		Ordfeminine = 170L,
		//
		// Summary:
		//     « key.
		Guillemotleft = 171L,
		//
		// Summary:
		//     ¬ key.
		Notsign = 172L,
		//
		// Summary:
		//     Soft hyphen key.
		Hyphen = 173L,
		//
		// Summary:
		//     ® key.
		Registered = 174L,
		//
		// Summary:
		//     ¯ key.
		Macron = 175L,
		//
		// Summary:
		//     ° key.
		Degree = 176L,
		//
		// Summary:
		//     ± key.
		Plusminus = 177L,
		//
		// Summary:
		//     ² key.
		Twosuperior = 178L,
		//
		// Summary:
		//     ³ key.
		Threesuperior = 179L,
		//
		// Summary:
		//     ´ key.
		Acute = 180L,
		//
		// Summary:
		//     µ key.
		Mu = 181L,
		//
		// Summary:
		//     ¶ key.
		Paragraph = 182L,
		//
		// Summary:
		//     · key.
		Periodcentered = 183L,
		//
		// Summary:
		//     ¸ key.
		Cedilla = 184L,
		//
		// Summary:
		//     ¹ key.
		Onesuperior = 185L,
		//
		// Summary:
		//     º key.
		Masculine = 186L,
		//
		// Summary:
		//     » key.
		Guillemotright = 187L,
		//
		// Summary:
		//     ¼ key.
		Onequarter = 188L,
		//
		// Summary:
		//     ½ key.
		Onehalf = 189L,
		//
		// Summary:
		//     ¾ key.
		Threequarters = 190L,
		//
		// Summary:
		//     ¿ key.
		Questiondown = 191L,
		//
		// Summary:
		//     À key.
		Agrave = 192L,
		//
		// Summary:
		//     Á key.
		Aacute = 193L,
		//
		// Summary:
		//     Â key.
		Acircumflex = 194L,
		//
		// Summary:
		//     Ã key.
		Atilde = 195L,
		//
		// Summary:
		//     Ä key.
		Adiaeresis = 196L,
		//
		// Summary:
		//     Å key.
		Aring = 197L,
		//
		// Summary:
		//     Æ key.
		Ae = 198L,
		//
		// Summary:
		//     Ç key.
		Ccedilla = 199L,
		//
		// Summary:
		//     È key.
		Egrave = 200L,
		//
		// Summary:
		//     É key.
		Eacute = 201L,
		//
		// Summary:
		//     Ê key.
		Ecircumflex = 202L,
		//
		// Summary:
		//     Ë key.
		Ediaeresis = 203L,
		//
		// Summary:
		//     Ì key.
		Igrave = 204L,
		//
		// Summary:
		//     Í key.
		Iacute = 205L,
		//
		// Summary:
		//     Î key.
		Icircumflex = 206L,
		//
		// Summary:
		//     Ï key.
		Idiaeresis = 207L,
		//
		// Summary:
		//     Ð key.
		Eth = 208L,
		//
		// Summary:
		//     Ñ key.
		Ntilde = 209L,
		//
		// Summary:
		//     Ò key.
		Ograve = 210L,
		//
		// Summary:
		//     Ó key.
		Oacute = 211L,
		//
		// Summary:
		//     Ô key.
		Ocircumflex = 212L,
		//
		// Summary:
		//     Õ key.
		Otilde = 213L,
		//
		// Summary:
		//     Ö key.
		Odiaeresis = 214L,
		//
		// Summary:
		//     × key.
		Multiply = 215L,
		//
		// Summary:
		//     Ø key.
		Ooblique = 216L,
		//
		// Summary:
		//     Ù key.
		Ugrave = 217L,
		//
		// Summary:
		//     Ú key.
		Uacute = 218L,
		//
		// Summary:
		//     Û key.
		Ucircumflex = 219L,
		//
		// Summary:
		//     Ü key.
		Udiaeresis = 220L,
		//
		// Summary:
		//     Ý key.
		Yacute = 221L,
		//
		// Summary:
		//     Þ key.
		Thorn = 222L,
		//
		// Summary:
		//     ß key.
		Ssharp = 223L,
		//
		// Summary:
		//     ÷ key.
		Division = 247L,
		//
		// Summary:
		//     ÿ key.
		Ydiaeresis = 0xFFL
	}
}
