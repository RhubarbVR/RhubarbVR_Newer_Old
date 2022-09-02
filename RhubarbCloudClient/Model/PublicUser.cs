using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public class PublicUser
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string NormalizedUserName { get; set; }
		public string ProfileIcon { get; set; }
		public uint IconColor { get; set; }
		public DateTimeOffset CreationDate { get; set; }
		public PublicUser() {

		}
	}
}
