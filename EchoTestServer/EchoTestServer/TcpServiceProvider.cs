namespace EchoTestServer
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Text;
    using System.Collections;

    public abstract class TcpServiceProvider : ICloneable
    {
        /// <SUMMARY>
        /// Provides a new instance of the object.
        /// </SUMMARY>
        public virtual object Clone()
        {
            throw new Exception("Derived clases" +
                                " must override Clone method.");
        }

        /// <SUMMARY>
        /// Gets executed when the server accepts a new connection.
        /// </SUMMARY>
        public abstract void OnAcceptConnection(ConnectionState state);

        /// <SUMMARY>
        /// Gets executed when the server detects incoming data.
        /// This method is called only if
        /// OnAcceptConnection has already finished.
        /// </SUMMARY>
        public abstract void OnReceiveData(ConnectionState state);

        /// <SUMMARY>
        /// Gets executed when the server needs to shutdown the connection.
        /// </SUMMARY>
        public abstract void OnDropConnection(ConnectionState state);
    }
}