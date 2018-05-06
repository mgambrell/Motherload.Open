using System;

namespace MTS.Engine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class FragmentShaderNameAttribute : Attribute
	{
		public string Name;
		public FragmentShaderNameAttribute(string name)
		{
			this.Name = name;
		}
	}
}


