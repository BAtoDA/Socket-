using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sunny;
using System.Net.Sockets;
using System.Net;
using Socket服务器.Socket服务器;
using Socket服务器.Socket客户端;
using Socket服务器.Socket_UDP;

namespace Socket服务器
{
    public partial class MainForm : Sunny.UI.UIForm
    {
        /// <summary>
        /// Socket服务器对象
        /// </summary>
        Socket_Server socket_Server;
        /// <summary>
        /// Socket客户端对象
        /// </summary>
        Socket_Client socket_Client;
        /// <summary>
        /// Udp对象
        /// </summary>
        Socket_UDP.Socket_UDP socket_UDP;
        public MainForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 用户点击开放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            //创建Socket对象
            switch (this.uiComboBox1.SelectedIndex)
            {
                case -1:
                case 0:
                    //判断上一次是否按下
                    if (uiButton1.Selected)
                    {
                        socket_Server.SocketClose();
                        uiButton1.Text = "开发端口";
                        uiButton1.Selected = false;
                        break;
                    }
                    socket_Server = new Socket_Server(new IPEndPoint(IPAddress.Parse(this.uiTextBox1.Text ?? "192.168.3.2"), int.Parse(this.uiTextBox2.Text ?? "2000")), this.uiRichTextBox1, this.uiComboBox2);
                    socket_Server.SocketLoad();
                    uiButton1.Text = "开放完成";
                    uiButton1.Selected = true;
                    break;
                case 1:
                    //判断上一次是否按下
                    if (uiButton1.Selected)
                    {
                        socket_Client.SocketClose();
                        uiButton1.Text = "链接";
                        uiButton1.Selected = false;
                        break;
                    }
                    socket_Client = new Socket_Client(new IPEndPoint(IPAddress.Parse(this.uiTextBox1.Text??"192.168.3.2"), int.Parse(this.uiTextBox2.Text??"2000")),
                       this.uiRichTextBox1, this.uiComboBox2);
                    socket_Client.SocketLoad();
                    uiButton1.Text = "链接完成";
                    uiButton1.Selected = true;
                    break;
                case 2:
                    //判断上一次是否按下
                    if (uiButton1.Selected)
                    {
                        socket_UDP.SocketClose();
                        uiButton1.Text = "链接";
                        uiButton1.Selected = false;
                        break;
                    }
                    socket_UDP =new Socket_UDP.Socket_UDP(new IPEndPoint(IPAddress.Parse(this.uiTextBox1.Text??"192.168.3.2"), int.Parse(this.uiTextBox2.Text??"2000")),
                       this.uiRichTextBox1, this.uiComboBox2,uiTextBox3);
                    socket_UDP.Socket_Udp();
                    uiButton1.Text = "链接完成";
                    uiButton1.Selected = true;
                    break;
            }
            this.uiComboBox1.Enabled = uiButton1.Selected ? false : true;
        }
        /// <summary>
        /// 用户点击发生数据按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            //创建Socket对象
            switch (this.uiComboBox1.SelectedIndex)
            {
                case -1:
                case 0:
                    if (socket_Server != null)
                        socket_Server.SocketSend(this.uiRichTextBox2.Text, this.uiComboBox2.SelectedIndex == -1 ? 0 : this.uiComboBox2.SelectedIndex);
                    return;
                case 1:
                    if (socket_Client != null)
                        socket_Client.SocketSend(this.uiRichTextBox2.Text, this.uiComboBox2.SelectedIndex == -1 ? 0 : this.uiComboBox2.SelectedIndex);
                    return;
                case 2:
                    if(socket_UDP!=null)
                        socket_UDP.SocketSend(this.uiRichTextBox2.Text);
                    break;

            }
        }
        bool Udpflag = false;
        private void uiComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (this.uiComboBox1.Text == "UDP")
            {
                this.uiButton1.Location = new Point(this.uiButton1.Location.X, this.uiButton1.Location.Y + 20);
                this.uiPanel2.Location = new Point(this.uiPanel2.Location.X, this.uiPanel2.Location.Y + 20);
                this.uiPanel2.Size = new Size(this.uiPanel2.Size.Width, this.uiPanel2.Size.Height - 20);
                this.uiTextBox3.Visible = true;
                this.uiLabel5.Visible = true;
                Udpflag = true;
                return;
            }
            if(Udpflag)
            {
                this.uiTextBox3.Visible = false;
                this.uiLabel5.Visible = false;
                this.uiButton1.Location = new Point(this.uiButton1.Location.X, this.uiButton1.Location.Y - 40);
                this.uiPanel2.Location = new Point(this.uiPanel2.Location.X, this.uiPanel2.Location.Y -40);
                this.uiPanel2.Size = new Size(this.uiPanel2.Size.Width, this.uiPanel2.Size.Height + 40);
                Udpflag = false;
            }
        }
    }
}
