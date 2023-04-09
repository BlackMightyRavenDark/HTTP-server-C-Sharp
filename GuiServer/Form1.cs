using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuiServer
{
    public partial class Form1 : Form
    {
        private Socket server = null;
        private bool active = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServer(server);
        }

        private void StartServer()
        {
            btnStartServer.Enabled = false;
            numericUpDownServerPort.Enabled = false;
            btnStopServer.Enabled = true;

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, (int)numericUpDownServerPort.Value);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(endPoint);
                server.Listen((int)SocketOptionName.MaxConnections);

                active = true;
                Task.Run(() =>
                {
                    while (active)
                    {
                        try
                        {
                            Socket client = server.Accept();
                            LogEvent($"{client.RemoteEndPoint} is connected");

                            ProcessClient(client);
                            DisconnectClient(client);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            active = false;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                server = null;
                active = false;
                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
                numericUpDownServerPort.Enabled = true;
            }
        }

        private void ProcessClient(Socket client)
        {
            byte[] buffer = new byte[ushort.MaxValue];
            int bytesRead = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            if (bytesRead == 0)
            {
                LogEvent($"Zero bytes received from {client.RemoteEndPoint}");
                return;
            }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            LogEvent($"{client.RemoteEndPoint} sent:\n<<<\n{msg}\n>>>");

            string[] strings = msg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] request = strings[0].Split(new char[] { ' ' }, 3);
            string answer = request[0] == "GET" ?
                GenerateResponse(200, "OK", "OK") :
                GenerateResponse(400, "Bad request", null);
            SendMessage(client, answer);
        }

        private void SendMessage(Socket client, string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            client.Send(msgBytes);
        }

        private void DisconnectClient(Socket client)
        {
            LogEvent($"{client.RemoteEndPoint} is disconnected");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static string GenerateResponse(int errorCode, string msg, string body)
        {
            string t = $"HTTP/1.1 {errorCode} {msg}\r\n" +
                "Access-Control-Allow-Origin: *\r\n";
            if (!string.IsNullOrEmpty(body))
            {
                byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                t += "Content-Type: text/plain; charset=UTF-8\r\n" +
                    $"Content-Length: {bodyBytes.Length}\r\n\r\n{body}";
            }
            else
            {
                t += "Content-Length: 0\r\n\r\n";
            }
            return t;
        }

        private void LogEvent(string eventText)
        {
            string dateTime = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");
            System.Diagnostics.Debug.WriteLine($"{dateTime}> {eventText}");
        }

        private void StopServer(Socket serverSocket)
        {
            if (serverSocket != null)
            {
                try
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                    serverSocket.Close();
                    serverSocket = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    if (ex is SocketException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Socket error {(ex as SocketException).ErrorCode}");
                    }
                    serverSocket.Close();
                    serverSocket = null;
                }
            }

            btnStopServer.Enabled = false;
            numericUpDownServerPort.Enabled = true;
            btnStartServer.Enabled = true;
        }
    }
}
