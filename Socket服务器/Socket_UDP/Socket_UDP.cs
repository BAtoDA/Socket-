using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Socket服务器.Socket_UDP
{
    /// <summary>
    /// Socket_udp对象
    /// </summary>
    class Socket_UDP
    {
        /// <summary>
        /// 传入的IP地址与端口
        /// </summary>
        IPEndPoint IPEnd;
        /// <summary>
        /// Udp客户端泛型表
        /// </summary>
        public List<UdpClient> Sockets = new List<UdpClient>();
        /// <summary>
        /// 显示接收报文框
        /// </summary>
        Sunny.UI.UIRichTextBox RichText;
        /// <summary>
        /// IP Udp对象下拉菜单选择
        /// </summary>
        Sunny.UI.UIComboBox ComboBox;
        /// <summary>
        /// UDP接收方端口
        /// </summary>
        Sunny.UI.UITextBox uITextBox;
        /// <summary>
        /// 接收客户端发生消息缓冲区
        /// </summary>
        byte[] reception = new byte[100];//接收字节
        /// <summary>
        /// Udp对象
        /// </summary>
        Socket UdpClientLoad;
        public Socket_UDP(IPEndPoint iPEnd, Sunny.UI.UIRichTextBox richTextBox, Sunny.UI.UIComboBox combo,Sunny.UI.UITextBox uITextBox)
        {
            this.IPEnd = iPEnd;
            this.RichText = richTextBox;
            this.ComboBox = combo;
            ComboBox.Items.Clear();//清空下拉
            this.uITextBox = uITextBox;
        }
        /// <summary>
        /// 创建Udp对象
        /// </summary>
        public void Socket_Udp()
        {
            UdpClientLoad = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpClientLoad.Bind(IPEnd);
            ComboBox.BeginInvoke((EventHandler)delegate
            {
                ComboBox.Items.Add(IPEnd.Address+IPEnd.Port.ToString());
                ComboBox.SelectedIndex = ComboBox.Items.Count - 1;
            });
            EndPoint endPoint = IPEnd;
            UdpClientLoad.BeginReceiveFrom(reception, 0, reception.Length, SocketFlags.None,ref endPoint, SocketReceive, UdpClientLoad);
        }
        public void Socket_Callback(IAsyncResult async)
        {
            Socket socket = (Socket)async.AsyncState;
            Socket socketuDP= socket.EndAccept(async);
            socketuDP.BeginReceive(reception, 0, reception.Length, SocketFlags.None, SocketReceive, socketuDP);
        }
        /// <summary>
        /// 向指定IP端口发生消息
        /// </summary>
        /// <param name="Data"></param>
        public void SocketSend(string Data)
        {
            if (!SocketHeartbeat(UdpClientLoad))
                return;
            EndPoint endPoint = new IPEndPoint(IPEnd.Address,int.Parse(uITextBox.Text??"00"));
            UdpClientLoad.BeginSendTo(Encoding.ASCII.GetBytes(Data), 0, Encoding.ASCII.GetBytes(Data).Length, SocketFlags.None,endPoint, SocketSend_Callback, $"向IP:{IPEnd.Address +"   "+ IPEnd.Port.ToString()} 发送：" + Data + "成功 \r\n");
        }
        /// <summary>
        /// 发送消息回调
        /// </summary>
        /// <param name="async"></param>
        public void SocketSend_Callback(IAsyncResult async)
        {
            RichTextcontrol(async.AsyncState.ToString() + "\r\n");
        }
        /// <summary>
        /// 异步接收Udp数据
        /// </summary>
        /// <param name="async"></param>
        public void SocketReceive(IAsyncResult async)
        {
            try
            {
                Socket udpClient = (Socket)(async.AsyncState);
                if (!SocketHeartbeat(udpClient))
                    return;
                int index = udpClient.EndReceive(async);
                RichTextcontrol($"远程主机IP：{IPEnd.Address + "   " + IPEnd.Port.ToString()} 回复报文： {BitConverter.ToString(reception, 0, index)} \r\n");
                EndPoint endPoint = IPEnd;
                udpClient.BeginReceiveFrom(reception, 0, reception.Length, SocketFlags.None, ref endPoint, SocketReceive, udpClient);
            }
            catch
            {
                //显示自定IP掉线
                RichTextcontrol($"IP:{IPEnd.Address + "   " + IPEnd.Port.ToString()} 已切断链接 \r\n");
            }
        }
        /// <summary>
        /// 判断检测客户端是否在线
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool SocketHeartbeat(Socket socket)
        {
            if (UdpClientLoad != null)
            {
                if (!socket.Poll(1000, SelectMode.SelectRead))
                {
                    return true;
                }
            }
            //显示自定IP掉线
            RichTextcontrol($"IP:{IPEnd.Address + "   " + IPEnd.Port.ToString()} 已切断链接 \r\n");
            return false;
        }
        /// <summary>
        /// 显示文本内容
        /// </summary>
        /// <param name="Value">显示值</param>
        private void RichTextcontrol(string Value)
        {

            RichText.BeginInvoke((EventHandler)delegate
            {
                RichText.AppendText(Value + " \r\n");
                RichText.Select(RichText.TextLength, 0);
                RichText.ScrollToCaret();
            });
        }
        /// <summary>
        /// Socket服务器释放
        /// </summary>
        public void SocketClose()
        {
            if (UdpClientLoad != null)
            {
                Sockets.ForEach(s1 => { s1.Close(); });
                Sockets = new List<UdpClient>();
                ComboBox.Items.Clear();//清空下拉
                UdpClientLoad.Close();//释放对象
                UdpClientLoad = null;
            }
        }
    }
}
