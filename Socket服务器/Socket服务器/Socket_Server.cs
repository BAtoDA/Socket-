using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Sunny.UI.Controls;
namespace Socket服务器.Socket服务器
{
    /// <summary>
    /// 用于Socket服务器实现
    /// </summary>
    class Socket_Server
    {
        /// <summary>
        /// 传入的IP地址与端口
        /// </summary>
        IPEndPoint IPEnd;
        /// <summary>
        /// Socket客户端泛型表
        /// </summary>
        public  List<Socket> Sockets = new List<Socket>();
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

        IList<ArraySegment<Byte>> Data = new List<ArraySegment<Byte>>() { new ArraySegment<byte>(new byte[10])  };
        public Socket_Server(IPEndPoint iPEnd,Sunny.UI.UIRichTextBox richTextBox, Sunny.UI.UIComboBox  combo)
        {
            this.IPEnd = iPEnd;
            this.RichText = richTextBox;
            this.ComboBox = combo;
            ComboBox.Items.Clear();//清空下拉
        }
        /// <summary>
        /// Socket套接字加载开放
        /// </summary>
        public  void  SocketLoad()
        {
            //创建Socket服务器
            socketload = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketload.Bind(this.IPEnd);
            //监听客户端
            socketload.Listen(10);
            socketload.BeginAccept(new AsyncCallback(Socketcall_back), socketload);
        }
        /// <summary>
        /// 回调获取Socket客户端对象
        /// </summary>
        /// <param name="async"></param>
        private void Socketcall_back(IAsyncResult async)
        {
            //强制对象
            Socket socket = (Socket)async.AsyncState;
            try
            {
                if (socketload == null)
                    return;
                //获取Socket客户端对象
                Socket Socketclient = socket.EndAccept(async);
                if (SocketHeartbeat(Socketclient))
                {
                    Socketclient.BeginReceive(reception, 0, reception.Length, SocketFlags.None, new AsyncCallback(SocketRend), Socketclient);//异步接收数据
                    Sockets.Add(Socketclient);
                    ComboBox.BeginInvoke((EventHandler)delegate
                    {
                        ComboBox.Items.Add(Socketclient.RemoteEndPoint);
                        ComboBox.SelectedIndex = ComboBox.Items.Count - 1;
                    });
                    RichTextcontrol($"有客户端接入：{Socketclient.RemoteEndPoint}\r\n");
                }
            }
            catch
            {
                if (socketload == null)
                    return;
             
            }
            finally
            {
                //继续回调监听客户端
                socket.BeginAccept(new AsyncCallback(Socketcall_back), socket);
            }
        }
        /// <summary>
        /// 向客户端发生数据
        /// </summary>
        /// <param name="Data">需要发生的数据</param>
        /// <param name="inedx">Socket客户端索引</param>
        public void SocketSend(string Data,int inedx)
        {
            try
            {
                Sockets[inedx].BeginSend(Encoding.UTF8.GetBytes(Data ?? "00"), 0, Encoding.UTF8.GetBytes(Data).Length, SocketFlags.None, new AsyncCallback(SocketSendcall_back), $"向IP:{Sockets[inedx].RemoteEndPoint} 发送：" + Data + "成功 \r\n");
            }
            catch (Exception e)
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
        /// 监听客户端发生的消息
        /// </summary>
        /// <param name="async"></param>
        public void SocketRend(IAsyncResult async)
        {
            Socket socket = async.AsyncState as Socket;
            try
            {
                int d = socket.IOControl(IOControlCode.DataToRead, null, new byte[100]);
                if (!SocketHeartbeat(socket)) return;
                //获取接收字节长度
                int index = socket.EndReceive(async);
                RichTextcontrol($"接收到IP：{socket.RemoteEndPoint} 的数据：{BitConverter.ToString(reception,0, index)} \r\n");
                socket.BeginReceive(reception, 0, reception.Length, SocketFlags.None, new AsyncCallback(SocketRend), socket);//异步接收数据
            }
            catch(Exception e)
            {
            }
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
