﻿using System;
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

				string[] request = strings[0].Split(new char[] { ' ' }, 3);
				if (request.Length == 3)
				{
					NameValueCollection headers = ParseHeaders(strings);
					if (request[0] == "GET")
					{
						if (request[1].StartsWith("/api/"))
						{
							ProcessApiRequest(client, request[1].Substring(4));
							return;
						}
						else if (request[1].StartsWith("/@"))
						{
							ProcessFileRequest(client, request[1].Substring(2), headers);
							return;
						}

						string fileRequested = request[1] == "/" ? "index.html" : request[1].Remove(0, 1);
						string fullFilePath = Path.Combine(webuiPath, fileRequested);
						if (File.Exists(fullFilePath))
						{
							long byteFrom;
							long byteTo;
							bool isRanged = HasKey(headers, "Range");
							if (isRanged)
							{
								if (!ExtractRange(headers, out byteFrom, out byteTo))
								{
									SendMessage(client, GenerateResponse(400, "Client error", "Invalid range"));
									return;
								}
							}
							else
							{
								byteFrom = 0L;
								byteTo = -1L;
							}

							SendFileAsStream(client, fullFilePath, byteFrom, byteTo, isRanged);
						}
						else
						{
							SendMessage(client, GenerateResponse(404, "Not found", "File not found"));
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
							long fileSize = GetFileSize(fullFilePath);
							bool isRanged = HasKey(headers, "Range");
							if (isRanged)
							{
								if (ExtractRange(headers, out long byteFrom, out long byteTo))
								{
									if (byteTo == -1L) { byteTo = fileSize == 0L ? 0L : fileSize - 1L; }
									if (byteFrom >= 0L && byteFrom <= byteTo && byteTo < fileSize)
									{
										string headersString = BuildHeaders(fullFilePath, isRanged, byteFrom, byteTo);
										SendMessage(client, $"HTTP/1.1 206 Partial content\r\n{headersString}\r\n\r\n");
									}
									else
									{
										SendMessage(client, GenerateResponse(416, "Range not satisfiable", fileSize));
									}
								}
								else
								{
									SendMessage(client, GenerateResponse(400, "Invalid range", fileSize));
								}
							}
							else
							{
								string headersString = BuildHeaders(fullFilePath);
								SendMessage(client, $"HTTP/1.1 200 OK\r\n{headersString}\r\n\r\n");
							}
						}
						else
						{
							SendMessage(client, GenerateResponse(404, "Not found", null));
						}
					}
					else
					{
						SendMessage(client, GenerateResponse(405, "Method not allowed", "Unsupported method"));
					}
				}
				else
				{
					SendMessage(client, GenerateResponse(400, "Client error", "Invalid request"));
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

		private void SendMessage(NativeSocket client, string msg)
		{
			byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
			client.Handle.Send(msgBytes);
		}

		private string BuildHeaders(string filePath, bool isRanged, long rangeFrom, long rangeTo)
		{
			string contentType = GetContentType(Path.GetExtension(filePath)?.ToLower());
			long fileSize = GetFileSize(filePath);
			string t = $"Access-Control-Allow-Origin: *\r\nAccept-Ranges: bytes\r\n" +
				$"Content-Type: {contentType}\r\n";
			if (isRanged)
			{
				t += $"Content-Length: {rangeTo - rangeFrom + 1L}\r\n" +
					$"Content-Range: bytes {rangeFrom}-{rangeTo}/{fileSize}";
			}
			else
			{
				t += $"Content-Length: {fileSize}";
			}
			return t;
		}

		private string BuildHeaders(string filePath)
		{
			return BuildHeaders(filePath, false, 0L, 0L);
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

		private void ProcessApiRequest(NativeSocket client, string requestedUrl)
		{
			if (requestedUrl.StartsWith("/browse?"))
			{
				ProcessApiBrowse(client, requestedUrl.Substring(8));
			}
		}

		private void ProcessApiBrowse(NativeSocket client, string requestedUrl)
		{
			string decodedUrl = HttpUtility.UrlDecode(requestedUrl);
			LogEvent($"{client.Handle.RemoteEndPoint} browsed to {decodedUrl}");

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

				SendData(client, Encoding.UTF8.GetBytes(jArray.ToString()), ".json");
			}
		}

		private void ProcessFileRequest(NativeSocket client, string requestedFilePath, NameValueCollection headers)
		{
			if (string.IsNullOrEmpty(requestedFilePath) || string.IsNullOrWhiteSpace(requestedFilePath))
			{
				SendMessage(client, GenerateResponse(404, "Client error", "No file specified"));
				return;
			}

			string decodedFilePath = HttpUtility.UrlDecode(requestedFilePath);
			LogEvent($"{client.Handle.RemoteEndPoint} requested file {decodedFilePath}");
			
			if (decodedFilePath.StartsWith("/"))
			{
				decodedFilePath = decodedFilePath.Remove(0, 1);
			}
 
			string fullFilePath = Path.Combine(configurator.PublicDirectory, decodedFilePath.Replace("/", "\\"));
			if (File.Exists(fullFilePath))
			{
				long byteFrom;
				long byteTo;
				bool isRanged = HasKey(headers, "Range");
				if (isRanged)
				{
					long fileSize = GetFileSize(fullFilePath);
					if (ExtractRange(headers, out byteFrom, out byteTo))
					{
						if (byteTo == -1L) { byteTo = fileSize == 0L ? 0L : fileSize - 1L; }
						if (byteFrom < 0L || byteFrom > byteTo || byteTo >= fileSize)
						{
							SendMessage(client, GenerateResponse(416, "Range not satisfiable", fileSize));
							return;
						}
					}
					else
					{
						SendMessage(client, GenerateResponse(400, "Client error", "Invalid range", fileSize));
						return;
					}
				}
				else
				{
					byteFrom = 0L;
					byteTo = -1L;
				}

				SendFileAsStream(client, fullFilePath, byteFrom, byteTo, isRanged);
			}
			else
			{
				SendMessage(client, GenerateResponse(404, "Not found", "File not found"));
			}
		}

		private void SendFileAsStream(NativeSocket client, string filePath, long byteStart, long byteEnd, bool isRanged)
		{
			try
			{
				using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					if (byteEnd == -1L)
					{
						byteEnd = stream.Length == 0L ? 0L : stream.Length - 1L;
					}

					if (byteStart > byteEnd || byteStart >= stream.Length || byteEnd >= stream.Length)
					{
						SendMessage(client, GenerateResponse(416, "Range not satisfiable", stream.Length));
						return;
					}

					long segmentSize = byteStart == byteEnd ? 1L : byteEnd - byteStart + 1L;
					string fileExt = Path.GetExtension(filePath)?.ToLower();
					int errorCode = isRanged ? 206 : 200;
					string responseCodeText = isRanged ? "206 Partial Content" : "200 OK";
					string t = $"HTTP/1.1 {responseCodeText}\r\nAccess-Control-Allow-Origin: *\r\nAccept-Ranges: bytes\r\n" +
						$"Content-Type: {GetContentType(fileExt)}\r\n" +
						$"Content-Length: {segmentSize}";
					if (isRanged)
					{
						t += $"\r\nContent-Range: bytes {byteStart}-{byteEnd}/{stream.Length}";
					}
					t += "\r\n\r\n";

					byte[] buffer = Encoding.UTF8.GetBytes(t);
					client.Handle.Send(buffer, SocketFlags.None);

					stream.Position = byteStart;
					long remaining = segmentSize;
					buffer = new byte[4096];
					while (remaining > 0L && !client.IsDisposed && client.Handle.Connected)
					{
						int bytesToRead = remaining > buffer.LongLength ? buffer.Length : (int)remaining;
						int bytesRead = stream.Read(buffer, 0, bytesToRead);
						if (bytesRead <= 0)
						{
							break;
						}

						client.Handle.Send(buffer, 0, bytesRead, SocketFlags.None, out SocketError socketError);
						if (socketError != SocketError.Success)
						{
							System.Diagnostics.Debug.WriteLine($"File transferring error: {socketError}!");
							break;
						}

						remaining -= bytesRead;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void SendData(NativeSocket client, byte[] data, string fileExtension)
		{
			string t = $"HTTP/1.1 200 OK\r\n" +
				"Access-Control-Allow-Origin: *\r\n";
			t += $"Content-Type: {GetContentType(fileExtension?.ToLower())}\r\n" +
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
				client.Handle.Send(buffer);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
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

		private static string GenerateResponse(int errorCode, string msg, string body, long contentLength = 0L)
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
				t += $"Content-Length: {contentLength}\r\n\r\n";
			}
			return t;
		}

		private static string GenerateResponse(int errorCode, string msg, long contentLength)
		{
			return GenerateResponse(errorCode, msg, null, contentLength);
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

		private static bool ExtractRange(NameValueCollection headers, out long byteFrom, out long byteTo)
		{
			string rangeValue = headers.Get("Range");
			if (!string.IsNullOrEmpty(rangeValue))
			{
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
			return false;
		}

		private string GetContentType(string ext)
		{
			return contentTypes != null && !string.IsNullOrEmpty(ext) && !string.IsNullOrWhiteSpace(ext) &&
				contentTypes.ContainsKey(ext) ? contentTypes[ext] :
				"text/plain; charset=UTF-8";
		}

		private static bool HasKey(NameValueCollection collection, string key)
		{
			for (int i = 0; i < collection.Count; ++i)
			{
				if (collection.GetKey(i) == key) { return true; }
			}
			return false;
		}
	}
}
