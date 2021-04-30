using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Socket服务器.Socket客户端
{
    /// <summary>
    /// Socket客户端
    /// </summary>
    class Socket_Client
    {
        /// <summary>
        /// 传入的IP地址与端口
        /// </summary>
        IPEndPoint IPEnd;
        /// <summary>
        /// Socket客户端泛型表
        /// </summary>
        public List<Socket> Sockets = new List<Socket>();
        /// <summary>
        /// 显示接收报文框
        /// </summary>
        Sunny.UI.UIRichTextBox RichText;
        /// <summary>
        /// IP Socket对象下拉菜单选择
        /// </summary>
        Sunny.UI.UIComboBox ComboBox;
        /// <summary>
        /// 接收客户端发生消息缓冲区
        /// </summary>
        byte[] reception = new byte[100];//接收字节
        /// <summary>
        /// Socket服务器对象
        /// </summary>
        Socket socketload;
        public Socket_Client(IPEndPoint iPEnd, Sunny.UI.UIRichTextBox richTextBox, Sunny.UI.UIComboBox combo)
        {
            this.IPEnd = iPEnd;
            this.RichText = richTextBox;
            this.ComboBox = combo;
            ComboBox.Items.Clear();//清空下拉
        }
        /// <summary>
        /// Socket套接字加载开放
        /// </summary>
        public void SocketLoad()
        {
            //创建Socket客户端
            socketload = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketload.BeginConnect(IPEnd,new AsyncCallback(SocketCallback),socketload);
        }
        /// <summary>
        /// 链接服务器回调
        /// </summary>
        /// <param name="async"></param>
        public void SocketCallback(IAsyncResult async)
        {
            //强转对象
            Socket socket = (Socket)async.AsyncState;
            try
            {
                //获取Socket套接字信息
                socket.EndConnect(async);
                if(SocketHeartbeat(socket))
                {
                    socket.BeginReceive(reception, 0, reception.Length, SocketFlags.None, new AsyncCallback(SocketReceive), socket);//异步接收数据
                    Sockets.Add(socket);
                    ComboBox.BeginInvoke((EventHandler)delegate
                    {
                        ComboBox.Items.Add(IPEnd.Address+"  "+IPEnd.Port.ToString());
                        ComboBox.SelectedIndex = ComboBox.Items.Count - 1;
                    });
                    RichTextcontrol($"成功链接服务器IP：{socket.RemoteEndPoint}\r\n");
                }
            }
            catch(Exception e)
            {
                RichTextcontrol($"链接服务器：ip {IPEnd.Address}   {IPEnd.Port} 失败!! 原因是：{e.Message}\r\n ");
            }
        }
        /// <summary>
        /// 监听来自服务器的消息
        /// </summary>
        public void SocketReceive(IAsyncResult async)
        {
            //获取Socket服务器消息
            Socket socket = async.AsyncState as Socket;
            try
            {
                if (!SocketHeartbeat(socket)) return;
                int index = socket.EndReceive(async);
                RichTextcontrol($"接收到IP：{socket.RemoteEndPoint} 的数据：{BitConverter.ToString(reception, 0, index)} \r\n");
                socket.BeginReceive(reception, 0, reception.Length, SocketFlags.None, new AsyncCallback(SocketReceive), socket);//异步接收数据
            }
            catch(Exception e)
            {
                RichTextcontrol($"服务器IP：{IPEnd.Address+"  "+IPEnd.Port.ToString()} 监听数据失败 \r\n");
            }
        }
        /// <summary>
        /// 向服务器发生消息
        /// </summary>
        /// <param name="Data">需要发生的数据</param>
        /// <param name="inedx">Socket客户端索引</param>
        public void SocketSend(string Data, int inedx)
        {
            try
            {
                Sockets[inedx].BeginSend(Encoding.ASCII.GetBytes(Data ?? "00"), 0, Encoding.ASCII.GetBytes(Data ?? "00").Length, SocketFlags.None,
                    new AsyncCallback(SocketSendcall_back), $"向IP:{Sockets[inedx].RemoteEndPoint} 发送：" + Data + "成功 \r\n");
            }
            catch(Exception e)
            {
                if (Sockets.Count > 0)
                    MessageBox.Show($"向IP:{Sockets[inedx].RemoteEndPoint} 错误：{e.Message} " + "\r\n");
                else
                    MessageBox.Show($"未链接任何Socket客户端。" + "\r\n");
            }
        }
        /// <summary>
        /// 消息发送状态监控 
        /// </summary>
        /// <param name="async"></param>
        private void SocketSendcall_back(IAsyncResult async)
        {
            RichTextcontrol(async.AsyncState.ToString() + "\r\n");
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
        /// 判断检测客户端是否在线
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool SocketHeartbeat(Socket socket)
        {
            if (socketload != null)
            {
                if (socket.Connected != false && !socket.Poll(1000, SelectMode.SelectRead))
                {
                    return true;
                }
            }
            //显示自定IP掉线
            RichTextcontrol($"IP:{socket.RemoteEndPoint} 已切断链接 \r\n");
            return false;
        }
        /// <summary>
        /// Socket服务器释放
        /// </summary>
        public void SocketClose()
        {
            if (socketload != null)
            {
                Sockets.ForEach(s1 => { s1.Close(); });
                Sockets = new List<Socket>();
                ComboBox.Items.Clear();//清空下拉
                socketload.Close();//释放对象
                socketload = null;
            }
        }
    }
}
