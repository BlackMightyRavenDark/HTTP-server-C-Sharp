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
		private List<NativeSocket> clientList;
		private static readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>();
		private Configurator configurator;
		private static readonly Dictionary<int, string> httpStatuses = new Dictionary<int, string>()
		{
			{ 200, "OK" },
			{ 206, "Partial Content" },
			{ 400, "Client Error" },
			{ 404, "Not Found" },
			{ 416, "Range Not Satisfiable" }
		};
		private bool isClosed = false;
		private static readonly string selfDirPath = Path.GetDirectoryName(Application.ExecutablePath);
		private readonly string webUiPath = $"{selfDirPath}\\web_ui";

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
				json["autostartServer"] = configurator.AutostartServer;
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

				jt = json.Value<JToken>("autostartServer");
				if (jt != null)
				{
					configurator.AutostartServer = jt.Value<bool>();
				}
			};
			configurator.Loaded += (s) =>
			{
				numericUpDownServerPort.Value = configurator.ServerPort;
				checkBoxAutostart.Checked = configurator.AutostartServer;
				textBoxPublicDirectory.Text = configurator.PublicDirectory;

				if (configurator.AutostartServer && !string.IsNullOrEmpty(configurator.PublicDirectory) &&
					!string.IsNullOrWhiteSpace(configurator.PublicDirectory) && Directory.Exists(configurator.PublicDirectory))
				{
					StartServer();
				}
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

		private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
		{
			configurator.AutostartServer = checkBoxAutostart.Checked;
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

				clientList = new List<NativeSocket>();

				LogEvent($"Server started on port {configurator.ServerPort}");

				active = true;
				await Task.Run(() =>
				{
					while (active)
					{
						try
						{
							Socket socket = server.Accept();
							LogEvent($"{socket.RemoteEndPoint} is connected");
							NativeSocket client = new NativeSocket(socket);

							Task.Run(() =>
							{
								ProcessClient(client);
								if (!client.IsDisposed)
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
				string msg = $"Ошибка запуска сервера! {ex.Message}";
				LogEvent(msg);

				if (server != null)
				{
					server.Close();
					server = null;
				}
				active = false;

				MessageBox.Show(msg, "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				btnStartServer.Enabled = true;
				btnStopServer.Enabled = false;
				numericUpDownServerPort.Enabled = true;
				textBoxPublicDirectory.Enabled = true;
				btnBrowsePublicDirectory.Enabled = true;

				return;
			}

			server.Close();
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

		private void ProcessClient(NativeSocket client)
		{
			AddClient(client);

			byte[] buffer = new byte[ushort.MaxValue];
			try
			{
				int bytesRead = client.Handle.Receive(buffer, 0, buffer.Length, SocketFlags.None);
				if (bytesRead == 0)
				{
					LogEvent($"Zero bytes received from {client.Handle.RemoteEndPoint}");
					return;
				}

				string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				string[] strings = msg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				LogEvent($"{client.Handle.RemoteEndPoint} sent: {strings[0]}");

				WebHeaderCollection headers = ParseHeaders(strings);

				string[] request = strings[0].Split(new char[] { ' ' }, 3);
				string method = request[0];
				string endpoint = request[1];
				if (endpoint.StartsWith("/web_ui/"))
				{
					ProcessWebUi(client, method, endpoint.Substring(8), headers);
				}
				else if (endpoint.StartsWith("/@"))
				{
					ProcessFileRequest(client, method, endpoint.Substring(2), headers);
				}
				else if (endpoint.StartsWith("/api/browse"))
				{
					string path = endpoint.Length >= 12 && endpoint[11] == '?' ? endpoint.Substring(12) : null;
					ProcessApiBrowse(client, method, path);
				}
				else
				{
					AnswerClient(client, method, 400, null, "Wrong endpoint! Navigate to 'GET /web_ui/'");
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
			}
		}

		private void ProcessWebUi(NativeSocket client, string method, string requestedPath, WebHeaderCollection requestHeaders)
		{
			switch (method)
			{
				case "GET":
					{
						string decodedPath = string.IsNullOrEmpty(requestedPath) || string.IsNullOrWhiteSpace(requestedPath) ?
							"index.html" : HttpUtility.UrlDecode(requestedPath);
						string fullFilePath = Path.Combine(webUiPath, decodedPath);
						if (!File.Exists(fullFilePath))
						{
							AnswerClient(client, 404, "File not found");
							return;
						}
						SendFile(client, fullFilePath, requestHeaders);
					}
					break;

				case "HEAD":
					{
						string decodedPath = string.IsNullOrEmpty(requestedPath) || string.IsNullOrWhiteSpace(requestedPath) ?
							"index.html" : HttpUtility.UrlDecode(requestedPath);
						string fullFilePath = Path.Combine(webUiPath, decodedPath);
						if (!File.Exists(fullFilePath))
						{
							AnswerClient(client, 404, "File not found");
							return;
						}

						WebHeaderCollection answerHeaders = BuildHeaders(fullFilePath, requestHeaders);
						if (answerHeaders == null)
						{
							AnswerClient(client, 500, "Can't access file");
							return;
						}
						AnswerClient(client, 200, answerHeaders, null);
					}
					break;

				default:
					AnswerClient(client, method, 400, null, $"Wrong method: {method}");
					break;
			}
		}

		private void ProcessApiBrowse(NativeSocket client, string method, string requestedPath)
		{
			if (method != "GET")
			{
				AnswerClient(client, 400, $"Wrong method: {method}");
				return;
			}

			if (string.IsNullOrEmpty(requestedPath) || string.IsNullOrWhiteSpace(requestedPath))
			{
				AnswerClient(client, 400, "Path not specified");
				return;
			}

			string decodedPath = HttpUtility.UrlDecode(requestedPath);
			LogEvent($"{client.Handle.RemoteEndPoint} browsed to {decodedPath}");

			string path = Path.Combine(configurator.PublicDirectory, decodedPath.Remove(0, 1).Replace("/", "\\"));
			if (Directory.Exists(path))
			{
				JArray jArray = new JArray();

				string[] directories = Directory.GetDirectories(path);
				foreach (string directory in directories)
				{
					JObject jDir = new JObject()
					{
						["name"] = Path.GetFileName(directory),
						["type"] = "directory"
					};
					jArray.Add(jDir);
				}

				string[] files = Directory.GetFiles(path);
				foreach (string file in files)
				{
					JObject jFile = new JObject()
					{
						["name"] = Path.GetFileName(file),
						["type"] = "file"
					};
					jArray.Add(jFile);
				}

				byte[] data = Encoding.UTF8.GetBytes(jArray.ToString());

				WebHeaderCollection headers = GetDefaultHttpHeaders();
				headers.Remove("Accept-Ranges");
				headers["Content-Type"] = "application/json";
				headers["Content-Length"] = data.Length.ToString();
				headers["Last-Modified"] = DateTime.UtcNow.ToString("R");

				AnswerClient(client, 200, headers, null);
				client.Handle.Send(data);
			}
			else
			{
				AnswerClient(client, 404, "Directory is not found");
			}
		}

		private void ProcessFileRequest(NativeSocket client, string requestedMethod,
			string requestedFilePath, WebHeaderCollection headers)
		{
			if (requestedMethod != "GET" && requestedMethod != "HEAD")
			{
				AnswerClient(client, 400, $"Wrong method: {requestedMethod}");
				return;
			}

			if (string.IsNullOrEmpty(requestedFilePath) || string.IsNullOrWhiteSpace(requestedFilePath))
			{
				AnswerClient(client, 400, "No file specified");
				return;
			}

			string decodedFilePath = HttpUtility.UrlDecode(requestedFilePath);
			LogEvent($"{client.Handle.RemoteEndPoint} requested file {decodedFilePath}");
			
			if (decodedFilePath.StartsWith("/"))
			{
				decodedFilePath = decodedFilePath.Substring(1);
			}
 
			string fullFilePath = Path.Combine(configurator.PublicDirectory, decodedFilePath.Replace("/", "\\"));

			if (requestedMethod == "HEAD")
			{
				WebHeaderCollection answerHeaders = BuildHeaders(fullFilePath, headers);
				AnswerClient(client, requestedMethod, answerHeaders != null ? 200 : 500, answerHeaders, null);
			}
			else
			{
				SendFile(client, fullFilePath, headers);
			}
		}

		private void DisconnectClient(NativeSocket client, bool autoRemove = true)
		{
			if (!client.IsDisposed)
			{
				LogEvent($"{client.Handle.RemoteEndPoint} is disconnected");
				if (autoRemove)
				{
					RemoveClient(client);
				}
				client.Dispose();
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

		private void AddClient(NativeSocket client)
		{
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { AddClient(client); });
			}
			else
			{
				lock (client)
				{
					clientList.Add(client);
				}
			}
		}

		private void RemoveClient(NativeSocket client)
		{
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { RemoveClient(client); });
			}
			else
			{
				lock (client)
				{
					clientList.Remove(client);
				}
			}
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

		private static HttpStatus GetHttpStatus(int statusCode)
		{
			return httpStatuses.ContainsKey(statusCode) ?
				new HttpStatus(statusCode, httpStatuses[statusCode]) :
				new HttpStatus(500, "Internal Server Error");
		}

		private WebHeaderCollection BuildHeaders(string filePath, WebHeaderCollection requestHeaders)
		{
			if (requestHeaders != null)
			{
				ExtractRange(requestHeaders, out long byteFrom, out long byteTo, out bool isRanged);
				return BuildHeaders(filePath, isRanged, byteFrom, byteTo);
			}
			return BuildHeaders(filePath, false, 0L, 0L);
		}

		private WebHeaderCollection BuildHeaders(string filePath, bool isRanged, long rangeFrom, long rangeTo)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(filePath);
				long segmentSize = isRanged ? (rangeTo - rangeFrom + 1L) : fileInfo.Length;

				WebHeaderCollection headers = GetDefaultHttpHeaders();
				headers["Content-Type"] = GetContentType(Path.GetExtension(filePath)?.ToLower());
				headers["Content-Length"] = segmentSize.ToString();
				headers["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R");

				if (isRanged)
				{
					headers["Content-Range"] = $"bytes {FormatRangedRange(rangeFrom, rangeTo, fileInfo.Length)}";
				}

				return headers;
			}
			catch
			{
				return null;
			}
		}

		private static WebHeaderCollection GetDefaultHttpHeaders()
		{
			return new WebHeaderCollection()
			{
				{ "Date", DateTime.UtcNow.ToString("R") },
				{ "Access-Control-Allow-Origin", "*" },
				{ "Accept-Ranges", "bytes" }
			};
		}

		private static void AnswerClient(NativeSocket socket, string requestMethod, HttpStatus status,
			WebHeaderCollection headers, string message)
		{
			byte[] body = !string.IsNullOrEmpty(requestMethod) && requestMethod != "HEAD" && !string.IsNullOrEmpty(message) ? Encoding.UTF8.GetBytes(message) : null;
			long contentLength = body != null ? body.LongLength : -1L;
			if (headers == null) { headers = new WebHeaderCollection(); }
			if (body != null)
			{
				headers["Content-Length"] = contentLength.ToString();
			}
			else
			{
				if (requestMethod == "POST" || requestMethod == "PUT")
				{
					headers["Content-Length"] = "0";
				}
				else if (requestMethod != null && requestMethod != "HEAD")
				{
					headers.Remove("Content-Length");
				}
			}

			if (status.Code == 416)
			{
				headers["Content-Range"] = $"bytes */{(contentLength < 0L ? "*" : contentLength.ToString())}";
			}

			string answer = $"HTTP/1.1 {status.Code} {status.Message}\r\n";
			answer += headers != null && headers.Count > 0 ? $"{HttpHeadersToString(headers)}\r\n" : "\r\n";
			socket.Send(answer);

			if (body != null)
			{
				socket.Handle.Send(body);
			}
		}

		private static void AnswerClient(NativeSocket socket, string method, int statusCode,
			WebHeaderCollection headers, string message)
		{
			HttpStatus status = GetHttpStatus(statusCode);
			AnswerClient(socket, method, status, headers, message);
		}

		private static void AnswerClient(NativeSocket socket, int statusCode, WebHeaderCollection headers, string message)
		{
			HttpStatus status = GetHttpStatus(statusCode);
			AnswerClient(socket, null, status, headers, message);
		}

		private static void AnswerClient(NativeSocket socket, int statusCode, string message)
		{
			HttpStatus status = GetHttpStatus(statusCode);
			AnswerClient(socket, null, status, null, message);
		}

		private void SendStream(NativeSocket socket, Stream stream, long position, long segmentSize)
		{
			if (segmentSize <= 0L || stream.Length <= 0L || !socket.IsConnected) { return; }
			string clientAddress = socket.Handle.RemoteEndPoint.ToString();
			stream.Position = position;
			long remaining = segmentSize;
			byte[] buffer = new byte[4096];
			while (remaining > 0L && socket.IsConnected)
			{
				int toRead = remaining > buffer.Length ? buffer.Length : (int)remaining;
				int read = stream.Read(buffer, 0, toRead);
				if (read <= 0) { break; }
				remaining -= read;
				socket.Handle.Send(buffer, 0, read, SocketFlags.None, out SocketError socketError);
				if (socketError != SocketError.Success)
				{
					System.Diagnostics.Debug.WriteLine($"Socket {clientAddress} error: {socketError}");
					break;
				}
			}
		}

		private void SendStream(NativeSocket socket, Stream stream)
		{
			if (stream.Length > 0L && socket.IsConnected)
			{
				string clientAddress = socket.Handle.RemoteEndPoint.ToString();
				byte[] buffer = new byte[4096];
				while (socket.IsConnected)
				{
					int read = stream.Read(buffer, 0, buffer.Length);
					if (read <= 0) { break; }
					socket.Handle.Send(buffer, 0, read, SocketFlags.None, out SocketError socketError);
					if (socketError != SocketError.Success)
					{
						System.Diagnostics.Debug.WriteLine($"Socket {clientAddress} error: {socketError}");
						break;
					}
				}
			}
		}

		private void SendFile(NativeSocket socket, string filePath, long filePosition, long segmentLength)
		{
			try
			{
				using (Stream stream = File.OpenRead(filePath))
				{
					if (filePosition >= 0L && segmentLength >= 0L)
					{
						SendStream(socket, stream, filePosition, segmentLength);
					}
					else
					{
						SendStream(socket, stream);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void SendFile(NativeSocket client, string filePath, WebHeaderCollection headers)
		{
			if (File.Exists(filePath))
			{
				long fileSize = GetFileSize(filePath);
				long byteFrom;
				long byteTo;
				bool isRanged = HasKey(headers, "Range");
				if (isRanged)
				{
					if (ExtractRange(headers, out byteFrom, out byteTo, out _))
					{
						if (byteTo == -1L) { byteTo = fileSize == 0L ? 0L : fileSize - 1L; }
						if (byteFrom < 0L || byteFrom > byteTo || byteTo >= fileSize)
						{
							AnswerClient(client, 416, null);
							return;
						}
					}
					else
					{
						AnswerClient(client, 400, "Invalid range");
						return;
					}
				}
				else
				{
					byteFrom = 0L;
					byteTo = -1L;
				}

				WebHeaderCollection answerHeaders = BuildHeaders(filePath, isRanged, byteFrom, byteTo);
				if (answerHeaders == null)
				{
					AnswerClient(client, 500, "Can't access requested file");
					return;
				}

				AnswerClient(client, isRanged ? 206 : 200, answerHeaders, null);
				long segmentSize = byteTo < 0L ? -1L : (byteTo - byteFrom + 1L);
				SendFile(client, filePath, byteFrom, segmentSize);
			}
			else
			{
				AnswerClient(client, 404, "File not found");
			}
		}

		private static string HttpHeadersToString(WebHeaderCollection headers)
		{
			string s = string.Empty;
			for (int i = 0; i < headers.Count; ++i)
			{
				string headerName = headers.GetKey(i);
				if (!string.IsNullOrEmpty(headerName))
				{
					s += $"{headerName}: {headers.Get(i)}\r\n";
				}
			}
			return s;
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

		private static WebHeaderCollection ParseHeaders(string[] strings)
		{
			WebHeaderCollection headers = new WebHeaderCollection();
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

		private static bool ExtractRange(NameValueCollection headers, out long byteFrom, out long byteTo, out bool isRanged)
		{
			string rangeValue = headers.Get("Range");
			if (!string.IsNullOrEmpty(rangeValue))
			{
				isRanged = true;
				int n = rangeValue.IndexOf('=');
				if (n >= 0)
				{
					rangeValue = rangeValue.Substring(n + 1);
				}
				if (!string.IsNullOrEmpty(rangeValue) && !string.IsNullOrWhiteSpace(rangeValue))
				{
					string[] splitted = rangeValue.Split('-');
					if (splitted == null || splitted.Length != 2)
					{
						byteFrom = 0L;
						byteTo = -1L;
						return false;
					}

					if (!string.IsNullOrEmpty(splitted[0]) && !string.IsNullOrWhiteSpace(splitted[0]))
					{
						if (!long.TryParse(splitted[0], out byteFrom))
						{
							byteFrom = 0L;
							byteTo = -1L;
							return false;
						}
					}
					else
					{
						byteFrom = 0L;
					}

					if (!string.IsNullOrEmpty(splitted[1]) && !string.IsNullOrWhiteSpace(splitted[1]))
					{
						if (!long.TryParse(splitted[1], out byteTo))
						{
							byteFrom = 0L;
							byteTo = -1L;
							return false;
						}
					}
					else
					{
						byteTo = -1L;
					}

					return true;
				}
			}

			byteFrom = 0L;
			byteTo = -1L;
			isRanged = false;
			return false;
		}

		private static string FormatRangedRange(long rangeFrom, long rangeTo, long contentLength)
		{
			string from = rangeFrom < 0L ? string.Empty : rangeFrom.ToString();
			string to = rangeTo < 0L ? string.Empty : rangeTo.ToString();
			string length = contentLength < 0L ? "*" : contentLength.ToString();
			return $"{from}-{to}/{length}";
		}

		private string GetContentType(string ext)
		{
			return contentTypes != null && !string.IsNullOrEmpty(ext) && !string.IsNullOrWhiteSpace(ext) &&
				contentTypes.ContainsKey(ext) ? contentTypes[ext] :
				"application/octet-stream";
		}

		private static bool HasKey(NameValueCollection collection, string key)
		{
			if (collection != null)
			{
				for (int i = 0; i < collection.Count; ++i)
				{
					if (collection.GetKey(i) == key) { return true; }
				}
			}
			return false;
		}
	}
}
