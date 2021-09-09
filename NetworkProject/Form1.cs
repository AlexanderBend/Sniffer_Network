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
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;

namespace NetworkProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //MessageBox.Show("Для более корректной работы рекумендуется запускать от имени администратора");


        }
        Socket socketz = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        byte[] bytedata = new byte[4096];
        IPAddress myip;
        Boolean started = false;
        Size sizediff;
        Boolean formloaded = false;
        IPAddress FilterIPAddress = new IPAddress(0);
        Boolean FilterIP;
        System.Net.NetworkInformation.NetworkInterface[] mycomputerconnections;
        String stringz;
        String typez;
        IPAddress ipfrom;
        IPAddress ipto;
        UInt16 destinationport;
        UInt16 sourceport;
        SaveFileDialog sfd = new SaveFileDialog();


        private void Form1_Load(object sender, EventArgs e)
        {
            sizediff.Height = this.Height - DGV.Height;
            sizediff.Width = this.Width - DGV.Width;
            formloaded = true;
            mycomputerconnections = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();


            foreach (System.Net.NetworkInformation.NetworkInterface item in mycomputerconnections)
            {
                comboBox1.Items.Add(item.Name);
            }



        }

        private void OnReceive(IAsyncResult asyncResult)
        {
            if (started == true)
            {

                uint readlength = BitConverter.ToUInt16(Byteswap(bytedata, 2), 0);
                sourceport = BitConverter.ToUInt16(Byteswap(bytedata, 22), 0);
                destinationport = BitConverter.ToUInt16(Byteswap(bytedata, 24), 0);

                if (bytedata[9] == 6)
                {
                    typez = "TCP";
                }
                else if (bytedata[9] == 17)
                {
                    typez = "UDP";
                }
                else
                    typez = "???";

                ipfrom = new IPAddress(BitConverter.ToUInt32(bytedata, 12));
                ipto = new IPAddress(BitConverter.ToUInt32(bytedata, 16));

                if (ipfrom.Equals(myip) == true || ipto.Equals(myip) == true)
                {
                    if (FilterIP == false || (FilterIP == true && (FilterIPAddress.Equals(ipfrom) || FilterIPAddress.Equals(ipto))))
                    {
                        stringz = "";
                        for (uint i = 26; i < readlength; i++)
                        {
                            if (Char.IsLetterOrDigit(Convert.ToChar(bytedata[i])) == true)
                            {
                                stringz = stringz + Convert.ToChar(bytedata[i]);
                            }
                            else
                            {
                                stringz = stringz + "";
                            }
                        }
                        DGV.Invoke(new MethodInvoker(DGVUpdate));

                    }
                }

            }

            socketz.BeginReceive(bytedata, 0, bytedata.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);


        }

        private void DGVUpdate()
        {
            
            DateTime dt = DateTime.Now;

            string[] row = new string[] { dt.ToString(), ipfrom.ToString() + ":" + sourceport, ipto.ToString() + ":" + destinationport, typez, stringz };

            //DGV.Rows.Add(row);
            if (row[1] != string.Empty)
            {
                DGV.Rows.Add(row);
            }

        }

        private byte[] Byteswap(byte[] bytez, uint index)
        {
            byte[] result = new byte[2];
            result[0] = bytez[index + 1];
            result[1] = bytez[index];
            return result;
        }

        private void BindSocket()
        {
            //    try
            //  {
            socketz.Bind(new IPEndPoint(myip, 0));
            socketz.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            byte[] bytrue = new byte[] { 1, 0, 0, 0 };
            byte[] byout = new byte[] { 1, 0, 0, 0 };
            socketz.IOControl(IOControlCode.ReceiveAll, bytrue, byout);
            socketz.Blocking = false;
            Array.Resize(ref bytedata, socketz.ReceiveBufferSize);
            socketz.BeginReceive(bytedata, 0, bytedata.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            comboBox1.Enabled = false;

            //  }
            //   catch (Exception ex)
            //   {
            //      comboBox1.BackColor = Color.Red;
            //   }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            for (int i = 0; i <= mycomputerconnections[comboBox1.SelectedIndex].GetIPProperties().UnicastAddresses.Count - 1; i++)
            {
                AddressFamily adresdeneme, adreskontrol;
                adreskontrol = AddressFamily.InterNetwork;
                adresdeneme = mycomputerconnections[comboBox1.SelectedIndex].GetIPProperties().UnicastAddresses[i].Address.AddressFamily;

                if (adresdeneme == adreskontrol)
                {
                    myip = mycomputerconnections[comboBox1.SelectedIndex].GetIPProperties().UnicastAddresses[i].Address;
                    BindSocket();
                }

            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            if (started == true)
            {
                button1.Text = "Старт";

                started = false;
            }
            else
            {
                button1.Text = "Стоп";
                started = true;
            }

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

            try
            {
                if (textBox1.Text != null)
                {
                    FilterIPAddress = IPAddress.Parse(textBox1.Text);
                    FilterIP = true;
                    textBox1.BackColor = Color.LimeGreen;
                }
                else
                {
                    FilterIP = false;
                    textBox1.BackColor = Color.White;
                }
            }
            catch (Exception)
            {
                FilterIP = false;
                textBox1.BackColor = Color.White;
            }

        }

        private void DGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            richTextBoxData.Text = Convert.ToString(this.DGV.CurrentRow.Cells[4].Value);
        }
        //Поиск
        private void Button2_Click(object sender, EventArgs e)  //Поиск
        {
            for (int i = 0; i < DGV.RowCount; i++)
            {
                DGV.Rows[i].Selected = false;
                for (int j = 0; j < DGV.ColumnCount; j++)
                    if (DGV.Rows[i].Cells[j].Value != null)
                        if (DGV.Rows[i].Cells[j].Value.ToString().Contains(textBox2.Text))
                        {
                            DGV.Rows[i].Selected = true;
                            break;
                        }
            }
        }

        //Сохранение
        private void Button3_Click(object sender, EventArgs e)
        {

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (TextWriter tw = new StreamWriter(sfd.FileName + ".txt"))
                {
                    for (int i = 0; i < DGV.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < DGV.Columns.Count; j++)
                        {
                            tw.Write($"{DGV.Rows[i].Cells[j].Value.ToString()}");

                            if (j != DGV.Columns.Count - 1)
                            {
                                tw.Write(",");
                            }
                        }

                    }
                }
            }


        }
        //Проверка BlackListIP
        private void CheckBtnIP_Click(object sender, EventArgs e)
        {
            try
            {
                string[] array = File.ReadAllLines(@"blacklist_for_ddos.txt").Where(line => line != string.Empty).ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    DGV.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    if (DGV.Rows[i].Cells[1].Value.ToString().Contains(array[i]))
                    {
                        DGV.Rows[i].Cells[1].Style.BackColor = Color.Red;
                        break;
                    }
                  
                }
            }
            catch { }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {


                Ping png = new Ping();
                PingReply replict = png.Send(textBox1.Text);
                if (replict.Status == IPStatus.Success)
                {
                    richTextBoxData.Text = "PING OK\n";
                    richTextBoxData.AppendText("Start log...\r\n" + "Адрес: " + replict.Address.ToString() + "\r\nВремя: " + replict.RoundtripTime + "\r\nВремя жизни пакета:" + replict.Options.Ttl + "\r\nФрагментирование: " + replict.Options.DontFragment + "\r\nРазмер пакета: " + replict.Buffer.Length + "\r\nEnd Log...\r\n");
                }
                else
                {
                    richTextBoxData.Text = "PING FALSE";
                }
            }
            catch { }




        }
    }
}
