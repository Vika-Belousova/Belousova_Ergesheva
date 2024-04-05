using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BelousovaViktoriyaClient;

internal static class SocketClient
{
    public static async Task StartClient()
    {
        var bytes = new byte[1024];

        try
        {
            var host = await Dns.GetHostEntryAsync("localhost");
            var ipAddress = host.AddressList[0];
            var remoteEp = new IPEndPoint(ipAddress, 11000);

            while (true)
            {
                try
                {
                    var sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEp);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint);


                    Console.Write(
                        "Enter action (1 - get a file, 2 - save a file, 3 - delete a file, 4 - get all local files): > ");
                    var keyChar = Console.ReadLine();

                    switch (keyChar)
                    {
                        case "1":
                            await sender.SendAsync(ReadFile(), SocketFlags.None);
                            var bytesRec = await sender.ReceiveAsync(bytes, SocketFlags.None);
                            var res = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                            var newFile = res.Split('>');
                            await File.WriteAllTextAsync("../client/data/" + newFile[0], newFile[1]);
                            Console.WriteLine($"The file was downloaded! Specify a name for it: > {newFile[0]}");
                            Console.WriteLine($"File saved on the hard drive!");
                            break;
                        case "2":
                            await sender.SendAsync(new ArraySegment<byte>(await CreateFileAndWriteInformation()),
                                SocketFlags.None);
                            var bytesRec1 = await sender.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                            var res1 = Encoding.ASCII.GetString(bytes, 0, bytesRec1);
                            var newFile1 = res1.Split('>');
                            Console.WriteLine($"The file was downloaded! Specify a name for it: > {newFile1[0]}");
                            break;
                        case "3":
                            await sender.SendAsync(DeleteFile(), SocketFlags.None);
                            Console.WriteLine($"The response says that this file was deleted successfully!");
                            break;
                        case "4":
                            GetAllLocalFiles();
                            continue;
                        case "exit":
                            sender.Send("> exit".Select(p => (byte)p).ToArray(), SocketFlags.None);
                            break;
                        default:
                            throw new ArgumentException("Please enter valid number.");
                    }


                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }

                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void GetAllLocalFiles()
    {
        const string path = "../client/data/";
        var directories = Directory.GetFiles(path).ToList();
        foreach (var splitFileName in directories.Select(fileName => fileName.Split('/'))
                     .Where(splitFileName => !splitFileName[3].Contains("metadata")))
        {
            Console.WriteLine(splitFileName[3]);
        }
    }

    private static async Task<byte[]> CreateFileAndWriteInformation()
    {
        const string path = "../client/data/";
        Console.WriteLine();
        Console.Write("Enter name of the file: > ");
        var fileName = Console.ReadLine();
        Console.Write("Enter name of the file to be saved on server: >");
        var fileNameServer = Console.ReadLine();
        if (!File.Exists(path + fileName))
        {
            Console.WriteLine("Please try again. This file was not found");
        }

        var res = await File.ReadAllTextAsync(path + fileName);
        Console.WriteLine("The request was sent.");
        File.Delete(path + fileName);
        return string.IsNullOrEmpty(fileNameServer)
            ? $"2>{fileName} >{res}".Select(p => (byte)p).ToArray()
            : $"2>{fileNameServer} >{res}".Select(p => (byte)p).ToArray();
    }

    private static byte[] DeleteFile()
    {
        Console.Write("Do you want to delete the file by name or id (1 - name, 2 - id): > ");
        var findFileByIdOrName = Console.ReadLine();
        switch (findFileByIdOrName)
        {
            case "1":
                Console.WriteLine("Enter name: > ");
                break;
            case "2":
                Console.WriteLine("Enter id: > ");
                break;
        }

        var fileId = Console.ReadLine();
        Console.WriteLine("The request was sent.");
        return $"1>{fileId}".Select(p => (byte)p).ToArray();
    }

    private static byte[] ReadFile()
    {
        Console.Write("Do you want to get the file by name or id (1 - name, 2 - id): > ");
        var findFileByIdOrName = Console.ReadLine();
        switch (findFileByIdOrName)
        {
            case "1":
                Console.WriteLine("Enter name: > ");
                break;
            case "2":
                Console.WriteLine("Enter id: > ");
                break;
        }

        var fileId = Console.ReadLine();
        Console.WriteLine("The request was sent.");
        return $"1>{fileId}".Select(p => (byte)p).ToArray();
    }
}