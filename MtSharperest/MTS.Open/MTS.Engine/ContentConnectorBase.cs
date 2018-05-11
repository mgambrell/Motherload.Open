using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using MTS.Engine.ContentUtils;

namespace MTS.Engine
{
	public class RuntimeConnector_TextureContext
	{
		public ImageBuffer ImageBuffer;
		public BinaryReader Reader;
	}

	public class ContentConnectorContext_ShaderProgram
	{
		public BinaryReader Reader;
		public object Handle;
	}

	public class PipelineConnector_TextureBaking
	{
		public ImageBuffer Image;
		public BinaryWriter Writer;
	}

	public class PipelineConnector_TextureEvaluation
	{
		/// <summary>
		/// Input: information about the texture
		/// </summary>
		public ImageInfo Info;

		/// <summary>
		/// Set to true if the pipeline can handle this texture
		/// </summary>
		public bool Accepted { get; private set; }

		/// <summary>
		/// Accepts the texture
		/// </summary>
		public void Accept() { Accepted = true; }
	}

	public class PipelineConnectorBase
	{
		Dictionary<Type, IContentPipeline> pipelinesCache = new Dictionary<Type, IContentPipeline>();

		/// <summary>
		/// Gets a pipeline suitable for building the given content
		/// </summary>
		public virtual IContentPipeline GetPipeline(ContentBase content)
		{
			//OK, here's the deal
			//we don't want this assembly to have to have a reference to the pipelines assembly
			//so what we're going to do is establish a convention here
			//Pipelines for type EngineType will be implemented by MTS.Engine.Pipelines.EngineTypePipeline
			//we'll walk down the base type hierarchy to find EngineType (since sometimes the provided content will be derived from EngineType)
			//and we'll use reflection to try finding each thing
			//Now, this is meant for use on the Proto target.. nonetheless, to speed it up, we'll keep it cached

			var type = content.GetType();
			IContentPipeline ret;
			if (pipelinesCache.TryGetValue(type, out ret))
				return ret;

			var baseType = type;
			for (;;)
			{
				var candidateTypeName = "MTS.Engine.Pipelines." + baseType.Name + "Pipeline";
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					var pipelineType = assembly.GetType(candidateTypeName, false);
					if (pipelineType != null)
					{
						ret = (IContentPipeline)Activator.CreateInstance(pipelineType);
						pipelinesCache[type] = ret;
						break;
					}
				}

				baseType = baseType.BaseType;
				if (baseType == null) break;
			}

			return ret;
		}

		/// <summary>
		/// Before textures are baked, they come through here to give us a chance to check things and mutate them
		/// </summary>
		public virtual void EvaluateTexture(PipelineConnector_TextureEvaluation evaluation)
		{
		}

		/// <summary>
		/// Performs texture encoding. May call through to some reference implementations or base type stuff if the backend has no special requirements
		/// </summary>
		public virtual void BakeTexture(PipelineConnector_TextureBaking baking)
		{
		}
	}

	/// <summary>
	/// The bridge between abstract content engine and specific engine and platform concerns.
	/// For instance, loads textures from baked texture resources
	/// WARNING: some of this is PIPELINE-side and some of this is RUNTIME-side
	/// Maybe I should not have mixed those up.
	/// TODO: rename something like BackendInterface or BackendConnector?
	/// </summary>
	public class RuntimeConnectorBase
	{
		public RuntimeConnectorBase()
		{
			//try loading the pipelines assembly
			try
			{
				AppDomain.CurrentDomain.Load("MTS.Engine.Pipeline");
			}
			catch
			{
			}
		}

		public virtual void UnloadShader(ContentConnectorContext_ShaderProgram context) { }
		public virtual void LoadShader(ContentConnectorContext_ShaderProgram context) { }

		public virtual IntPtr LoadTexture(RuntimeConnector_TextureContext context)
		{
			return IntPtr.Zero;
		}
		public virtual void DestroyTexture(IntPtr handle)
		{
		}
	}

}


