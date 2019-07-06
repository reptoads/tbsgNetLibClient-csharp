using ENet;

namespace tbsgNetLib
{
    public class Connection
    {
        Connection(Peer peer, uint id)
        {
            this.peer = peer;
            this.id = id;
        }
        public Peer Peer
        {
            get { return peer; }
        }
        public uint Id
        {
            get { return id; }
        }
        public bool Identified
        {
            get { return identified; }
        }

        static public uint IdCount
        {
            get { return idCount; }
        }

        static public uint NewConnectionId()
        {
            idCount++;
            return idCount;
        }
        private Peer peer;
        private uint id = 0;
        private bool identified = false;
        private static uint idCount;
    }
}
