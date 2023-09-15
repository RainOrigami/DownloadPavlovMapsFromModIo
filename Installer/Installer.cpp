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
    // Path to Dotnet directory
    const char* dotnetdir = "C:/Program Files/dotnet";
    std::string installdir;
    std::string installedexedir;

    // Structure which would store the metadata
    struct stat sb;
 
    // Calls the function with path as argument
    // If the file/directory exists at the path returns 0
    // If block executes if path exists
    if (stat(dotnetdir, &sb) == 0)
    {
        clog <<"Dotnet framework installation found!";
        cout <<"Set install path for Pavlov Map Downloader"<<endl;
        cin >> installdir;
        cout <<"Attempting to install at "<<installdir<<endl;
        if(_mkdir("installdir") == -1)
        {
        cerr << " Error"<< endl;
        }
        else
        {
        cout << "File Path Created, downloading file";
        }
        string dwnld_URL = "https://github.com/RainOrigami/DownloadPavlovMapsFromModIo/releases/download/v6/DownloadPavlovMapsFromModIo.exe"; //binary download path, may need to use raw file link instead at some point
        URLDownloadToFile(NULL, dwnld_URL.c_str(), installdir.c_str(), 0, NULL);
        sleep_for(5s);
        installedexedir.append(installdir);
        installedexedir.append("DownloadPavlovMapsFromModIo.exe");
        ShellExecute(NULL, "open", installedexedir.c_str(), NULL, NULL, SW_SHOWDEFAULT);
        exit(0);
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