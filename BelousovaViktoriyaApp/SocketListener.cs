using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BelousovaViktoriyaApp;

// Socket Listener acts as a server and listens to the incoming
// messages on the specified port and protocol.
public class SocketListener
{
    private Socket handler;

    public async Task StartServer()
    {
        var host = await Dns.GetHostEntryAsync("localhost");
        var ipAddress = host.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen();
            Console.WriteLine("Server Started");
            while (true)
            {
                handler = await listener.AcceptAsync();


                string? data = null;

                while (true)
                {
                    var bytes = new byte[2046];
                    var bytesRec = await handler.ReceiveAsync(bytes, SocketFlags.None);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf(">", StringComparison.Ordinal) > -1)
                    {
                        break;
                    }
                }

                var dataArray = data.Split('>');
                var msg = "";
                switch (dataArray[0])
                {
                    case "1":
                        msg = await ReadFile(dataArray[1]);
                        break;
                    case "2":
                        msg = await CreateFile(dataArray[1], dataArray[2]);
                        break;
                    case "3":
                        msg = await RemoveFile(dataArray[1]);
                        break;
                    case "exit":
                        ShutdownConnection();
                        break;
                    default:
                        throw new ArgumentException("Please enter valid number.");
                }


                await handler.SendAsync(Encoding.ASCII.GetBytes(msg), SocketFlags.None);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void ShutdownConnection()
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }

    private static async Task<string> CreateFile(string fileName, string data)
    {
        const string path = $"../server/data/";
        var fi = new FileInfo(path + fileName);
        var number = 0;
        if (fi.Exists)
        {
            var result = await File.ReadAllTextAsync(path + "numberOfFiles.json");
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(result);
            var res = dictionary[fileName];
            return $"The response says that file is saved! ID = {res}";
        }
        else
        {
            var fs = File.Create(path + fileName);
            await fs.WriteAsync(data.Select(p => (byte)p).ToArray());
            await fs.DisposeAsync();
            var result = await File.ReadAllTextAsync(path + "numberOfFiles.json");
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(result);

            if (dictionary is { Count: 0 })
            {
                dictionary.Add(fileName, number = 1);
            }
            else
            {
                var lastObj = dictionary.Last();
                number = lastObj.Value;
                dictionary.Add(fileName, ++number);
            }

            var serialize = JsonSerializer.Serialize(dictionary);
            await File.WriteAllTextAsync(path + "numberOfFiles.json", serialize);
        }

        return $"The response says that file is saved! ID = {number}";
    }

    private static async Task<string> RemoveFile(string fileNameOrId)
    {
        var path = $"../server/data/";
        var fi = new FileInfo(path);
        if (!fi.Exists) return "The response says that the file was not found!";
        var text = await File.ReadAllTextAsync(path + "numberOfFiles.json");
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(text);
        if (int.TryParse(fileNameOrId, out var number))
        {
            if (dictionary != null && dictionary.ContainsValue(number))
            {
                var file = dictionary.FirstOrDefault(p => p.Value == number).Key;
                File.Delete(path + file);
            }
        }
        else
        {
            var file = dictionary.Remove(fileNameOrId);
            File.Delete(path + fileNameOrId);
        }

        return "The response says that the file was successfully deleted!";
    }

    private static async Task<string> ReadFile(string? fileNameOrId)
    {
        var path = $"../server/data/";
        var fileName = string.Empty;

        var text = await File.ReadAllTextAsync(path + "numberOfFiles.json");
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(text);
        if (int.TryParse(fileNameOrId, out var number))
        {
            if (dictionary != null && dictionary.ContainsValue(number))
            {
                var file = dictionary.FirstOrDefault(p => p.Value == number).Key;
                if (File.Exists(path + file))
                {
                    var fileContent = await File.ReadAllTextAsync(path + file);
                    fileName = file;
                    dictionary.Remove(file);
                    return $"{fileName}> {fileContent}";
                }
            }
        }
        else
        {
            var fileContent = await File.ReadAllTextAsync(path + fileNameOrId);
            dictionary.Remove(fileNameOrId);
            return $"{fileNameOrId}> {fileContent}";
        }

        return "The response says that the file was not found!";
    }
}