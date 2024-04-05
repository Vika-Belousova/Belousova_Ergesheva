using BelousovaViktoriyaApp;

var result = new SocketListener();
CreateDirectory();
await result.StartServer();


void CreateDirectory()
{
    var directoryPath = "../server/data";
    var di = new DirectoryInfo(directoryPath);
    if (!di.Exists)
    {
        Directory.CreateDirectory(directoryPath);
    }
}