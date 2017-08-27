﻿namespace YanZhiwei.DotNet2.Utilities.Communication
{
    using Enum;
    using Model;
    using Operator;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using YanZhiwei.DotNet2.Utilities.Args;

    /// <summary>
    /// Socket 服务
    /// </summary>
    /// 时间：2016/6/7 22:38
    /// 备注：
    public sealed class HighPerformanceServer
    {
        #region Fields

        private int currentConnections = 0;
        private EndPoint ipeSender;
        private Socket listener;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol">类型</param>
        /// <param name="ipAddress">ip地址</param>
        /// 时间：2016/6/7 11:35
        /// 备注：
        public HighPerformanceServer(SocketProtocol protocol, string ipAddress)
        : this(protocol, ipAddress, 9888)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol">类型</param>
        /// <param name="ipAddress">ip地址</param>
        /// <param name="port">端口</param>
        /// 时间：2016/6/7 11:35
        /// 备注：
        public HighPerformanceServer(SocketProtocol protocol, string ipAddress, ushort port)
        {
            Init(protocol, ipAddress, port);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// 终端已经连接事件
        /// </summary>
        /// 时间：2016/6/7 22:47
        /// 备注：
        public event EventHandler<EventArgs> OnClientConnected;

        /// <summary>
        /// 终端已经断开连接事件
        /// </summary>
        /// 时间：2016/6/7 22:48
        /// 备注：
        public event EventHandler<EventArgs> OnClientDisconnected;

        /// <summary>
        /// 终端正在断开连接事件
        /// </summary>
        /// 时间：2016/6/7 22:48
        /// 备注：
        public event EventHandler<EventArgs> OnClientDisconnecting;

        /// <summary>
        /// 数据接收事件
        /// </summary>
        /// 时间：2016/6/7 22:48
        /// 备注：
        public event EventHandler<SocketSeesionEventArgs> OnDataReceived;

        /// <summary>
        /// 服务启动事件
        /// </summary>
        /// 时间：2016/6/7 22:46
        /// 备注：
        public event EventHandler<EventArgs> OnServerStart;

        /// <summary>
        /// 服务启动完成事件
        /// </summary>
        /// 时间：2016/6/7 22:46
        /// 备注：
        public event EventHandler<EventArgs> OnServerStarted;

        /// <summary>
        /// 服务已经停止事件
        /// </summary>
        /// 时间：2016/6/7 22:47
        /// 备注：
        public event EventHandler<EventArgs> OnServerStoped;

        /// <summary>
        /// 服务正在停止事件
        /// </summary>
        /// 时间：2016/6/7 22:46
        /// 备注：
        public event EventHandler<EventArgs> OnServerStopping;

        #endregion Events

        #region Properties

        /// <summary>
        /// 当前连接数
        /// </summary>
        public int CurrentConnections
        {
            get
            {
                return currentConnections;
            }
        }

        /// <summary>
        /// IPEndPoint
        /// </summary>
        public IPEndPoint Endpoint
        {
            get;
            set;
        }

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxQueuedConnections
        {
            get;
            set;
        }

        /// <summary>
        /// 端口
        /// </summary>
        public ushort Port
        {
            get;
            set;
        }

        /// <summary>
        /// 连接类型
        /// </summary>
        public SocketProtocol Protocol
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// 启动服务
        /// </summary>
        /// 时间：2016/6/7 22:54
        /// 备注：
        /// <exception cref="System.InvalidOperationException">未能成功创建Socket服务类型。</exception>
        public void Start()
        {
            if (OnServerStart != null)
            {
                OnServerStart(this, null);
            }

            listener = GetCorrectSocket();

            if (listener != null)
            {
                listener.Bind(this.Endpoint);

                if (this.Protocol == SocketProtocol.TCP)
                {
                    listener.Listen(this.MaxQueuedConnections);
                    listener.BeginAccept(new AsyncCallback(ClientConnected), listener);
                }
                else if (this.Protocol == SocketProtocol.UDP)
                {
                    SocketConnectionInfo _connection = new SocketConnectionInfo();
                    _connection.Buffer = new byte[SocketConnectionInfo.BufferSize];
                    _connection.Socket = listener;
                    ipeSender = new IPEndPoint(IPAddress.Any, this.Port);
                    listener.BeginReceiveFrom(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, ref ipeSender, new AsyncCallback(DataReceived), _connection);
                }

                if (OnServerStarted != null)
                {
                    OnServerStarted(this, null);
                }
            }
            else
            {
                throw new InvalidOperationException("未能成功创建Socket服务类型。");
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// 时间：2016/6/7 22:54
        /// 备注：
        public void Stop()
        {
            if (OnServerStopping != null)
            {
                OnServerStopping(this, null);
            }

            if (OnServerStoped != null)
            {
                OnServerStoped(this, null);
            }
        }

        /// <summary>
        /// 处理终端异步连接
        /// </summary>
        /// <param name="asyncResult">IAsyncResult</param>
        /// 时间：2016/6/7 22:54
        /// 备注：
        internal void ClientConnected(IAsyncResult asyncResult)
        {
            Interlocked.Increment(ref currentConnections);

            SocketConnectionInfo _connection = new SocketConnectionInfo();
            _connection.Buffer = new byte[SocketConnectionInfo.BufferSize];
            Socket _asyncListener = (Socket)asyncResult.AsyncState;
            Socket _asyncClient = _asyncListener.EndAccept(asyncResult);
            _connection.Socket = _asyncClient;

            if (OnClientConnected != null)
            {
                OnClientConnected(this, null);
            }

            if (this.Protocol == SocketProtocol.TCP)
            {
                _asyncClient.BeginReceive(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, new AsyncCallback(DataReceived), _connection);
            }
            else if (this.Protocol == SocketProtocol.UDP)
            {
                _asyncClient.BeginReceiveFrom(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, ref ipeSender, new AsyncCallback(DataReceived), _connection);
            }

            listener.BeginAccept(new AsyncCallback(ClientConnected), listener);
        }

        internal void ClientDisconnected(IAsyncResult asyncResult)
        {
            SocketConnectionInfo _sci = (SocketConnectionInfo)asyncResult;
            _sci.Socket.EndDisconnect(asyncResult);

            if (OnClientDisconnected != null)
            {
                OnClientDisconnected(this, null);
            }
        }

        /// <summary>
        /// 处理数据异步接收
        /// </summary>
        /// <param name="asyncResult">IAsyncResult</param>
        /// 时间：2016/6/7 22:56
        /// 备注：
        internal void DataReceived(IAsyncResult asyncResult)
        {
            try
            {
                SocketConnectionInfo _connection = (SocketConnectionInfo)asyncResult.AsyncState;
                int _bytesRead;
                SocketSeesionEventArgs _arg = new SocketSeesionEventArgs();
                if (this.Protocol == SocketProtocol.UDP)
                {
                    _bytesRead = _connection.Socket.EndReceiveFrom(asyncResult, ref ipeSender);
                    _arg.TerminalInfo = (IPEndPoint)ipeSender;
                }
                else if (this.Protocol == SocketProtocol.TCP)
                {
                    _bytesRead = _connection.Socket.EndReceive(asyncResult);
                    _arg.TerminalInfo = (IPEndPoint)_connection.Socket.RemoteEndPoint;
                }
                else
                {
                    _bytesRead = 0;
                }

                _connection.BytesRead += _bytesRead;

                if (IsSocketConnected(_connection.Socket))
                {
                    if (_bytesRead == 0 || (_bytesRead > 0 && _bytesRead < SocketConnectionInfo.BufferSize))
                    {
                        byte[] _buffer = _connection.Buffer;
                        int _totalBytesRead = _connection.BytesRead;
                        _connection = new SocketConnectionInfo();
                        _connection.Buffer = new byte[SocketConnectionInfo.BufferSize];
                        _connection.Socket = ((SocketConnectionInfo)asyncResult.AsyncState).Socket;

                        if (this.Protocol == SocketProtocol.UDP)
                        {
                            _connection.Socket.BeginReceiveFrom(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, ref ipeSender, new AsyncCallback(DataReceived), _connection);
                        }
                        else if (this.Protocol == SocketProtocol.TCP)
                        {
                            _connection.Socket.BeginReceive(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, new AsyncCallback(DataReceived), _connection);
                        }

                        if (_totalBytesRead < _buffer.Length)
                        {
                            Array.Resize<byte>(ref _buffer, _totalBytesRead);
                        }

                        if (OnDataReceived != null)
                        {
                            _arg.DataBuffer = _buffer;
                            OnDataReceived(_connection, _arg);
                        }

                        _buffer = null;
                    }
                    else
                    {
                        Array.Resize<Byte>(ref _connection.Buffer, _connection.Buffer.Length + SocketConnectionInfo.BufferSize);

                        if (this.Protocol == SocketProtocol.UDP)
                        {
                            _connection.Socket.BeginReceiveFrom(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, ref ipeSender, new AsyncCallback(DataReceived), _connection);
                        }
                        else if (this.Protocol == SocketProtocol.TCP)
                        {
                            _connection.Socket.BeginReceive(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, new AsyncCallback(DataReceived), _connection);
                        }
                    }
                }
                else if (_connection.BytesRead > 0)
                {
                    Array.Resize<byte>(ref _connection.Buffer, _connection.BytesRead);

                    if (OnDataReceived != null)
                    {
                        _arg.DataBuffer = _connection.Buffer;
                        OnDataReceived(_connection, _arg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal void DisconnectClient(SocketConnectionInfo connection)
        {
            if (OnClientDisconnecting != null)
            {
                OnClientDisconnecting(this, null);
            }

            connection.Socket.BeginDisconnect(true, new AsyncCallback(ClientDisconnected), connection);
        }

        internal Socket GetCorrectSocket()
        {
            if (this.Protocol == SocketProtocol.TCP)
            {
                return new Socket(this.Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (this.Protocol == SocketProtocol.UDP)
            {
                return new Socket(this.Endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }
            else
            {
                return null;
            }
        }

        internal bool IsSocketConnected(Socket socket)
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }

        /// <summary>
        /// 参数初始化
        /// </summary>
        /// <param name="server">ServerType</param>
        /// <param name="ipAddress">Ip地址</param>
        /// <param name="port">端口</param>
        /// 时间：2016/6/7 22:45
        /// 备注：
        /// <exception cref="System.ArgumentException">未能识别的Ip地址。</exception>
        private void Init(SocketProtocol server, string ipAddress, ushort port)
        {
            ValidateOperator.Begin().NotNullOrEmpty(ipAddress, "Ip地址").IsIp(ipAddress, "Ip地址");
            IPAddress _ipAddress;

            if (IPAddress.TryParse(ipAddress, out _ipAddress))
            {
                this.Endpoint = new IPEndPoint(_ipAddress, port);
            }
            else
            {
                throw new ArgumentException("未能识别的Ip地址。");
            }

            this.Protocol = server;
            this.Port = port;
        }

        /// <summary>
        /// 回复终端数据报文
        /// </summary>
        /// <param name="datagram">数据报文</param>
        /// <param name="endpoint">终端信息</param>
        public void Reply(byte[] datagram, IPEndPoint endpoint)
        {
            if (this.Protocol == SocketProtocol.UDP)
            {
                listener.SendTo(datagram, endpoint);
            }
            else if (this.Protocol == SocketProtocol.TCP)
            {
                listener.Send(datagram);
            }
        }

        #endregion Methods
    }
}