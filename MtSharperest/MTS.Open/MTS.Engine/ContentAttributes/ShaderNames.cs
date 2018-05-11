using System;

namespace MTS.Engine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ShaderNamesAttribute : Attribute
	{
		public string VsName;
		public string FsName;
		public ShaderNamesAttribute(string vsName, string fsName)
		{
			this.VsName = vsName;
			this.FsName = fsName;
		}
	}
}


