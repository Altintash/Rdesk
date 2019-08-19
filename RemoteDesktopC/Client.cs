using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace RemoteDesktopC
{
    public partial class Client : Form
    {
        public byte[] _bufferReceive;
        public byte[] _bufferSend;
        public string hello = "Hello";
        Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public string ip;

        public Client()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            connectToServer();
        }

        public void connectToServer()
        {
            ip = textBox1.Text;
            try
            {
                _socket.BeginConnect(ip, 6099, new AsyncCallback(ConnectCallback), null);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + " connectToServer", Application.ProductName);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                _bufferSend = Encoding.ASCII.GetBytes(hello);
                _socket.BeginSend(_bufferSend, 0, _bufferSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " ConnectCallback", Application.ProductName);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
                _bufferReceive = new byte[_socket.ReceiveBufferSize * 50];
                _socket.BeginReceive(_bufferReceive, 0, _bufferReceive.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " SendCallback", Application.ProductName);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndReceive(ar);
                ImageConverter imgConv = new ImageConverter();
                Application.DoEvents();
                Image img = (Image)imgConv.ConvertFrom(_bufferReceive);
                pictureBox1.Image = img;
                _socket.BeginSend(_bufferSend, 0, _bufferSend.Length, SocketFlags.None, SendCallback, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " ReceiveCallback", Application.ProductName);
            }
        }
    }
}
