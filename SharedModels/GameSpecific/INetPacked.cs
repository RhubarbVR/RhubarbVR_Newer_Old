using System;
using System.Collections.Generic;
using System.Text;

using MessagePack;
using MessagePack.Formatters;

namespace SharedModels.GameSpecific
{

	[Union(0, typeof(DataPacked))]
	[Union(1, typeof(StreamDataPacked))]
	[Union(2, typeof(ConnectToAnotherUser))]
	public interface IRelayNetPacked
	{
	}
}
