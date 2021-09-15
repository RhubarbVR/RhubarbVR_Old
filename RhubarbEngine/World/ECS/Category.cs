using System;
using System.Collections.Generic;
using System.Linq;

namespace RhubarbEngine.World.ECS
{
    public class Category : Attribute
	{
		public readonly string[] Paths;

		public Category(params string[] paths)
		{
			var end = new List<string>();
			foreach (var item in paths)
			{
				var p = from e in item.Split('/', '\\')
						where !string.IsNullOrEmpty(e)
						select e;
				end.AddRange(p);
			}
			this.Paths = end.ToArray();
		}
	}
}
