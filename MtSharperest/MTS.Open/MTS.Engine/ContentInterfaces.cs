using System;
using System.IO;

namespace MTS.Engine
{
	public interface IContentPipeline
	{
		/// <summary>
		/// Not a great name, but this analyzes the inputs and determines dependencies, and whether the content exists / can be built
		/// To simplify things, you can specify several dependencies with the same tag, and the existing one will get selected
		/// If there are multiple choices, it will be an error
		/// So for example, Texture could depend on mytexture.png and mytexture.tga 
		/// </summary>
		void Prepare(PipelineBakeContext context);

		/// <summary>
		/// Bakes the content from its raw ingredients.
		/// Note: it's strange that this is an instance method, when it should not refer to any instance members--but that's just how it is.
		/// Maybe this can be another interface? MakeBaker? I need to try that.
		/// </summary>
		bool Bake(PipelineBakeContext context);
	}

	/// <summary>
	/// Content types implementing this can be loaded from baked data.
	/// It is conceivable to have dynamic-only content which can't be loaded baked, but it seems unlikely
	/// </summary>
	public interface IBakedLoader
	{
		/// <summary>
		/// Loads the content from baked data
		/// </summary>
		bool LoadBaked(PipelineLoadBakedContext context);
	}


	public class ContentLoadContext
	{
		public ContentBase Content;
	}

	/// <summary>
	/// OLD RAMBLING NONSENSE NOTES
	/// A type that can handle Load and Unload signals (OvenContentLoader is the canonical example).
	/// Unload being here has turned out to be quite a nuisance, since it is just basically universally calling a universal unloader for the content type
	/// But this will give us the opportunity to have content that's been loaded in wildly different ways. 
	/// (we could also achieve this by having that content say: Unload() { if(isLoadedSpecially) unloadSpecially(); else unloadNormally(); }
	/// So... uh.... given that.... maybe I should take Unload out.
	/// Yeah, because I'm also thinking about making loading tightly correlated with construction. therefore this could actually just be a MARKER,
	/// not a functional interface
	/// ERRR. NOPE! This is how the content base 
	/// </summary>
	public interface IContentLoader
	{
		bool Load(ContentLoadContext context);
	}

}

/// <summary>
/// the types in here are used to collaborate between classes without exposing methods to casual users
/// Theyre all implemented explicitly
/// </summary>
namespace MTS.Engine.PrivateInterfaces
{
	public interface ITextureLoaders
	{
		void LoadSolidColor(int width, int height, uint color);
	}

	public interface IArtLoaders
	{
		void LoadParamsOwned(int width, int height, float umin, float vmin, float umax, float vmax, Texture texture);
	}
}


