using System;
using System.Collections.Generic;
using System.IO;
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
        private List<Socket> clientList;
        private string publicDir = Path.GetDirectoryName(Application.ExecutablePath);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopServer(server);
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

            int serverPort = (int)numericUpDownServerPort.Value;
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(endPoint);
                server.Listen((int)SocketOptionName.MaxConnections);

                clientList = new List<Socket>();

                LogEvent($"Server started on port {serverPort}");

                active = true;
                Task.Run(() =>
                {
                    while (active)
                    {
                        try
                        {
                            Socket client = server.Accept();
                            LogEvent($"{client.RemoteEndPoint} is connected");

                            Task.Run(() =>
                            {
                                ProcessClient(client);
                                DisconnectClient(client);
                            });
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
                MessageBox.Show($"Ошибка запуска сервера!\n{ex.Message}", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                server = null;
                active = false;
                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
                numericUpDownServerPort.Enabled = true;
            }
        }

        private void ProcessClient(Socket client)
        {
            AddClient(client);

            byte[] buffer = new byte[ushort.MaxValue];
            int bytesRead = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            if (bytesRead == 0)
            {
                LogEvent($"Zero bytes received from {client.RemoteEndPoint}");
                return;
            }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] strings = msg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            LogEvent($"{client.RemoteEndPoint} sent: {strings[0]}");
            string[] request = strings[0].Split(new char[] { ' ' }, 3);
            string fileRequested = request[1];
            string fullFilePath = Path.Combine(publicDir, fileRequested.Remove(0, 1));
            if (!string.IsNullOrEmpty(fullFilePath) && !string.IsNullOrWhiteSpace(fullFilePath) &&
                File.Exists(fullFilePath))
            {
                SendData(client, File.ReadAllBytes(fullFilePath));
            }
            else
            {
                SendMessage(client, GenerateResponse(404, "Not found", "File not found"));
            }
        }

        private void SendMessage(Socket client, string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            client.Send(msgBytes);
        }

        private void SendData(Socket client, byte[] data)
        {
            string t = $"HTTP/1.1 200 OK\r\n" +
                "Access-Control-Allow-Origin: *\r\n";
                t += "Content-Type: text/plain; charset=UTF-8\r\n" +
                    $"Content-Length: {data.Length}\r\n\r\n";
            byte[] header = Encoding.UTF8.GetBytes(t);
            byte[] buffer = new byte[header.Length + data.Length];
            for (int i = 0; i < header.Length; ++i)
            {
                buffer[i] = header[i];
            }
            for (int i = 0; i < data.Length; ++i)
            {
                buffer[i + header.Length] = data[i];
            }
            client.Send(buffer);
        }

        private void DisconnectClient(Socket client, bool autoRemove = true)
        {
            LogEvent($"{client.RemoteEndPoint} is disconnected");
            if (autoRemove)
            {
                RemoveClient(client);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private void DisconnectAllClients()
        {
            if (clientList != null)
            {
                clientList.ForEach((client) =>
                {
                    DisconnectClient(client, false);
                });
                clientList.Clear();
            }
        }

        private void AddClient(Socket client)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { AddClient(client); });
            }
            else
            {
                clientList.Add(client);
            }
        }

        private void RemoveClient(Socket client)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { RemoveClient(client); });
            }
            else
            {
                clientList.Remove(client);
            }
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
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { LogEvent(eventText); });
            }
            else
            {
                string dateTime = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");
                listBoxLog.Items.Add($"{dateTime}> {eventText}");
                if (checkBoxAutoscroll.Checked)
                {
                    listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                }
            }
        }

        private void StopServer(Socket serverSocket)
        {
            if (serverSocket != null)
            {
                try
                {
                    DisconnectAllClients();
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

                LogEvent("Server stopped!");
            }

            btnStopServer.Enabled = false;
            numericUpDownServerPort.Enabled = true;
            btnStartServer.Enabled = true;
        }
    }
}
