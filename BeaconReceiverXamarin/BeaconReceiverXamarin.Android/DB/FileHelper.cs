using System;
using System.IO;
using BeaconReceiverXamarin.Data;
using BeaconReceiverXamarin.Droid.DB;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileHelper))]
namespace BeaconReceiverXamarin.Droid.DB
{
    public class FileHelper : IFileHelper
    {
        public string GetLocalFilePath(string filename)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return Path.Combine(path, filename);
        }
    }
}