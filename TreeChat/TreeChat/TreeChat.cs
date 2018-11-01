using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace TreeChat
{
    public enum PacketType : byte
    {
        ConnectToParent,
        RespondFromParent,
        Message,
        MessageDelivered,
        AreYouAlive,
        ImAlive
    }

    public class TreeChat
    {
        private static readonly int MaxAllowedAttempts = 5;
        private static readonly int MaxMessagesStored = 50;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan IterationsDelay = TimeSpan.FromMilliseconds(1000);

        private readonly UdpClient udpClient;
        private readonly string userName;
        private readonly int lossPercent;
        private IPEndPoint parentNode;

        private readonly CancellationTokenSource source
            = new CancellationTokenSource();

        private Task sendTask;
        private Task receiveTask;

        private bool readyForWork = false;

        private readonly ConcurrentDictionary<IPEndPoint, NeighbourNode> neighbours
            = new ConcurrentDictionary<IPEndPoint, NeighbourNode>();

        private readonly ConcurrentDictionary<Guid, Message> recentMessages =
            new ConcurrentDictionary<Guid, Message>();


        public TreeChat(string userName, int lossPercent, IPEndPoint parentNode, int localPort)
        {
            this.userName = userName;
            this.lossPercent = lossPercent;

            this.parentNode = parentNode;
            if (parentNode == null)
            {
                readyForWork = true;
            }
            else
            {
                readyForWork = false;
                neighbours.TryAdd(parentNode, new NeighbourNode());
            }

            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, localPort));
        }

        public void Launch()
        {
            sendTask = RunTask(KeepSending);

            receiveTask = RunTask(KeepReceiving);
        }

        private Task RunTask(Func<Task> Function)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Function();
                }

                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    Console.WriteLine($"[Error]: {e.Message}");
                }
            });
        }

        private async Task KeepSending()
        {
            while (!source.Token.IsCancellationRequested)
            {
                while (!readyForWork)
                {
                    byte[] message = { (byte)PacketType.ConnectToParent };
                    await udpClient.SendAsync(message, message.Length, parentNode).ConfigureAwait(false);

                    await Task.Delay(IterationsDelay, source.Token).ConfigureAwait(false);
                }

                while (true)
                {
                    foreach (var node in neighbours)
                    {
                        if (MaxAllowedAttempts < node.Value.LastPinged.AttemptsAlready)
                        {
                            DisconnectNeighbour(node.Key);
                            continue;
                        }

                        foreach (var message in node.Value.LastAttempts)
                        {
                            if (MaxAllowedAttempts < message.Value.AttemptsAlready)
                            {
                                DisconnectNeighbour(node.Key);
                                break;
                            }

                            if (DateTime.Now - message.Value.LastAttempt > RetryInterval)
                            {
                                await SendMessage(message.Key, node.Key).ConfigureAwait(false);

                                var newInfo = new AttemptsInfo(message.Value.AttemptsAlready + 1, DateTime.Now);
                                node.Value.LastAttempts[message.Key] = newInfo;
                                node.Value.LastPinged = newInfo;
                            }
                        }

                        if (DateTime.Now - node.Value.LastPinged.LastAttempt > RetryInterval)
                        {
                            var command = (byte)(PacketType.AreYouAlive);
                            byte[] message = { command };
                            await udpClient.SendAsync(message, 1, node.Key).ConfigureAwait(false);

                            var newInfo = new AttemptsInfo(node.Value.LastPinged.AttemptsAlready + 1, DateTime.Now);
                            node.Value.LastPinged = newInfo;
                        }
                    }

                    await Task.Delay(IterationsDelay, source.Token).ConfigureAwait(false);
                }
            }
        }

        private void HandleConnectionToParent(UdpReceiveResult packet)
        {
            readyForWork = true;
            Console.WriteLine("[Info]: Connected to parent. ");
        }

        private async Task HandleChildConnection(UdpReceiveResult packet)
        {
            await RespondToChild(packet.RemoteEndPoint);

            neighbours.TryAdd(packet.RemoteEndPoint, new NeighbourNode());

            Console.WriteLine($"[Info]: New child connected: {packet.RemoteEndPoint}");
        }

        private async Task RespondToChild(IPEndPoint destination)
        {
            byte[] message = { (byte)PacketType.RespondFromParent };

            await udpClient.SendAsync(message, message.Length, destination).ConfigureAwait(false);
        }

        public void SendMessage(string text)
        {
            var message = new Message(userName, text, DateTime.Now);

            foreach (var node in neighbours)
            {
                node.Value.LastAttempts
                    .TryAdd(message, new AttemptsInfo(attemptsAlready: 1, lastAttempt: DateTime.Now));
                SendMessage(message, node.Key).Wait();
            }

            Console.WriteLine(message);
        }

        private async Task SendMessage(Message message, IPEndPoint recipient)
        {
            byte[] serializedMessage;
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, message);
                serializedMessage = ms.GetBuffer();
            }

            byte[] buffer = new byte[serializedMessage.Length + 1];
            buffer[0] = (byte)PacketType.Message;
            Array.Copy(serializedMessage, 0, buffer, 1, serializedMessage.Length);

            await udpClient.SendAsync(buffer, buffer.Length, recipient).ConfigureAwait(false);
        }

        private async Task KeepReceiving()
        {
            while (!source.Token.IsCancellationRequested)
            {
                UdpReceiveResult packet;
                try
                {
                    packet = await udpClient.ReceiveAsync().ConfigureAwait(false);
                }
                catch (Exception e) when (!(e is SocketException))
                {
                    Console.WriteLine($"[Error]: {e.Message}");
                    continue;
                }

                if (new Random().Next(100) < lossPercent)
                {
                    //Console.WriteLine($"[Error]: Lost packed from {packet.RemoteEndPoint}");
                    continue;
                }

                try
                {
                    await RecievePacket(packet).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error]: something went wrong " +
                                      $"when handling packet from {packet.RemoteEndPoint}: {e.Message}");
                    throw;
                }
            }
        }

        private async Task RecievePacket(UdpReceiveResult packet)
        {
            switch ((PacketType)packet.Buffer[0])
            {
                case PacketType.ConnectToParent:
                    await HandleChildConnection(packet).ConfigureAwait(false);
                    break;

                case PacketType.RespondFromParent:
                    HandleConnectionToParent(packet);
                    break;

                case PacketType.Message:
                    await HandleMessageRecievement(packet).ConfigureAwait(false);
                    break;

                case PacketType.MessageDelivered:
                    byte[] guidBytes = new byte[packet.Buffer.Length - 1];
                    Array.Copy(packet.Buffer, 1, guidBytes, 0, guidBytes.Length);

                    AttemptsInfo value;
                    var messageToRemove = new Message(guidBytes);
                    neighbours[packet.RemoteEndPoint].LastAttempts.TryRemove(messageToRemove, out value);
                    neighbours[packet.RemoteEndPoint].ResetAttempts();

                    break;

                case PacketType.AreYouAlive:
                    byte[] message = { (byte)PacketType.ImAlive };
                    await udpClient.SendAsync(message, 1, packet.RemoteEndPoint).ConfigureAwait(false);
                    break;

                case PacketType.ImAlive:
                    neighbours[packet.RemoteEndPoint].ResetAttempts();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleMessageRecievement(UdpReceiveResult packet)
        {
            Message message;
            using (var ms = new MemoryStream(packet.Buffer, 1, packet.Buffer.Length - 1))
            {
                message = (Message)new BinaryFormatter().Deserialize(ms);
            }

            byte[] serializedGuid = message.Id.ToByteArray();
            byte[] buffer = new byte[serializedGuid.Length + 1];
            buffer[0] = (byte)PacketType.MessageDelivered;
            Array.Copy(serializedGuid, 0, buffer, 1, serializedGuid.Length);

            await udpClient.SendAsync(buffer, buffer.Length, packet.RemoteEndPoint).ConfigureAwait(false);

            neighbours[packet.RemoteEndPoint].ResetAttempts();

            if (recentMessages.TryAdd(message.Id, message))
            {
                Console.WriteLine(message);

                foreach (var node in neighbours)
                {
                    if (!node.Key.Equals(packet.RemoteEndPoint))
                    {
                        node.Value.LastAttempts.TryAdd(message, new AttemptsInfo(1, DateTime.Now));
                        await SendMessage(message, node.Key).ConfigureAwait(false);
                    }
                }

                if (recentMessages.Count >= MaxMessagesStored)
                {
                    Message value;
                    var idToRemove = recentMessages
                        .OrderBy(kvp => kvp.Value.WhenSent)
                        .First()
                        .Key;
                    recentMessages.TryRemove(idToRemove, out value);
                }
            }
        }
        private void DisconnectNeighbour(IPEndPoint node)
        {
            NeighbourNode value;
            if (neighbours.TryRemove(node, out value))
            {
                if (node.Equals(parentNode))
                {
                    Console.WriteLine($"[Info]: Parent {node} disconnected. We are the root now.");
                }
                else
                {
                    Console.WriteLine($"[Info]: Child {node} disconnected");
                }
            }
        }

        public void Stop()
        {
            Console.WriteLine("[Info]: Terminating all tasks...");
            source.Cancel();
            Task.WaitAll(sendTask, receiveTask);
        }

    }
}