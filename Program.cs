using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModDownloader
{
    class Program
    {
        static async Task Main()
        {
            // Step 1: Ask for OAuth access token
            Console.Write("Enter your OAuth bearer access token: ");
            string accessToken = Console.ReadLine();

            // Step 2: Ask for Pavlov mods directory
            Console.Write("Enter the directory where you want to save the mods: ");
            string pavlovModsDirectory = Console.ReadLine();

            // Step 3: Get /me/subscribed and save the result in a variable
            string subscribedModsJson = GetJsonData("/me/subscribed", accessToken);

            // Step 4: Extract mods with game_id 3959
            JArray subscribedMods = ExtractModsByGameId(subscribedModsJson, 3959);

            // Step 5: Ask user to continue
            Console.Write("Do you want to continue? (Y/N): ");
            string continueOption = Console.ReadLine();

            if (continueOption?.Trim().ToLower() == "y")
            {
                // Step 6: Download and extract mods
                foreach (var mod in subscribedMods)
                {
                    string id = mod["id"].ToString();
                    string taint = null;

                    // get platform windows modfile_live
                    foreach (var platform in mod["platforms"])
                    {
                        if (platform["platform"]?.ToString() == "windows")
                        {
                            taint = platform["modfile_live"].ToString();
                            break;
                        }
                    }

                    string modFilesJson = GetJsonData($"/games/3959/mods/{id}/files/{taint}", accessToken);

                    JObject jsonData = JObject.Parse(modFilesJson);

                    string downloadUrl = jsonData["download"]["binary_url"].ToString();

                    Console.WriteLine($"Downloading mod: {mod["name"]}");

                    await DownloadAndExtractMod(downloadUrl, pavlovModsDirectory, id, taint);
                }
            }
            else
            {
                Console.WriteLine("Download canceled.");
            }
        }

        static string GetJsonData(string endpoint, string accessToken)
        {
            string baseUrl = "https://api.mod.io/v1";
            string url = $"{baseUrl}{endpoint}";

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Authorization", $"Bearer {accessToken}");
                return client.DownloadString(url);
            }
        }

        static JArray ExtractModsByGameId(string subscribedModsJson, int gameId)
        {
            JObject jsonData = JObject.Parse(subscribedModsJson);
            JArray mods = jsonData["data"] as JArray;
            JArray selectedMods = new JArray();

            foreach (var mod in mods)
            {
                int modGameId = mod["game_id"].Value<int>();
                if (modGameId == gameId)
                {
                    Console.WriteLine(mod["name"]);
                    selectedMods.Add(mod);
                }
            }

            return selectedMods;
        }

        static async Task DownloadAndExtractMod(string downloadUrl, string destinationDirectory, string modId, string taint)
        {
            if (Directory.Exists(Path.Combine(destinationDirectory, $"UGC{modId}")))
            {
                Console.WriteLine("Already exists, skipping!");
                return;
            }

            string tempZipFile = Path.GetTempFileName();
            string tempExtractDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (sender, e) =>
                {
                    Console.Write($"\rDownloading... {e.ProgressPercentage}%");
                };

                await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempZipFile);
            }

            Console.WriteLine();
            Console.WriteLine("Extracting mod...");

            ZipFile.ExtractToDirectory(tempZipFile, tempExtractDirectory);

            Directory.CreateDirectory(Path.Combine(destinationDirectory, $"UGC{modId}"));

            string modDestinationDirectory = Path.Combine(destinationDirectory, $"UGC{modId}", "Data");

            Directory.Move(tempExtractDirectory, modDestinationDirectory);

            File.WriteAllText(Path.Combine(destinationDirectory, $"UGC{modId}", "taint"), taint);

            Console.WriteLine("Mod downloaded and extracted successfully.");
        }
    }
}
