/*using Godot;

public class NATTraversal : Node
{
    PacketPeerUDP server = new PacketPeerUDP();
    PacketPeerUDP peer = new PacketPeerUDP();

    bool foundServer = false;
    bool peerGreet = false;
    bool peerInfo = false;
    bool peerConfirm = false;
    bool peerProceed = false;

    bool isHost = false;

    System.Collections.Generic.Dictionary<string, NATTraversal> peers = new System.Collections.Generic.Dictionary<string, NATTraversal>();

    string address;
    int port;

    string hostAddress = "";
    int hostPort = 0;

    public override void _Ready()
    {
        //timer?
    }

    public override void _Process(float delta)
    {
        if (peer.GetAvailablePacketCount() > 0)
        {
            var packet = peer.GetPacket();
            var packetString = new System.Text.ASCIIEncoding().GetString(packet);

            if(!peerGreet)
            {
                if (packetString.BeginsWith("greet"))
                {
                    var split = packetString.Split(":");
                    HandleGreet(split[1], split[2].ToInt(), split[3].ToInt());
                }
            }

            if(!peerConfirm)
            {
                if (packetString.BeginsWith("confirm"))
                {
                    var split = packetString.Split(":");
                    HandleConfirm(split[2], split[1].ToInt(), split[4].ToInt(), split[3]);
                }
            }
        }
    }

    void HandleGreet(string peerName, int peerPort, int myPort)
    {
        if (myPort != port)
        {
            port = myPort;
            peer.Close();
            peer.Listen(port, "*");
        }

        peerGreet = true;
    }

    void HandleConfirm(string peerName, int peerPort, int myPort, bool host)
    {
        if (peers[peerName].port != peerPort)
            peers[peerName].port = peerPort;

        peers[peerName].isHost = host;
        if(host)
        {
            hostAddress = peers[peerName].address
        }
    }

}*/