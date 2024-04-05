using BelousovaViktoriyaApp;

var result = new SocketListener();
await CreateDirectoryAndDefaultFiles();
await result.StartServer();
return;


async Task CreateDirectoryAndDefaultFiles()
{
    const string directoryPath = "../server/data";
    var di = new DirectoryInfo(directoryPath);
    if (!di.Exists)
    {
        Directory.CreateDirectory(directoryPath);
        var fileStream = File.Create(directoryPath + "/numberOfFiles.json");
        await fileStream.WriteAsync("{}".Select(p => (byte)p).ToArray());
        await fileStream.DisposeAsync();
    }
}