using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace TicTacToeClientSide
{
    public partial class MainWindow : Window
    {
        private const int port = 27001;
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Player_Name.Text == string.Empty || string.IsNullOrWhiteSpace(Player_Name.Text))
                MessageBox.Show("Enter player Name", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                ConnectToServer();
                RequestLoop();
            }
        }

        private void RequestLoop()
        {
            var receiver = Task.Run(() =>
            {
                while (true)
                {
                    ReceiveResponse();
                }
            });
        }


        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            IntegrateToView(text);
        }
        public bool HasSecondPlayerStart { get; set; } = false;

        private void IntegrateToView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var data = text.Split('\n');
                var row1 = data[0].Split('\t');
                var row2 = data[1].Split('\t');
                var row3 = data[2].Split('\t');

                b1.Content = row1[0];
                b2.Content = row1[1];
                b3.Content = row1[2];

                b4.Content = row2[0];
                b5.Content = row2[1];
                b6.Content = row2[2];

                b7.Content = row3[0];
                b8.Content = row3[1];
                b9.Content = row3[2];
                // EnabledAllButtons(true);
            });
        }

        private void ConnectToServer()
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    ++attempts;
                    ClientSocket.Connect(IPAddress.Parse("10.2.13.15"), port);
                }
                catch (Exception)
                {
                }
            }

            MessageBox.Show("Connected");

            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text = Encoding.ASCII.GetString(data);
            this.Title = "Player : " + text;
            this.player.Text = this.Title;

        }
        private void b1_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    var bt = sender as Button;
                    string request = bt.Content.ToString() + player.Text.Split(' ')[2];
                    SendString(request);

                    // EnabledAllButtons(false);
                });
            });
        }

        public void EnabledAllButtons(bool enabled)
        {
            foreach (var item in myWrap.Children)
            {
                if (item is Button bt)
                {
                    bt.IsEnabled = enabled;
                }
            }
        }

        private void SendString(string request)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(request);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
    }
}
