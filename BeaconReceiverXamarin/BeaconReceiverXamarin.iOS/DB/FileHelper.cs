using System;
using System.IO;
using Xamarin.Forms;
using BeaconReceiverXamarin.iOS.DB;
using BeaconReceiverXamarin.Data;

[assembly: Dependency(typeof(FileHelper))]
namespace BeaconReceiverXamarin.iOS.DB
{
	public class FileHelper : IFileHelper
	{
		public string GetLocalFilePath(string filename)
		{
			string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

			if (!Directory.Exists(libFolder))
			{
				Directory.CreateDirectory(libFolder);
			}

			return Path.Combine(libFolder, filename);
		}
	}
}
