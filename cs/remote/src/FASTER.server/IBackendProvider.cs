﻿using System;
using System.Net.Sockets;
using FASTER.common;

namespace FASTER.server
{
    /// <summary>
    /// Interface to provides server-side session processing logic
    /// </summary>
    public interface IBackendProvider
    {
        /// <summary>
        /// Given messages of wire format type, returns a session that handles that wire format. If no backend is configured
        /// for the given wire format, an exception is thrown.
        /// </summary>
        /// <param name="wireFormat">Wire format</param>
        /// <param name="socket">Socket connection</param>
        /// <returns>Server session</returns>
        ServerSessionBase GetBackend(WireFormat wireFormat, Socket socket);
    }
}