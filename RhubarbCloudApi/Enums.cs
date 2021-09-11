using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.OpenAPITools.Model
{
	public enum SessionsType
	{
		Education,
		Business,
		Casual,
		NSFW,
	}
	public enum AccessLevel
	{
		Private,
		Friends,
		FriendsOfFriends,
		Anyone
	}
}
