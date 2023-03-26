using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RhubarbCloudClient.Model;

using RhuEngine.Linker;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class NetworkingSettingsObject : SettingsObject
	{
		[SettingsField("The preferred Connection Type")]
		[NeedsRebootAttribute]
		public ConnectionType PreferredConnectionType = ConnectionType.HolePunch;

		[SettingsField("For ConnectionType Direct you need to give your public ip and to open ports for the port range")]
		public string PublicIP = "0.0.0.0";

		[SettingsField("Starting port to be used to ConnectionType Direct")]
		public int StartPortRange = 25472;

		[SettingsField("End port to be used to ConnectionType Direct Should be a range with at least 10 ports so you can be in 10 sessions at once")]
		public int EndPortRange = 25572;

	}
}
