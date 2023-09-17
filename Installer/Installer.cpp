#include <iostream>
#include <sys/stat.h>
#include <stdlib.h>
#include <string>
#include <Windows.h>
#include <direct.h>
#include <chrono>
#include <thread>
#pragma comment(lib, "urlmon.lib")
using namespace std;
using namespace std::this_thread;
using namespace std::chrono_literals;
using std::chrono::system_clock;
 
int main()
{
    system("title PavlovModioDownloaderInstaller");
    // Path to Dotnet directory
    const char* dotnetdir = "C:/Program Files/dotnet";
    // Structure which would store the metadata
    struct stat sb;
 
    // Calls the function with path as argument
    // If the file/directory exists at the path returns 0
    // If block executes if path exists
    if (stat(dotnetdir, &sb) == 0)
    {
        string installdir;
        cout <<"Welcome to the Pavlov Map Auto Downloader"<<endl<<"Please Set your desired install path for Pavlov Map Downloader"<<endl;
        cin >> installdir; //sets target install dir
        cout <<"Attempting to install at "<<installdir<<endl;
        //sets command line strings
        installdir.assign(installdir + "\PavlovModioDownloader");
        string createinstalldircmd = "md " + installdir; // link below this will need to be changed if you merge the PR
        string rawurl = " https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/master/Program/Compiled/DownloadPavlovMapsFromModIo.exe";
        string installdir_and_url = installdir + rawurl;
        string installdircmd = "curl -O --output-dir " + installdir_and_url;
        //commands
        system(createinstalldircmd.c_str());
        cout << "File Path Created, downloading file";
        system(installdircmd.c_str());
        string exedir = "start " + installdir + "DownloadPavlovMapsFromModIo.exe";
        rawurl.assign(" https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/master/Program/Compiled/version.txt");
        installdir_and_url.assign(installdir + rawurl);
        installdircmd.assign("curl -O --output-dir " + installdir_and_url);
        system(installdircmd.c_str()); //Line below downloads the version txt
        system("curl -O https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/main/Program/Compiled/version.txt");
        sleep_for(5s);
        system("start DownloadPavlovMapsFromModIo.exe"); //Boots the installed exe
        cout << "Download Successful!"<<endl;
        sleep_for(10s);
        exit(0); //exits program after delay
    }
    else
        {
            
            for (bool valid = false; valid = false;)
            {
                string installQuery;
                cout << "Dotnet 6 Framwork not found, would you like to install it?[Y/N]";
                cin >> installQuery;
                if (installQuery == "y")
                {
                    system("winget install Microsoft.DotNet.DesktopRuntime.6");
                    valid = true;
                }
                else if (installQuery == "Y")
                {
                    system("winget install Microsoft.DotNet.DesktopRuntime.6");
                    valid = true;
                }
                else if (installQuery == "n")
                {
                    cout << "Terminating program... I'll be back!";
                    sleep_for(5s);
                    exit(0);
                }
                else if (installQuery == "N")
                {
                    cout << "Terminating program... I'll be back!";
                    sleep_for(5s);
                    exit(0);
                }
                else
                {
                    cout << "Invalid Command!";
                }
            }
        }
        
 
    return 0;
}