using System;
using System.Threading.Tasks;
using HttpClientWithProgress.Models;

namespace HttpClientWithProgress.Example1
{
    public sealed class Program
    {
        //Some url to download... currently dotnet 3.1.8 runtime for windows x64
        private const string Url = "https://download.visualstudio.microsoft.com/download/pr/add2ffbe-a288-4d47-8b09-a39c8645f505/8516700dd5bd85fe07e8010e55d8f653/windowsdesktop-runtime-3.1.8-win-x64.exe";
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Let's start downloading!");

            var httpClient = new HttpClientWithProgress();

            var bytes = await httpClient.GetBytesAsync(Url, ProgressHandler).ConfigureAwait(false);
            
            //Do something with the bytes
            
            
            Console.WriteLine("Download finished!");
        }

        private static void ProgressHandler(HttpProgress progress)
        {
            Console.Write($"\r{progress.ToString("Download dotnet")}                                                                    ");
            
            if (progress.IsDone)
                Console.WriteLine();
        }
    }
}