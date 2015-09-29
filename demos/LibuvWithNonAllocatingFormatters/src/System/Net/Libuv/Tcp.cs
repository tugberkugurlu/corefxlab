﻿
namespace System.Net.Libuv
{
    public class TcpListener : UVListener<Tcp> 
    {
        UVLoop[] _acceptLoops;
        Random r = new Random();

        public TcpListener(string ipAddress, int port, UVLoop loop, UVLoop[] acceptLoops) : base(loop, HandleType.UV_TCP)
        {
            _acceptLoops = acceptLoops;
            UVException.ThrowIfError(UVInterop.uv_tcp_init(Loop.Handle, Handle));
            Bind(this, ipAddress, port);
        }

        static void Bind(TcpListener listener, string ipAddress, int port, bool ip6 = false, bool dualstack = false)
        {
            if (ip6)
            {
                sockaddr_in6 address = CreateSockaddrIp6(ipAddress, port);
                UVException.ThrowIfError(UVInterop.uv_tcp_bind(listener.Handle, ref address, (uint)(dualstack ? 0 : 1)));
            }
            else
            {
                sockaddr_in address = CreateSockaddr(ipAddress, port);
                UVException.ThrowIfError(UVInterop.uv_tcp_bind(listener.Handle, ref address, 0));
            }
        }

        static sockaddr_in CreateSockaddr(string ip, int port)
        {
            sockaddr_in address;
            int result = UVInterop.uv_ip4_addr(ip, port, out address);
            UVException.ThrowIfError(result);
            return address;
        }

        static sockaddr_in6 CreateSockaddrIp6(string ip, int port)
        {
            sockaddr_in6 address;
            int result = UVInterop.uv_ip6_addr(ip, port, out address);
            UVException.ThrowIfError(result);
            return address;
        }

        protected override UVStream CreateStream()
        {
            // TODO: this policy needs to be smarter
            var acceptLoop = r.Next(0, _acceptLoops.Length - 1);
            return new Tcp(_acceptLoops[acceptLoop]);
        }
    }

    public class Tcp : UVStream
    {
        internal Tcp(UVLoop loop) : base(loop, HandleType.UV_TCP)
        {
            UVException.ThrowIfError(UVInterop.uv_tcp_init(loop.Handle, Handle));
        }

        void SetNoDelay(bool value)
        {
            UVException.ThrowIfError(UVInterop.uv_tcp_nodelay(Handle, value ? 1 : 0));
        }

        void SetKeepAlive(bool value, int delay)
        {
            UVException.ThrowIfError((UVInterop.uv_tcp_keepalive(Handle, value ? 1 : 0, delay)));
        }
    }
}
