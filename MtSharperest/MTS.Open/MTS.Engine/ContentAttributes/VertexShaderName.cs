using System;

namespace MTS.Engine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class VertexShaderNameAttribute : Attribute
	{
		public string Name;
		public VertexShaderNameAttribute(string name)
		{
			this.Name = name;
		}
	}
}


