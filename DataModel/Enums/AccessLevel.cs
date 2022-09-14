using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Enums
{
	[Flags]
	public enum AccessLevel
	{
		NoOne = 0,
		Public = 1,
		Friends = 2,
		FriendsOfFriends = 4,
		Group = 8,
		Followers = 16,
		FriendsFollowers = 32,


		//ShortHands
		Friends_FriendsOfFriends = Friends | FriendsOfFriends,
		Friends_FriendsOfFriends_Followers = Friends | FriendsOfFriends | Followers,
		Friends_Group = Friends | Group,
		Friends_FriendsOfFriends_Group = Friends | FriendsOfFriends | Group,
		KnownPeople = Friends | FriendsOfFriends | Followers | Group | FriendsFollowers,
	}
}
