using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Microsoft.DirectX.DirectInput;
using System.IO;


namespace JoystickID
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadPostApp();
            LoadSetting();
            UpdateList();
            if (CheckOrder2() == true)
            {
                RunPostApp(1);
                this.Close();
                return;
            }
            RunPostApp(2);

            timer1.Tick += Timer1_Tick;
            timer1.Interval = 1000;
            timer1.Start();
        }

        public Dictionary<int, string> _setting = new Dictionary<int, string>(); // device enum index, device guid
        private string _filename = "setting.ini";


        public void LoadSetting()
        {
            if (File.Exists(_filename) == false)
                return;

            string temp = File.ReadAllText(_filename);
            _setting = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, string>>(temp);

        }

        public void SaveSetting()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                string desc = item.ProductGuid.ToString();
                _setting[i] = desc;
            }
            string temp = Newtonsoft.Json.JsonConvert.SerializeObject(_setting);
            File.WriteAllText(_filename, temp);
        }

        private string _postappfilename = "postapp.ini";
        private string _postapp1;
        private string _postapp2;

        public void LoadPostApp()
        {
            if (File.Exists(_postappfilename) == false)
            {
                File.WriteAllText(_postappfilename, "");
                return;
            }

            var lines = File.ReadAllLines(_postappfilename);
            if (lines.Length > 0)
                _postapp1 = lines[0];
            if (lines.Length > 1)
                _postapp2 = lines[1];
        }

        public void RunPostApp(int index)
        {
            string temp = null;
            switch (index)
            {
                case 1:
                    temp = _postapp1;
                    break;

                case 2:
                    temp = _postapp2;
                    break;
            }

            if (string.IsNullOrWhiteSpace(temp) == true)
                return;

            Process.Start(temp);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateList();
            if (CheckOrder2() == true)
            {
                this.Close();
            }
        }

        private IList<SharpDX.DirectInput.DeviceInstance> _list;
        private void UpdateList()
        {
            listBox1.Items.Clear();
            var di = new SharpDX.DirectInput.DirectInput();
            _list = di.GetDevices(SharpDX.DirectInput.DeviceClass.GameControl, SharpDX.DirectInput.DeviceEnumerationFlags.AttachedOnly);

            foreach (var item in _list)
            {
                string temp = item.ProductName + item.ProductGuid.ToString();
                Debug.WriteLine(temp);
                listBox1.Items.Add(temp);
            }

        }

        private bool CheckOrder()
        {
            int itemCount = _list.Count;
            if (itemCount < 3)
                return false;

            if (_list[0].ProductName.ToLower().Contains("pedal") == false)
                return false;

            if (_list[1].ProductName.ToLower().Contains("throttle") == false)
                return false;

            if (_list[2].ProductName.ToLower().Contains("stick") == false)
                return false;

            return true;
        }

        private bool CheckOrder2()
        {
            if (_list.Count < 3)
                return false;

            for (int i = 0; i < _list.Count; i++)
            {
                string listGUID = _list[i].ProductGuid.ToString();
                string settingGUID = _setting[i];
                if (listGUID != settingGUID)
                    return false;
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveSetting();
            MessageBox.Show("save done");
        }
    }
}
