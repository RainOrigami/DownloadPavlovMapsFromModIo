using System;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModDownloader
{
    class ProgressBar
    {
        private const int progressBarWidth = 50;
        private readonly long total;

        public ProgressBar(long total)
        {
            this.total = total;
        }

        public void Update(long progress)
        {
            double percentage = (double)progress / total;
            int completed = (int)(percentage * progressBarWidth);

            Console.Write("\r[" + new string('#', completed) + new string(' ', progressBarWidth - completed) + $"] {percentage:P}");
        }

        public void Finish()
        {
            Console.WriteLine();
        }
    }

    class Program
    {
        private const string settingsPath = "./settings.json";
        private const string pavlovModsDirectoryBasePath = "%localappdata%\\Pavlov\\Saved\\Mods";
        private const string modIoBaseUrl = "https://api.mod.io/v1";

        record Settings(string AccessToken, string PavlovModsDirectory);
        record Mod(string Id, string LatestVersion, string Name, bool Exists, bool Download);

        private Settings? settings = null;

        static async Task Main()
        {
            try
            {
                await new Program().execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static HttpClient createClient(string accessToken)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return client;
        }

        private static async Task<HttpResponseMessage> get(string endpoint, string accessToken)
        {
            HttpClient client = createClient(accessToken);
            return await client.GetAsync($"{modIoBaseUrl}{endpoint}");
        }

        private static async Task<string> getString(string endpoint, string accessToken)
        {
            HttpResponseMessage response = await get(endpoint, accessToken);
            return await response.Content.ReadAsStringAsync();
        }

        static async Task download(string url, string destination, string accessToken)
        {
            using (HttpClient client = createClient(accessToken))
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long receivedBytes = 0;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[4096];
                        int bytesRead;

                        var progressBar = new ProgressBar(totalBytes);

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            receivedBytes += bytesRead;
                            progressBar.Update(receivedBytes);
                        }

                        progressBar.Finish();
                    }
                }
            }
        }

        private async Task execute()
        {
            if (File.Exists(settingsPath))
            {
                this.settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));
            }

            if (this.settings == null)
            {
                string? accessToken = null;

                while (accessToken == null)
                {
                    Console.Write("Enter your Mod.io OAuth access token (mod.io/me/access): ");
                    accessToken = Console.ReadLine()!;

                    if (accessToken.Length < 500)
                    {
                        Console.WriteLine("Access token is very short. Please make sure that you generate an OAuth token and press the + button on the right and then copy the token. NOT the API key!");
                        accessToken = null;
                        continue;
                    }

                    HttpResponseMessage response = await get("/me", accessToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed to get user data from Mod.io. Make sure your token is correct and has read permissions.");
                        accessToken = null;
                        continue;
                    }
                }

                string pavlovModsDirectory = Environment.ExpandEnvironmentVariables(pavlovModsDirectoryBasePath);

                if (Directory.Exists(pavlovModsDirectory))
                {
                    Console.Write($"Pavlov VR mods directory found at {pavlovModsDirectory}. Do you want to use this directory? (Y/N): ");
                    string? pavlovModsDirectoryOption = Console.ReadLine();

                    if (pavlovModsDirectoryOption?.Trim().ToLower() != "y")
                    {
                        Console.Write("Enter the path to your Pavlov VR mods directory: ");
                        pavlovModsDirectory = Console.ReadLine()!;
                    }
                }
                else
                {
                    Console.Write("Could not find Pavlov VR mods directory.");
                    Console.Write("Enter the path to your Pavlov VR mods directory: ");
                    pavlovModsDirectory = Console.ReadLine()!;
                }

                this.settings = new(accessToken, pavlovModsDirectory);

                File.WriteAllText(settingsPath, JsonConvert.SerializeObject(this.settings));
            }

            string? subscribedModsJson = null;
            JArray? subscribedMods = null;

            try
            {
                subscribedModsJson = await getString("/me/subscribed", this.settings.AccessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Failed to get subscribed mods from Mod.io. Make sure your token is correct and has read permissions.");
                return;
            }

            try
            {
                subscribedMods = extractModsByGameId(subscribedModsJson, 3959);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Failed to extract Pavlov VR mods from subscribed mods. Make sure you are subscribed to Pavlov VR mods and that your token is correct.");
                return;
            }

            Console.WriteLine($"Found {subscribedMods.Count} Pavlov VR mods.");

            if (subscribedMods.Count == 0)
            {
                Console.WriteLine("No Pavlov VR mods found. Make sure you are subscribed to Pavlov VR mods.");
                return;
            }

            List<Mod> modsToDownload = new();

            foreach (var mod in subscribedMods)
            {
                Console.Write($"{mod["name"]} - ");

                string? latestVersion = null;

                foreach (var platform in mod["platforms"])
                {
                    if (platform["platform"]?.ToString() == "windows")
                    {
                        latestVersion = platform["modfile_live"].ToString();
                        break;
                    }
                }

                if (latestVersion == null)
                {
                    Console.WriteLine("Could not find a Windows version of this mod. Skipping.");
                    continue;
                }

                bool exists = Directory.Exists(Path.Combine(settings.PavlovModsDirectory, $"UGC{mod["id"]}"));
                bool download = true;

                if (exists)
                {
                    Console.Write("Exists");

                    string currentVersion = File.ReadAllText(Path.Combine(settings.PavlovModsDirectory, $"UGC{mod["id"]}", "taint"));

                    if (currentVersion == latestVersion)
                    {
                        Console.WriteLine(", up to date");
                        download = false;
                    }
                    else
                    {
                        Console.WriteLine(", update required");
                    }
                }
                else
                {
                    Console.WriteLine("New, download required");
                }

                modsToDownload.Add(new(mod["id"].ToString(), latestVersion, mod["name"].ToString(), exists, download));
            }

            Console.Write("Do you want to continue? (Y/N): ");
            string continueOption = Console.ReadLine();

            if (continueOption?.Trim().ToLower() != "y")
            {
                Console.WriteLine("Download canceled.");
                return;
            }

            foreach (Mod mod in modsToDownload)
            {
                string modFilesJson = await getString($"/games/3959/mods/{mod.Id}/files/{mod.LatestVersion}", settings.AccessToken);

                JObject jsonData = JObject.Parse(modFilesJson);

                string downloadUrl = jsonData["download"]["binary_url"].ToString();

                Console.WriteLine($"Downloading: {mod.Name}");

                await downloadAndExtractMod(downloadUrl, settings, mod);
            }
        }

        private static JArray extractModsByGameId(string subscribedModsJson, int gameId)
        {
            JObject jsonData = JObject.Parse(subscribedModsJson);
            JArray mods = jsonData["data"] as JArray;
            JArray selectedMods = new JArray();

            foreach (var mod in mods)
            {
                int modGameId = mod["game_id"].Value<int>();
                if (modGameId == gameId)
                {
                    selectedMods.Add(mod);
                }
            }

            return selectedMods;
        }

        private static async Task downloadAndExtractMod(string downloadUrl, Settings settings, Mod mod)
        {
            if (!mod.Download)
            {
                return;
            }

            string modDirectory = Path.Combine(settings.PavlovModsDirectory, $"UGC{mod.Id}");

            if (mod.Exists)
            {
                Directory.Delete(modDirectory, true);
            }

            string tempZipFile = Path.GetTempFileName();
            string tempExtractDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            await download(downloadUrl, tempZipFile, settings.AccessToken);

            Console.WriteLine("Extracting mod...");

            ZipFile.ExtractToDirectory(tempZipFile, tempExtractDirectory);

            Directory.CreateDirectory(modDirectory);

            string modDataDirectory = Path.Combine(modDirectory, "Data");

            Directory.Move(tempExtractDirectory, modDataDirectory);

            File.WriteAllText(Path.Combine(modDirectory, "taint"), mod.LatestVersion);

            Directory.Delete(tempExtractDirectory, true);
            File.Delete(tempZipFile);

            Console.WriteLine("Mod downloaded and extracted successfully.");
        }
    }
}
