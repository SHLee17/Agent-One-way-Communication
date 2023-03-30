using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    class Program
    {
        static Dictionary<int, TcpClient> clientsDict = new Dictionary<int, TcpClient>();
        static int clientIdCounter = 0;

        static async Task Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 7777);
            listener.Start();
            Console.WriteLine("Waiting for clients...");

            string previousFilePath = null;

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                clientIdCounter++;
                clientsDict.Add(clientIdCounter, client);
                Console.WriteLine($"Client {clientIdCounter} connected");
                // 클라이언트 핸들러 생성 및 시작
                ClientHandler clientHandler = new ClientHandler(clientIdCounter, client);
                clientHandler.Start();
            }
        }

        static void SendMessageToAllClients(string message)
        {
            foreach (KeyValuePair<int, TcpClient> pair in clientsDict)
            {
                TcpClient client = pair.Value;
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                client.GetStream().Write(sendBytes, 0, sendBytes.Length);
            }
        }

        class ClientHandler
        {
            private int clientId;
            private TcpClient client;

            public ClientHandler(int clientId, TcpClient client)
            {
                this.clientId = clientId;
                this.client = client;
            }

            public void Start()
            {
                Task.Run(() => Receive());
            }

            private async void Receive()
            {
                try
                {
                    while (true)
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRead = await client.GetStream().ReadAsync(bytes, 0, bytes.Length);
                        string message = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                        Console.WriteLine($"Received message from client {clientId}: {message}");

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    clientsDict.Remove(clientId);
                    Console.WriteLine($"Client {clientId} disconnected");
                }
            }
        }
    }
}

