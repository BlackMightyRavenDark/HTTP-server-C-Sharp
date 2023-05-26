using System;
using System.Net.Sockets;

namespace GuiServer
{
    internal class NativeSocket : IDisposable
    {
        public Socket Handle { get; private set; }
        public bool IsDisposed { get; private set; }

        public NativeSocket(Socket socket)
        {
            Handle = socket;
        }

        public void Dispose()
        {
            IsDisposed = true;

            if (Handle != null)
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

                Handle = null;
            }
        }
    }
}
