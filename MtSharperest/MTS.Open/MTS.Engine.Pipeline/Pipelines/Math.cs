namespace MTS.Engine
{
	public static class PipelineMath
	{
		/// <summary>
		/// returns the next higher power of 2 than the provided value, for rounding up POW2 textures.
		/// </summary>
		public static int TextureUptoPow2(int k)
		{
			k--;
			for (int i = 1; i < 32; i <<= 1)
				k = k | k >> i;
			int candidate = k + 1;

			//use a sane minimum size, I guess.
			//I need to remove this restriction later with deeper study
			//various platforms dislike sizes under 16 for various formats
			//one platform in particular doesnt like widths under 64 for linear textures
			//BLECK. dont want this now
			//if (candidate < 64) candidate = 64;

			return candidate;
		}
	}
}