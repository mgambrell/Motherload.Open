using System;
using System.IO;
using System.Collections.Generic;

namespace MTS.Engine.Host
{
	/// <summary>
	/// A customized FileSystemWatcher which waits for batches of changes to complete before triggering.
	/// Since many DCC tools will to a bunch of assorted file IO operations when saving.
	/// </summary>
	public class SmartFileSystemWatcher : IDisposable
	{
		FileSystemWatcher fsw = new FileSystemWatcher();
		DateTime lastEvent;
		int batchMs;

		public SmartFileSystemWatcher(int batchMs, string directory)
		{
			fsw.Path = directory;
			fsw.IncludeSubdirectories = true;
			fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
			fsw.Changed += new FileSystemEventHandler(fsw_Changed);
			fsw.Renamed += new RenamedEventHandler(fsw_Changed);
			fsw.Created += new FileSystemEventHandler(fsw_Changed);
			fsw.Deleted += new FileSystemEventHandler(fsw_Changed);
			fsw.EnableRaisingEvents = true;
			lastEvent = DateTime.MaxValue;
			this.batchMs = batchMs;
		}

		public void Dispose()
		{
			fsw.EnableRaisingEvents = false;
			fsw.Dispose();
			fsw = null;
		}

		List<string> paths = new List<string>();

		public void GetPathsInto(List<string> target)
		{
			if((DateTime.Now - lastEvent).Milliseconds < batchMs)
			lock (paths)
			{
				target.AddRange(paths);
				paths.Clear();
			}
		}

		void fsw_Changed(object sender, FileSystemEventArgs e)
		{
			lock (paths)
			{
				lastEvent = DateTime.Now;
				var fp = e.FullPath;
				if(!paths.Contains(fp))
					paths.Add(fp);
			}
		}
	}

}