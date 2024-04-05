// See https://aka.ms/new-console-template for more information


using BelousovaViktoriyaClient;

CreateDirectory();
await SocketClient.StartClient();
return;

void CreateDirectory()
{
    const string directoryPath = "../client/data";
    var di = new DirectoryInfo(directoryPath);
    if (!di.Exists)
    {
        Directory.CreateDirectory(directoryPath);
    }
}