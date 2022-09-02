using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Enums
{
	/// <summary>
	/// Relation with other user
	/// </summary>
    public enum UserRelation
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		/// <summary>
		/// Follower
		/// </summary>
		Follower,
		/// <summary>
		/// Friend
		/// </summary>
		Friend,
		/// <summary>
		/// Blocked
		/// </summary>
		Blocked
	}
}
