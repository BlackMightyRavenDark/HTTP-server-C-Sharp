using System;
using System.Net.Sockets;
using System.Text;

namespace GuiServer
{
	internal class NativeSocket : IDisposable
	{
		public Socket Handle { get; private set; }
		public bool IsDisposed { get; private set; }
		public bool IsConnected => !IsDisposed && Handle != null && Handle.Connected;

		public NativeSocket(Socket socket)
		{
			Handle = socket;
		}

		public void Dispose()
		{
			IsDisposed = true;

			if (Handle != null)
			{
				lock (Handle)
				{
					try
					{
						Handle.Shutdown(SocketShutdown.Both);
						if (Handle.Connected)
						{
							Handle.Disconnect(false);
						}
						Handle.Close();
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(ex.Message);
						Handle.Close();
					}
				}

				Handle = null;
			}
		}

		public void Send(string message, Encoding encoding)
		{
			Handle.Send(encoding.GetBytes(message));
		}

		public void Send(string message)
		{
			Send(message, Encoding.UTF8);
		}
	}
}
