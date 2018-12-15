﻿/*
    Copyright(c) 2014-2018 FOG Project

    The MIT License

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions :
    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using Microsoft.Extensions.Logging;
using NamedPipeWrapper;
using Zazzles.Core.PubSub.IPC;

namespace Zazzles.Windows.PubSub.IPC
{
    class NamedPipeClient : AbstractIPCAgent
    {
        NamedPipeClient<byte[]> client;
        public NamedPipeClient(string pipe, ILogger<AbstractIPCAgent> logger, IParser parser) : base(logger, parser)
        {
            client = new NamedPipeClient<byte[]>(pipe);
            client.
            client.ServerMessage += ((conn, msg) =>
            {
                OnReceive(msg);
            });
        }

        public override bool Connect()
        {
            client.Start();
            return true;
        }

        public override bool Disconnect()
        {
            client.Stop();
            return true;
        }

        protected override bool Send(byte[] msg)
        {
            client.PushMessage(msg);
            return true;
        }

        public override void Dispose()
        {
            client.Stop();
        }

    }
}