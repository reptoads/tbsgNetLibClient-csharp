using System;
using System.Collections.Generic;
using System.Threading;
using ENet;
namespace NetLib{

    internal struct NetEvent
    {
        public NetEvent(EventType type, Packet packet)
        {
            this.type = type;
            this.packet = packet;
        }

        public NetEvent(EventType type) : this()
        {
            this.type = type;
            this.packet = new Packet();
        }

        public ENet.EventType type;
        public Packet packet;
    };

    public abstract class Client
    {
        protected Client()
        {
            this.netEventQueue = new Queue<NetEvent>();
        }

        public void Init()
        {
            Library.Initialize();
            this.host = new Host();
            readThread = new Thread(new ThreadStart(ReceivePackets));
            readThread.IsBackground = true;
        }

        public void DeInt()
        {
            StopThread();
        }
        private void StopThread()
        {
            if (readThread.IsAlive)
            {
                readThread.Abort();
            }
            Library.Deinitialize();
        }

        public void Connect(string ip,ushort port,ulong connectionId){
            host.Create(null, 1);
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            peer = host.Connect(address, 200, 1234);
            Log("[NetLib] connect to server "+ip+":"+port);
            readThread.Start();
        }
        public void Disconnect(){
            if (IsConnected) { 
                peer.DisconnectNow(0);
            }
        }

        public void SendPacket(NetCommands command){
            Packet sending = new Packet();
            sending.Write((uint)command);
            peer.Send(0, sending.Bytes);
        }

        public void SendPacket(NetCommands command,Packet packet)
        {
            Packet sending = new Packet();
            sending.Write((uint) command);
            sending.Write(packet);
            peer.Send(0, sending.Bytes);
        }

        public void ReceivePackets()
        {
            while (host.Service(1) >= 0)
            {
                Event @event;
                while (host.CheckEvents(out @event) > 0)
                {
                    Log("[NetLib]  Client: revived " + @event.Type.ToString());
                    switch (@event.Type)
                    {
                        case ENet.EventType.Connect:
                            Log("[NetLib] Connect");
                            isConnected = true;
                            netEventQueue.Enqueue(new NetEvent(@event.Type));
                            break;
                        case ENet.EventType.Disconnect:
                            Log("[NetLib] Disconnect");
                            isConnected = false;
                            netEventQueue.Enqueue(new NetEvent(@event.Type));
                            break;
                        case ENet.EventType.Receive:
                            var data = @event.Packet.GetBytes();
                            Packet packet = new Packet(data);
                            netEventQueue.Enqueue(new NetEvent(@event.Type, packet));
                            @event.Packet.Dispose();
                            break;
                    }
                }
            }
        }

        public void HandleEvents()
        {
            lock (netEventQueue)
            {
                while (netEventQueue.Count != 0)
                {
                    NetEvent netEvent = netEventQueue.Dequeue();

                    switch (netEvent.type)
                    {
                        case ENet.EventType.None:
                            break;
                        case ENet.EventType.Connect:
                        {
                            isConnected = true;
                            OnConnect();
                            OnConnectEvent?.Invoke();
                        }
                            break;
                        case ENet.EventType.Receive:
                        {
                            HandleAnyPacket(netEvent.packet);
                        }
                            break;
                        case ENet.EventType.Disconnect:
                        {
                            isConnected = false;
                            OnDisconnect();
                            OnDisconnectEvent?.Invoke();
                        }
                            break;
                    }
                }
            }
        }

        public void HandleAnyPacket(Packet packet)
        {
            uint commandInt = packet.ReadUint();
            var command = (NetCommands)commandInt;
            if (command == NetCommands.CustomCommand)
            {
                uint customCommand = packet.ReadUint();
                Log("[NetLib] handling customCommand " + customCommand.ToString());
                HandleCustomPacket(customCommand, packet);
            }
            else
            {
                Log("[NetLib] handling NetCommands " + ((NetCommands)command).ToString());
                HandlePacket(command, packet);
            }

        }

        private void HandlePacket(NetCommands command, Packet packet)
        {
            switch (command)
            {
                case NetCommands.IdentifySuccessful:
                    isIdentified = true;
                    Log("[NetLib] Identify successful");
                    OnIdentificationSuccess();
                    OnIdentificationSuccessEvent?.Invoke();
                    break;
                case NetCommands.IdentifyFailure:
                    isIdentified = false;
                    Log("[NetLib] Identify failure: The server didn't accept your access token! It might be already in use or it has expired. Please retry logging in.");
                    var failuerCode = (IdentifyResponse) packet.ReadUint();
                    OnIdentificationFailure(failuerCode);
                    OnIdentificationFailureEvent?.Invoke(failuerCode);
                    break;
                case NetCommands.HandshakeSuccess:
                    Log("[NetLib]  Handshake successful");
                    var identificationPacket = GetIdentity();
                    SendPacket(NetCommands.Identify, identificationPacket);
                    Log("[NetLib] Identifying...");
                    break;
                default:
                    Log("[NetLib] command was not implemented");
                    break;
            }
        }

        public abstract void OnConnect();
        public abstract void OnDisconnect();
        public abstract void OnIdentificationSuccess();
        public abstract void OnIdentificationFailure(IdentifyResponse identifyResponse);

        protected abstract Packet GetIdentity();
        protected abstract void HandleCustomPacket(uint customCommand, Packet packet);

        protected abstract void Log(string msg);


        public bool IsConnected => isConnected;

        public bool IsIdentified => isIdentified;

        private Host host;
        private Peer peer;
        private Connection connection;

        private Queue<NetEvent> netEventQueue;
        private Thread readThread;
        private bool isConnected = false;
        private bool isIdentified = false;



        //events:
        public struct Delegates {
            public delegate void OnConnect();
            public delegate void OnDisconnect();
            public delegate void OnIdentificationSuccess();
            public delegate void OnIdentificationFailure(IdentifyResponse identifyResponse);
        }

            public static event Delegates.OnConnect OnConnectEvent;
            public static event Delegates.OnDisconnect OnDisconnectEvent;
            public static event Delegates.OnIdentificationSuccess OnIdentificationSuccessEvent;
            public static event Delegates.OnIdentificationFailure OnIdentificationFailureEvent;


    }

}