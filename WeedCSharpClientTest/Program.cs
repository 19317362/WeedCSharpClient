﻿using System;
using System.Configuration;
using System.IO;
using static System.Console;

using WeedCSharpClient;
using WeedCSharpClient.Helper;

namespace WeedCSharpClientTest
{
    class Program
    {
        private static readonly WeedCSharpClientProxy WeedProxy = new WeedCSharpClientProxy();
        private static Random random = new Random();

        static void Main(string[] args)
        {
            WriteLine("start...");
            UploadTmpFiles();
            //UploadLocalFiles();
            WriteLine("finish...");
        }

        private static void UploadTmpFiles()
        {
            var upper = random.Next(100) + 1;
            for (var i = 0; i < upper; i++)
            {
                var buffer = new byte[random.Next(100) + 50];
                random.NextBytes(buffer);

                #region Upload
                var result = WeedProxy.Upload(buffer, "file" + i);
                WriteLine(result.url);
                #endregion

                #region Lookup
                var index = result.url.LastIndexOf('/') + 1;
                var fid = result.url.Substring(index);
                var vid = long.Parse(fid.Split(',')[0]);
                var location = WeedProxy.Lookup(vid);
                WriteLine($"http://{location}/{fid}");
                #endregion
            }

            #region Upload using HttpHelper
            var bytes = new byte[random.Next(100) + 50];
            random.NextBytes(bytes);
            var uri = new Uri(ConfigurationManager.AppSettings["WeedMasterUrl"]);
            var assignation = new WeedCSharpClientImpl(uri).Assign(new AssignParams());
            var uploadUrl = $"http://{assignation.Location.publicUrl}/{assignation.WeedFSFile.Fid}";
            var postResult = HttpHelper.MultipartPost(uploadUrl, bytes, "file", random.Next().ToString(), "application/octet-stream");
            WriteLine($"Upload using HttpHelper: {Environment.NewLine}{uploadUrl}{Environment.NewLine}{postResult}");
            #endregion

            #region Delete
            WeedProxy.Delete(uploadUrl);
            WriteLine(uploadUrl + " has been deleted.");
            #endregion
        }

        private static void UploadLocalFiles()
        {
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["FilePath"]);
            foreach (var file in files)
            {
                #region Upload
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name;
                var stream = fileInfo.OpenRead();
                //var buffer = StreamHelper.StreamToBytes(stream);
                var result = WeedProxy.Upload(stream, fileName);
                WriteLine(result.url);
                #endregion

                #region Lookup
                var index = result.url.LastIndexOf('/') + 1;
                var fid = result.url.Substring(index);
                var vid = long.Parse(fid.Split(',')[0]);
                var location = WeedProxy.Lookup(vid);
                WriteLine($"http://{location}/{fid}");
                #endregion
            }
        }
    }
}
