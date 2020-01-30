using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket
{
    /// <summary>
    /// Request handler
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    public delegate void RequestHandler<TAppSession, TRequestInfo>(TAppSession session, TRequestInfo requestInfo)
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo;

}
