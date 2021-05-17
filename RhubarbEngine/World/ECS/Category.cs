using System;

namespace RhubarbEngine.World.ECS
{
	public class Category : Attribute
	{
		public readonly string[] Paths;

		public Category(params string[] paths)
		{
			this.Paths = paths;
		}
	}
}
