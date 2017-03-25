using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TTT
{
    public partial class Form1 : Form
    {
        public string PlayerXName = @"Stepan";
        public string Player0Name = @"Guest";
        public int[] GameData = new int[9];
        private List<Control> _list = new List<Control>();

        public Form1()
        {
            InitializeComponent();

            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(Button))
                    _list.Add(control);
            }

            new Thread(UpdateBoard).Start();
        }

        private void UpdateBoard()
        {
            UdpClient listener = new UdpClient(4444);
            IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, 4444);
            try
            {
                while (true)
                {
                    var receiveByteArray = listener.Receive(ref groupEp);
                    var receivedData = Encoding.ASCII.GetString(receiveByteArray, 0, receiveByteArray.Length);
                    var index = Convert.ToInt32(receivedData);
                    var control = _list.Single(t => t.Name.Contains(index.ToString()));
                    control.BeginInvoke((MethodInvoker)delegate { control.Text = @"0"; });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private int Gamecheck()
        {
            var line1 = GameData[0] + GameData[1] + GameData[2];
            var line2 = GameData[3] + GameData[4] + GameData[5];
            var line3 = GameData[6] + GameData[7] + GameData[8];
            var col1 = GameData[0] + GameData[3] + GameData[6];
            var col2 = GameData[1] + GameData[4] + GameData[7];
            var col3 = GameData[2] + GameData[5] + GameData[8];
            var diag1 = GameData[6] + GameData[4] + GameData[2];
            var diag2 = GameData[0] + GameData[4] + GameData[8];
            if (line1 == 3 || line2 == 3 || line3 == 3 || col1 == 3 || col2 == 3 || col3 == 3 || diag1 == 3 || diag2 == 3)
            {
                return 1;
            }
            if (line1 == 12 || line2 == 12 || line3 == 12 || col1 == 12 || col2 == 12 || col3 == 12 || diag1 == 12 || diag2 == 12)
            {
                return 2;
            }
            if (GameData.Where(r => r == 0).ToList().Count == 0)
            {
                MessageBox.Show(@"Draw!");
                ResetGame();
                return 3;
            }
            return 0;
        }

        public void ResetGame()
        {
            if (MessageBox.Show(@"Another game?", @"Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                Close();
            }
            btn1.Text = @"";
            btn2.Text = @"";
            btn3.Text = @"";
            btn4.Text = @"";
            btn5.Text = @"";
            btn6.Text = @"";
            btn7.Text = @"";
            btn8.Text = @"";
            btn9.Text = @"";
            CurrPlName.Text = @"Current player: " + PlayerXName;
            for (var i = 0; i < GameData.Length; i++)
            {
                GameData[i] = 0;
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Connect(tbIP.Text.Split(':')[0], Convert.ToInt32(tbIP.Text.Split(':')[1]));

            Button currBtn = sender as Button;
            if (currBtn != null && currBtn.Text.Length == 0)
            {
                if (CurrPlName.Text == @"Current player: " + PlayerXName)
                {
                    currBtn.Text = @"X";
                    int currBtnNr;
                    int.TryParse(currBtn.Name.Substring(3, 1), out currBtnNr);
                    GameData[currBtnNr - 1] = 1;
                    //CurrPlName.Text = @"Current player: " + Player0Name;
                    sock.Send(Encoding.ASCII.GetBytes(currBtnNr.ToString()));
                    if (Gamecheck() == 1)
                    {
                        MessageBox.Show(PlayerXName + @" win!");
                        ResetGame();
                    }
                }
                else
                {
                    currBtn.Text = @"0";
                    int currBtnNr;
                    int.TryParse(currBtn.Name.Substring(3, 1), out currBtnNr);
                    GameData[currBtnNr - 1] = 4;
                    //CurrPlName.Text = @"Current player: " + PlayerXName;
                    if (Gamecheck() == 2)
                    {
                        MessageBox.Show(Player0Name + @" win!");
                        ResetGame();
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CurrPlName.Text = @"Current player: " + PlayerXName;
        }
    }
}
