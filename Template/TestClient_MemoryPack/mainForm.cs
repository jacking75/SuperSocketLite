using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


namespace csharp_test_client;

[SupportedOSPlatform("windows10.0.177630")]
public partial class mainForm : Form
{
    ClientSimpleTcp Network = new ClientSimpleTcp();

    bool IsNetworkThreadRunning = false;
    bool IsBackGroundProcessRunning = false;

    System.Threading.Thread NetworkReadThread = null;
    System.Threading.Thread NetworkSendThread = null;

    PacketBufferManager PacketBuffer = new PacketBufferManager();
    Queue<byte[]> RecvPacketQueue = new ();
    Queue<byte[]> SendPacketQueue = new ();

    System.Windows.Forms.Timer dispatcherUITimer = new();


    public mainForm()
    {
        InitializeComponent();
    }

    private void mainForm_Load(object sender, EventArgs e)
    {
        PacketBuffer.Init((8096 * 10), MemoryPackPacketHeader.HeaderSize, 1024);

        IsNetworkThreadRunning = true;
        NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
        NetworkReadThread.Start();
        NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
        NetworkSendThread.Start();

        IsBackGroundProcessRunning = true;
        dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
        dispatcherUITimer.Interval = 100;
        dispatcherUITimer.Start();

        btnDisconnect.Enabled = false;

        SetPacketHandler();
        DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
    }

    private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        IsNetworkThreadRunning = false;
        IsBackGroundProcessRunning = false;

        Network.Close();
    }

    private void btnConnect_Click(object sender, EventArgs e)
    {
        string address = textBoxIP.Text;

        if (checkBoxLocalHostIP.Checked)
        {
            address = "127.0.0.1";
        }

        int port = Convert.ToInt32(textBoxPort.Text);

        if (Network.Connect(address, port))
        {
            labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;

            DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);
        }
        else
        {
            labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
        }
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
        SetDisconnectd();
        Network.Close();
    }
        

    void NetworkReadProcess()
    {
        while (IsNetworkThreadRunning)
        {
            if (Network.IsConnected() == false)
            {
                System.Threading.Thread.Sleep(1);
                continue;
            }

            var recvData = Network.Receive();

            if (recvData != null)
            {
                PacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                while (true)
                {
                    var data = PacketBuffer.Read();
                    if (data == null)
                    {
                        break;
                    }

                    lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                    {
                        RecvPacketQueue.Enqueue(data);
                    }
                }
                //DevLog.Write($"받은 데이터: {recvData.Item2}", LOG_LEVEL.INFO);
            }
            else
            {
                Network.Close();
                SetDisconnectd();
                DevLog.Write("서버와 접속 종료 !!!", LOG_LEVEL.INFO);
            }
        }
    }

    void NetworkSendProcess()
    {
        while (IsNetworkThreadRunning)
        {
            System.Threading.Thread.Sleep(1);

            if (Network.IsConnected() == false)
            {
                continue;
            }

            lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
            {
                if (SendPacketQueue.Count > 0)
                {
                    var packet = SendPacketQueue.Dequeue();
                    Network.Send(packet);
                }
            }
        }
    }


    void BackGroundProcess(object sender, EventArgs e)
    {
        ProcessLog();

        try
        {
            byte[] packet = null;

            lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
            {
                if (RecvPacketQueue.Count() > 0)
                {
                    packet = RecvPacketQueue.Dequeue();
                }
            }

            if (packet != null)
            {
                PacketProcess(packet);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
        }
    }

    private void ProcessLog()
    {
        // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
        int logWorkCount = 0;

        while (IsBackGroundProcessRunning)
        {
            System.Threading.Thread.Sleep(1);

            string msg;

            if (DevLog.GetLog(out msg))
            {
                ++logWorkCount;

                if (listBoxLog.Items.Count > 512)
                {
                    listBoxLog.Items.Clear();
                }

                listBoxLog.Items.Add(msg);
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
            else
            {
                break;
            }

            if (logWorkCount > 8)
            {
                break;
            }
        }
    }


    public void SetDisconnectd()
    {
        if (btnConnect.Enabled == false)
        {
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
        }

        SendPacketQueue.Clear();

        labelStatus.Text = "서버 접속이 끊어짐";
    }

    public void PostSendPacket(byte[] packetData)
    {
        if (Network.IsConnected() == false)
        {
            DevLog.Write("서버 연결이 되어 있지 않습니다", LOG_LEVEL.ERROR);
            return;
        }

        SendPacketQueue.Enqueue(packetData);
    }

    byte[] GenerateRandomBytes(int size)
    {
        // 암호학적으로 안전한 난수 생성기 사용
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[size];
            rng.GetBytes(bytes);
            return bytes;
        }
    }


    // MemoryPack 패킷으로 테스트
    private void button3_Click(object sender, EventArgs e)
    {
        var response = new PKTReqResEcho()
        {
            DummyData = textBox3.Text,
        };

        var sendData = MemoryPackSerializer.Serialize(response);
        MemoryPackPacketHeader.Write(sendData, PacketId.ReqResTestEcho);
        PostSendPacket(sendData);
    }
}
