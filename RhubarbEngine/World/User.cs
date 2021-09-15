using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Components.Users;

namespace RhubarbEngine.World
{
	public class User : Worker, IWorldObject
	{
		public User()
		{

		}
		public User(World _world, IWorldObject _parent) : base(_world, _parent)
		{

		}
		public User(IWorldObject _parent) : base(_parent.World, _parent)
		{

		}

		public User(IWorldObject _parent, bool newrefid = true) : base(_parent.World, _parent, newrefid)
		{

		}

		public SyncRef<UserRoot> userroot;

		public Sync<string> username;

		public Sync<string> normalizedusername;

		public Sync<string> profileUrl;

		public Sync<int> favoriteColor;

		public Sync<bool> verified;

		public Sync<int> identifier;

		public Sync<string> uuid;

		public Sync<DateTime> creationDate;

		public Sync<DateTime> banDate;

		public Sync<DateTime> tempbanDate;

		public Sync<bool> isAdmin;

		public Sync<bool> isAssistant;

		public Sync<bool> isLightMode;

		public SyncValueList<string> tags;

		public SyncAbstractObjList<UserStream> userStreams;

		public override void BuildSyncObjs(bool newRefIds)
		{
			userroot = new SyncRef<UserRoot>(this, newRefIds);
			username = new Sync<string>(this, newRefIds);
			normalizedusername = new Sync<string>(this, newRefIds);
			profileUrl = new Sync<string>(this, newRefIds);
			favoriteColor = new Sync<int>(this, newRefIds);
			verified = new Sync<bool>(this, newRefIds);
			identifier = new Sync<int>(this, newRefIds);
			uuid = new Sync<string>(this, newRefIds);
			creationDate = new Sync<DateTime>(this, newRefIds);
			banDate = new Sync<DateTime>(this, newRefIds);
			tempbanDate = new Sync<DateTime>(this, newRefIds);
			isAdmin = new Sync<bool>(this, newRefIds);
			isAssistant = new Sync<bool>(this, newRefIds);
			isLightMode = new Sync<bool>(this, newRefIds);
			tags = new SyncValueList<string>(this, newRefIds);
			userStreams = new SyncAbstractObjList<UserStream>(this, newRefIds);
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public T FindUserStream<T>(string name) where T : UserStream
		{
			foreach (var item in userStreams)
			{
				if (item.name.Value == name)
				{
					try
					{
						return (T)item;
					}
					catch
					{ }
				}
			}
			return null;
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public T FindOrCreateUserStream<T>(string name) where T : UserStream
		{
			var thing = FindUserStream<T>(name);
			if (thing == null)
			{
				var stream = (UserStream)Activator.CreateInstance(typeof(T));
				userStreams.Add(stream);
				stream.name.Value = name;
				return (T)stream;
			}
			else
			{
				return thing;
			}
		}
	}
}
