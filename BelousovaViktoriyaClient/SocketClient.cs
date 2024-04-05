using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BelousovaViktoriyaClient;

public class SocketClient
{
    public static async Task StartClient()
    {
        byte[] bytes = new byte[1024];
        bool exitRequested = false; // Флаг для проверки запроса на выход

        try
        {
            var host = Dns.GetHostEntry("localhost");
            var ipAddress = host.AddressList[0];
            var remoteEp = new IPEndPoint(ipAddress, 11000);

            while (!exitRequested)
            {
                try
                {
                    var sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    sender.Connect(remoteEp);


                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint);

                    Console.Write("Enter action (1 - get a file, 2 - create a file, 3 - delete a file): > ");
                    var keyChar = Console.ReadLine();

                    switch (keyChar)
                    {
                        case "1":
                            sender.Send(ReadFile());
                            break;
                        case "2":
                            sender.Send(CreateFileAndWriteInformation());

                            break;
                        case "3":
                            sender.Send(DeleteFile());
                            break;
                        case "exit":
                            sender.Send(Encoding.ASCII.GetBytes("")); 
                           Console.WriteLine("The request was sent."); 
                            exitRequested = true; //для завершения цикла
                            break;
                        default:
                            Console.WriteLine("Please enter valid number.");
                            break;
                    }

                    var bytesRec = sender.Receive(bytes);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));
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

    private static byte[] CreateFileAndWriteInformation()
    {
        Console.WriteLine();
        Console.Write("Enter filename: > ");
        var fileName = Console.ReadLine();
        Console.Write("Enter file content: >");
        var fileContent = Console.ReadLine();
        Console.WriteLine("The request was sent.");
        return $"2>{fileName} >{fileContent}".Select(p => (byte)p).ToArray();
    }

    private static byte[] DeleteFile()
    {
        Console.Write("Enter filename: > ");
        var fileName = Console.ReadLine();
        Console.WriteLine("The request was sent.");
        return $"3>{fileName}".Select(p => (byte)p).ToArray();
    }

    private static byte[] ReadFile()
    {
        Console.Write("Enter filename: > ");
        var fileName = Console.ReadLine();
        Console.WriteLine("The request was sent.");
        return $"1>{fileName}".Select(p => (byte)p).ToArray();
    }
}