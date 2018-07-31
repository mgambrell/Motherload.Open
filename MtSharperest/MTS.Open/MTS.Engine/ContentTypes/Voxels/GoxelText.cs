using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Xna.Framework;

namespace MTS.Engine.ContentTypes.Voxels
{
	public unsafe class GoxelText : ContentBase, IBakedLoader
	{
		public struct Voxel
		{
			public float color; //packed, intended for unpacking
			public short x, y, z, pad; //pad required to keep the color float aligned
		}

		public Voxel[] Voxels;

		protected override void ContentUnload()
		{
			Voxels = new Voxel[0];
		}

		// I guess you can put a compiled vertex buffer here, but we can't unload it
		// Anyway, I wish I could specify this as a processor to create a vertex buffer and manage that here
		// maybe one day
		// Maybe test it by making a Mesh content type and testing it by scanning for 2 different types..
		// to exercise the distinction between content processors and content types
		// (maybe the its that pipeline that would know how to decide which processor to use)
		public object User;

		unsafe bool IBakedLoader.LoadBaked(PipelineLoadBakedContext context)
		{
			var br = context.BakedReader;

			int voxelCount = br.ReadInt32();
			var bytes = br.ReadBytes(voxelCount * Marshal.SizeOf<Voxel>());

			//TODO - utilities to read to pointers (maybe brute hacks)
			Voxels = new Voxel[voxelCount];
			fixed (Voxel* v = Voxels)
			{
				Marshal.Copy(bytes, 0, new IntPtr(v), bytes.Length);
			}

			return true;
		}
	}
}

