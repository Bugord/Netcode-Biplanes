namespace Network
{
    public enum ConnectStatus
    {
        Undefined,
        /// <summary>
        /// Client successfully connected. This may also be a successful reconnect.
        /// </summary>
        Success,
        /// <summary>
        /// Can't join, server is already at capacity.
        /// </summary>
        ServerFull,
        /// <summary>
        /// Logged in on a separate client, causing this one to be kicked out.
        /// </summary>
        LoggedInAgain,
        /// <summary>
        /// Intentional Disconnect triggered by the user.
        /// </summary>
        UserRequestedDisconnect,
        /// <summary>
        /// Server disconnected, but no specific reason given.
        /// </summary>
        GenericDisconnect,
        /// <summary>
        /// Client lost connection and is attempting to reconnect.
        /// </summary>
        Reconnecting,
        /// <summary>
        /// Client build type is incompatible with server.
        /// </summary>
        IncompatibleBuildType,
        /// <summary>
        /// Host intentionally ended the session.
        /// </summary>
        HostEndedSession,
        /// <summary>
        /// Server failed to bind.
        /// </summary>
        StartHostFailed,
        /// <summary>
        /// Failed to connect to server and/or invalid network endpoint.
        /// </summary>
        StartClientFailed,
        /// <summary>
        /// Server intentionally ended the session.
        /// </summary>
        ServerEndedSession,
        /// <summary>
        /// Client build version is incompatible with server.
        /// </summary>
        IncompatibleVersions,
        /// <summary>
        /// Server failed to bind.
        /// </summary>
        StartServerFailed
    }
}