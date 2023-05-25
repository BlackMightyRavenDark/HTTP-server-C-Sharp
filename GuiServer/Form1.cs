using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GuiServer
{
    public partial class Form1 : Form
    {
        private Socket server = null;
        private bool active = false;
        private List<Socket> clientList;
        private Dictionary<string, string> contentTypes = new Dictionary<string, string>();
        private Configurator configurator;
        private bool isClosed = false;
        private static readonly string selfDirPath = Path.GetDirectoryName(Application.ExecutablePath);
        private readonly string webuiPath = $"{selfDirPath}\\webui";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string contentTypesFilePath = $"{selfDirPath}\\mime.txt";
            if (File.Exists(contentTypesFilePath))
            {
                LoadContentTypes(contentTypesFilePath);
            }

            configurator = new Configurator();
            configurator.Saving += (s, json) =>
            {
                json["serverPort"] = configurator.ServerPort;
                json["publicDirectory"] = configurator.PublicDirectory;
            };
            configurator.Loading += (s, json) =>
            {
                JToken jt = json.Value<JToken>("serverPort");
                if (jt != null)
                {
                    configurator.ServerPort = jt.Value<int>();
                }

                jt = json.Value<JToken>("publicDirectory");
                if (jt != null)
                {
                    configurator.PublicDirectory = jt.Value<string>();
                }
            };
            configurator.Loaded += (s) =>
            {
                numericUpDownServerPort.Value = configurator.ServerPort;
                textBoxPublicDirectory.Text = configurator.PublicDirectory;
            };

            configurator.Load();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            isClosed = true;
            StopServer(server);
            configurator.Save();
        }

        private void numericUpDownServerPort_ValueChanged(object sender, EventArgs e)
        {
            configurator.ServerPort = (int)numericUpDownServerPort.Value;
        }

        private void textBoxPublicDirectory_TextChanged(object sender, EventArgs e)
        {
            configurator.PublicDirectory = textBoxPublicDirectory.Text;
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServer(server);
        }

        private void btnBrowsePublicDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Выберите папку для общего доступа";
            if (!string.IsNullOrEmpty(configurator.PublicDirectory) &&
                !string.IsNullOrWhiteSpace(configurator.PublicDirectory))
            {
                fbd.SelectedPath = configurator.PublicDirectory;
            }
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                configurator.PublicDirectory = fbd.SelectedPath;
                textBoxPublicDirectory.Text = configurator.PublicDirectory;
            }

            fbd.Dispose();
        }

        private async void StartServer()
        {
            btnStartServer.Enabled = false;
            numericUpDownServerPort.Enabled = false;
            textBoxPublicDirectory.Enabled = false;
            btnBrowsePublicDirectory.Enabled = false;

            if (string.IsNullOrEmpty(configurator.PublicDirectory) ||
                string.IsNullOrWhiteSpace(configurator.PublicDirectory))
            {
                MessageBox.Show("Не указана папка для общего доступа!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartServer.Enabled = true;
                numericUpDownServerPort.Enabled = true;
                textBoxPublicDirectory.Enabled = true;
                btnBrowsePublicDirectory.Enabled = true;
                return;
            }

            if (!Directory.Exists(configurator.PublicDirectory))
            {
                MessageBox.Show("Папка для общего доступа не найдена!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartServer.Enabled = true;
                numericUpDownServerPort.Enabled = true;
                textBoxPublicDirectory.Enabled = true;
                btnBrowsePublicDirectory.Enabled = true;
                return;
            }

            btnStopServer.Enabled = true;

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, configurator.ServerPort);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(endPoint);
                server.Listen((int)SocketOptionName.MaxConnections);

                clientList = new List<Socket>();

                LogEvent($"Server started on port {configurator.ServerPort}");

                active = true;
                await Task.Run(() =>
                {
                    while (active)
                    {
                        try
                        {
                            Socket client = server.Accept();
                            LogEvent($"{client.RemoteEndPoint} is connected");

                            Task.Run(() =>
                            {
                                if (ProcessClient(client))
                                {
                                    DisconnectClient(client);
                                }
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
                return;
            }

            server.Dispose();
            server = null;

            if (!isClosed)
            {
                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
                numericUpDownServerPort.Enabled = true;
                textBoxPublicDirectory.Enabled = true;
                btnBrowsePublicDirectory.Enabled = true;

                LogEvent("Server stopped!");
            }
        }

        private bool ProcessClient(Socket client)
        {
            AddClient(client);

            byte[] buffer = new byte[ushort.MaxValue];
            try
            {
                int bytesRead = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                if (bytesRead == 0)
                {
                    LogEvent($"Zero bytes received from {client.RemoteEndPoint}");
                    return true;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] strings = msg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                LogEvent($"{client.RemoteEndPoint} sent: {strings[0]}");

                string[] request = strings[0].Split(new char[] { ' ' }, 3);
                if (request.Length == 3)
                {
                    NameValueCollection headers = ParseHeaders(strings);
                    if (request[0] == "GET")
                    {
                        if (request[1].StartsWith("/api/"))
                        {
                            ProcessApiRequest(client, request[1].Substring(4));
                            return true;
                        }
                        else if (request[1].StartsWith("/@"))
                        {
                            ProcessFileRequest(client, request[1].Substring(2), headers);
                            return true;
                        }

                        string fileRequested = request[1] == "/" ? "index.html" : request[1].Remove(0, 1);
                        string fullFilePath = Path.Combine(webuiPath, fileRequested);
                        if (File.Exists(fullFilePath))
                        {
                            ExtractRange(headers, out long byteFrom, out long byteTo);
                            return SendFileAsStream(client, fullFilePath, byteFrom, byteTo);
                        }
                        else
                        {
                            return SendMessage(client, GenerateResponse(404, "Not found", "File not found"));
                        }
                    }
                    else if (request[0] == "HEAD")
                    {
                        string decodedFileUrl = HttpUtility.UrlDecode(request[1]);
                        string fileRequested = decodedFileUrl == "/" ? "index.html" : decodedFileUrl.Remove(0, 1);
                        string fullFilePath;
                        if (fileRequested.StartsWith("@/"))
                        {
                            fileRequested = fileRequested.Remove(0, 2);
                            fullFilePath = Path.Combine(configurator.PublicDirectory, fileRequested.Replace("/", "\\"));
                        }
                        else
                        {
                            fullFilePath = Path.Combine(webuiPath, fileRequested.Replace("/", "\\"));
                        }

                        if (File.Exists(fullFilePath))
                        {
                            string headersString = BuildHeaders(fullFilePath);
                            return SendMessage(client, $"HTTP/1.1 200 OK\r\n{headersString}\r\n\r\n");
                        }
                        else
                        {
                            return SendMessage(client, GenerateResponse(404, "Not found", null));
                        }
                    }
                    else
                    {
                        return SendMessage(client, GenerateResponse(405, "Method not allowed", "Unsupported method"));
                    }
                }
                else
                {
                    return SendMessage(client, GenerateResponse(400, "Client error", "Invalid request"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (ex is SocketException)
                {
                    int socketErrorCode = (ex as SocketException).ErrorCode;
                    string t = $"Client read error! Socket error {socketErrorCode}";
                    System.Diagnostics.Debug.WriteLine(t);
                }
                return false;
            }
        }

        private bool SendMessage(Socket client, string msg)
        {
            try
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
                client.Send(msgBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private string BuildHeaders(string filePath)
        {
            string contentType = GetContentType(Path.GetExtension(filePath));
            long fileSize = GetFileSize(filePath);
            string t = $"Access-Control-Allow-Origin: *\r\nContent-Type: {contentType}\r\nContent-Length: {fileSize}";
            return t;
        }

        private long GetFileSize(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0L;
            }
        }

        private void ProcessApiRequest(Socket client, string requestedUrl)
        {
            if (requestedUrl.StartsWith("/browse?"))
            {
                ProcessApiBrowse(client, requestedUrl.Substring(8));
            }
        }

        private bool ProcessApiBrowse(Socket client, string requestedUrl)
        {
            string decodedUrl = HttpUtility.UrlDecode(requestedUrl);
            LogEvent($"{client.RemoteEndPoint} browsed to {decodedUrl}");

            string path = Path.Combine(configurator.PublicDirectory, decodedUrl.Remove(0, 1).Replace("/", "\\"));
            if (Directory.Exists(path))
            {
                JArray jArray = new JArray();

                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    JObject jDir = new JObject();
                    jDir["name"] = Path.GetFileName(directory);
                    jDir["type"] = "directory";
                    jArray.Add(jDir);
                }

                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    JObject jFile = new JObject();
                    jFile["name"] = Path.GetFileName(file);
                    jFile["type"] = "file";
                    jArray.Add(jFile);
                }

                return SendData(client, Encoding.UTF8.GetBytes(jArray.ToString()), ".json");
            }

            return true;
        }

        private bool ProcessFileRequest(Socket client, string requestedFilePath, NameValueCollection headers)
        {
            if (string.IsNullOrEmpty(requestedFilePath) || string.IsNullOrWhiteSpace(requestedFilePath))
            {
                return SendMessage(client, GenerateResponse(404, "Client error", "No file specified"));
            }

            string decodedFilePath = HttpUtility.UrlDecode(requestedFilePath);
            LogEvent($"{client.RemoteEndPoint} requested file {decodedFilePath}");
            
            if (decodedFilePath.StartsWith("/"))
            {
                decodedFilePath = decodedFilePath.Remove(0, 1);
            }
 
            string fullFilePath = Path.Combine(configurator.PublicDirectory, decodedFilePath.Replace("/", "\\"));
            if (File.Exists(fullFilePath))
            {
                ExtractRange(headers, out long byteFrom, out long byteTo);
                return SendFileAsStream(client, fullFilePath, byteFrom, byteTo);
            }
            else
            {
                return SendMessage(client, GenerateResponse(404, "Not found", "File not found"));
            }
        }

        private bool SendFileAsStream(Socket client, string filePath, long byteStart, long byteEnd)
        {
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Position = byteStart;

                    if (byteEnd == -1L || byteEnd < byteStart)
                    {
                        byteEnd = stream.Length == 0L ? 0 : stream.Length - 1L;
                    }
                    long segmentSize = byteStart == byteEnd ? 1L : byteEnd - byteStart + 1;
                    string fileExt = Path.GetExtension(filePath);
                    int errorCode = segmentSize == stream.Length ? 200 : 206;
                    string t = $"HTTP/1.1 {errorCode} OK\r\nContent-Type: {GetContentType(fileExt)}\r\n" +
                        $"Content-Length: {segmentSize}\r\naccept-range: bytes\r\n\r\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(t);
                    client.Send(buffer, SocketFlags.None);

                    long remaining = segmentSize;
                    buffer = new byte[4096];
                    while (remaining > 0L && client.Connected)
                    {
                        int bytesToRead = remaining > buffer.LongLength ? buffer.Length : (int)remaining;
                        int bytesRead = stream.Read(buffer, 0, bytesToRead);
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        remaining -= bytesRead;
                        client.Send(buffer, 0, bytesRead, SocketFlags.None);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (ex is SocketException)
                {
                    return false;
                }
            }

            return true;
        }

        private bool SendData(Socket client, byte[] data, string fileExtension)
        {
            string t = $"HTTP/1.1 200 OK\r\n" +
                "Access-Control-Allow-Origin: *\r\n";
            t += $"Content-Type: {GetContentType(fileExtension)}\r\n" +
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

            try
            {
                client.Send(buffer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private void DisconnectClient(Socket client, bool autoRemove = true)
        {
            try
            {
                LogEvent($"{client.RemoteEndPoint} is disconnected");
                client.Shutdown(SocketShutdown.Both);
                if (client.Connected)
                {
                    client.Disconnect(false);
                }
                if (autoRemove)
                {
                    clientList.Remove(client);
                }
                client.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
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
            if (!isClosed)
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate { LogEvent(eventText); });
                }
                else
                {
                    string dateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
                    listBoxLog.Items.Add($"{dateTime}> {eventText}");
                    if (checkBoxAutoscroll.Checked)
                    {
                        listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                    }
                }
            }
        }

        private void StopServer(Socket serverSocket)
        {
            if (serverSocket != null)
            {
                try
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                    serverSocket.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    if (ex is SocketException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Socket error {(ex as SocketException).ErrorCode}");
                    }
                    serverSocket.Close();
                }

                DisconnectAllClients();
            }

            if (!isClosed)
            {
                btnStopServer.Enabled = false;
            }
        }

        private void LoadContentTypes(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            string[] strings = fileContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string str in strings)
            {
                if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str) || str.StartsWith("#"))
                {
                    continue;
                }

                string[] keyValue = str.Split(new char[] { '|' }, 2);
                if (keyValue != null && keyValue.Length == 2)
                {
                    if (!string.IsNullOrEmpty(keyValue[0]) && !string.IsNullOrWhiteSpace(keyValue[0]))
                    {
                        string contentTypeValueTrimmed = keyValue[0].Trim();
                        string[] extensions = keyValue[1].ToLower().Split(',');
                        if (extensions != null && extensions.Length > 0)
                        {
                            foreach (string extension in extensions)
                            {
                                if (!string.IsNullOrEmpty(extension) && !string.IsNullOrWhiteSpace(extension))
                                {
                                    string extensionTrimmed = extension.Trim();
                                    if (!extensionTrimmed.Contains(" ") && extensionTrimmed.StartsWith("."))
                                    {
                                        contentTypes.Add(extensionTrimmed, contentTypeValueTrimmed);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static NameValueCollection ParseHeaders(string[] strings)
        {
            NameValueCollection headers = new NameValueCollection();
            for (int i = 1; i < strings.Length; ++i)
            {
                if (string.IsNullOrEmpty(strings[i]))
                {
                    break;
                }
                string[] splitted = strings[i].Split(new char[] { ':' }, 2);
                if (splitted.Length == 2)
                {
                    headers.Add(splitted[0].Trim(), splitted[1].Trim());
                }
            }
            return headers;
        }

        private static void ExtractRange(NameValueCollection headers, out long byteFrom, out long byteTo)
        {
            string t = headers.Get("Range");
            if (!string.IsNullOrEmpty(t))
            {
                int n = t.IndexOf('=');
                if (n >= 0)
                {
                    t = t.Substring(n + 1);
                }
                if (!string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t))
                {
                    string[] splitted = t.Split('-');
                    byteFrom = long.Parse(splitted[0]);
                    byteTo = string.IsNullOrEmpty(splitted[1]) || string.IsNullOrWhiteSpace(splitted[1]) ?
                        -1L : long.Parse(splitted[1]);
                    return;
                }
            }

            byteFrom = 0L;
            byteTo = -1L;
        }

        private string GetContentType(string ext)
        {
            return contentTypes != null && !string.IsNullOrEmpty(ext) && !string.IsNullOrWhiteSpace(ext) &&
                contentTypes.ContainsKey(ext) ? contentTypes[ext] :
                "text/plain; charset=UTF-8";
        }
    }
}
