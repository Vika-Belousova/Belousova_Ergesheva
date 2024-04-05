    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    namespace BelousovaViktoriyaApp;

    // Socket Listener acts as a server and listens to the incoming
    // messages on the specified port and protocol.
    public class SocketListener
    {
        private Socket handler;

        public async Task StartServer()
        {
            var host = Dns.GetHostEntry("localhost");
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
                    handler = listener.Accept();

                    string? data = null;
                    byte[]? bytes;

                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = await handler.ReceiveAsync(bytes);
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
                            //Console.WriteLine(dataArray[1]);
                            msg = await CreateFile(dataArray[1], dataArray[2]);
                            break;
                        case "3":
                            msg = RemoveFile(dataArray[1]);
                            break;
                        case "exit":
                            ShutdownConnection();
                            break;
                        default:
                            throw new ArgumentException("Please enter valid number.");
                    }

                    await handler.SendAsync(Encoding.ASCII.GetBytes(msg));

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ShutdownConnection()//закрытие соед. с клиентом
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static async Task<string> CreateFile(string fileName, string data)
        {
            var file = $"../server/data/{fileName}";
            var fi = new FileInfo(file);
            if (fi.Exists)
            {
                Console.WriteLine("403");

                return " The response says that creating the file was forbidden!";
            }
            else
            {
                var fs = File.Create(file);
                await fs.WriteAsync(data.Select(p => (byte)p).ToArray());
                await fs.DisposeAsync();
                Console.WriteLine("200");
                return "The response says that file was created!";
            }
        }

        private string RemoveFile(string fileName)
        {
            var file = $"../server/data/{fileName}";
            var fi = new FileInfo(file);
            if (fi.Exists)
            {
                File.Delete(file);
                return "The response says that the file was successfully deleted!";
            }

            return "The response says that the file was not found!";
        }

        private static async Task<string> ReadFile(string fileName)
        {
            var file = $"../server/data/{fileName}";
            var fi = new FileInfo(file);
            if (fi.Exists)
            {
                Console.WriteLine($"200 {await File.ReadAllTextAsync(file)}");
                return "The content of file  is:" + await File.ReadAllTextAsync(file);
            }
            Console.WriteLine("404");
            return "The response says that the file was not found!";
        }

    }