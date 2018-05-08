using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

//todo: could use reflection to use the same engine version the game was just built with?
//edit: I dont understand what that means now.
//update: I think it means, I would read the target assembly, find its engine reference assembly, then load the ContentManager from that
//this way, it's possible we could keep oven from having to get updated often or depend on the engine...
//or, is it bad if the engine versions mismatch? Maybe so.
//We might have to move more logic into ContentManager and make this a really dumb driver

public class Program
{
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern uint SetDllDirectory(string lpPathName);

	static string DllDirectory;

	static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		lock (AppDomain.CurrentDomain)
		{
			var asms = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var asm in asms)
				if (asm.FullName == args.Name)
					return asm;

			//load missing assemblies by trying to find them in the dll directory
			string dllname = new AssemblyName(args.Name).Name + ".dll";
			string fname = Path.Combine(DllDirectory, dllname);
			if (!File.Exists(fname)) return null;
			//it is important that we use LoadFile here and not load from a byte array; otherwise mixed (managed/unamanged) assemblies can't load
			return Assembly.LoadFile(fname);
		}
	}

	//oven.exe PathToGameExe Platform NameOfContentType ContentDir DataDir
	//NOTE: due to necessity of project config hookup, we can only operate on the main game exe right now
	static void Main(string[] args)
	{
		var exePath = Path.GetFullPath(args[0]);
		var platformName = args[1];
		var contentTypeName = args[2];
		var contentDir = args[3];
		var dataDir = args[4];

		//analyze arguments
		MTS.Engine.PlatformType platformType;
		if (!Enum.TryParse<MTS.Engine.PlatformType>(platformName, out platformType))
			throw new ArgumentException("Specified invalid platform");

		//find dependencies from whatever dlls the target assembly is using
		DllDirectory = Path.GetDirectoryName(exePath);
		SetDllDirectory(DllDirectory);
		AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

		//open target game
		var asm = Assembly.LoadFile(exePath);

		//look for the project config in there
		var contentManager = new MTS.Engine.ContentManager(true);
		contentManager.StartupOven(asm, platformType);

		//look for the target content in there
		var contentType = asm.GetType(contentTypeName, true);

		//mount that content....
		var content = contentManager.Mount(contentType, contentDir, dataDir);

		//this loads all the content, and it will be dumped out at the same time
		content.Load();
	}
}
