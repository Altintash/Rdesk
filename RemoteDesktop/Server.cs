using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace RemoteDesktop
{
    public partial class Server : Form
    {
        public List<Socket> _clientSockets = new List<Socket>();
        public Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public byte[] _bufferSend;
        public Socket _cl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Bitmap desktop = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public byte[] _bufferReceive;
        
        public Server()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            serverStart();
        }

        public void serverStart()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 6099));
            _serverSocket.Listen(20);
            _serverSocket.BeginAccept(new AsyncCallback(ConnectCallback), null);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _cl = _serverSocket.EndAccept(ar);
                _serverSocket.BeginAccept(new AsyncCallback(ConnectCallback), null);
                _clientSockets.Add(_cl);
                _bufferReceive = new byte[_cl.ReceiveBufferSize];

                _cl.BeginReceive(_bufferReceive, 0, _bufferReceive.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch(Exception ex)
            {
                _clientSockets.Remove(_cl);
                MessageBox.Show(ex.Message + " in ConnectCallback", Application.ProductName);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                _cl.EndReceive(ar);

                Application.DoEvents();
                Graphics g = Graphics.FromImage(desktop as Image);
                g.CopyFromScreen(0, 0, 0, 0, desktop.Size);
                ImageConverter imageConverter = new ImageConverter();
                _bufferSend = (byte[])imageConverter.ConvertTo(desktop, typeof(byte[]));

                _cl.BeginSend(_bufferSend, 0, _bufferSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception ex)
            {
                _clientSockets.Remove(_cl);
                MessageBox.Show(ex.Message + " in ReceiveCallback", Application.ProductName);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                _cl.EndSend(ar);
                _cl.BeginReceive(_bufferReceive, 0, _bufferReceive.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                _clientSockets.Remove(_cl);
                MessageBox.Show(ex.Message + " in SendCallback", Application.ProductName);
            }
        }

        private void picMassive(byte[] ImageInArray)
        {
            Application.DoEvents();
            Graphics g = Graphics.FromImage(desktop as Image);
            g.CopyFromScreen(0, 0, 0, 0, desktop.Size);
            ImageConverter imageConverter = new ImageConverter();
            ImageInArray = (byte[])imageConverter.ConvertTo(desktop, typeof(byte[]));
        }
    }
}
