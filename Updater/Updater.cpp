#include <iostream>
#include <Windows.h>
#include<string>
#include<fstream>
#include <filesystem>
#include <algorithm>
using namespace std;
int main()
{
	// We don't need to specify a directory becuase the exe (should) be located at the same folder as the main app
    cout << "Checking for Update" << endl;
    system("curl  https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/main/Program/Compiled/version.txt -o readversion.txt");
    fstream readlatestversion;
    fstream readuserversion;
    string readvernum;
    string readuservernum;
    string uservernum = readuservernum;
    string latestver = readvernum;
    readuserversion.open("UserVersion.txt", ios::in); //open a file to perform read operation using file object
    if (readuserversion.is_open())//checking whether the file is open
        {

            while (getline(readuserversion, readuservernum)) //read data from file object, if we need extra functionalities we can put them here
            {
                uservernum.assign(readvernum);
                cout << "Successfully Read UserVersion.txt, Returned Version "<< uservernum << endl;
            }
            readuserversion.close(); //close the file object.
        }
    else
    {
        cerr << "Failed to Open UserVersion.txt!";
    }
        readlatestversion.open("readversion.txt", ios::in); //open a file to perform read operation using file object
    if (readlatestversion.is_open())//checking whether the file is open
    {
        
        while (getline(readlatestversion, readvernum)) //read data from file object and put it into string.
            { 
                latestver.assign(readvernum);
                cout << endl << "Latest version:" << latestver << endl;
                cout <<"Your Version:" << uservernum << endl;
            }
            readlatestversion.close(); //close the file object.
    }
    if (uservernum > latestver)
    {
        char FilePathWexe[MAX_PATH]; //Gets Filepath
        GetModuleFileNameA(NULL, FilePathWexe, MAX_PATH);
        string UpdaterFile = "Updater.exe";
        string FilePath = FilePathWexe;
        string::size_type substringtodelte = FilePath.find(UpdaterFile);
        FilePath.erase(substringtodelte, FilePath.length()); //Removes Extension
        cout <<"File Found at"<< FilePath<<endl;
        string updatecmdline;
        updatecmdline.append("curl -O --output-dir ");
        updatecmdline.append(FilePath);
        updatecmdline.append(" https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/master/Program/Compiled/DownloadPavlovMapsFromModIo.exe");
        system(updatecmdline.c_str());
        cout << "Sucessfully Updated!"<<endl;
        updatecmdline.assign("curl https://raw.githubusercontent.com/THW-Reaper/DownloadPavlovMapsFromModIoWithInstaller/main/Program/Compiled/version.txt -o UserVersion.txt");
        system(updatecmdline.c_str());
    }
    return(0);
}