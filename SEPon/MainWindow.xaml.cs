using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Runtime;
using System.Text.Json;
using System.IO;
using System.Linq;
using NAudio.Wave;
using Microsoft.Win32;

namespace SEPon
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Button> seButtons = new List<Button>();
        SEList list = new SEList();
        WaveOut waveOut;

        public MainWindow()
        {
            InitializeComponent();
            waveOut = new WaveOut();
            Init();
        }

        private void Update(int index)
        {
            File.WriteAllText("config/config.json", JsonSerializer.Serialize(list));
            setButtonContent(seButtons[index], index);
        }

        private void Init()
        {
            fillList();
            LoadSettings();
            for (int i = 0; i < MainG.RowDefinitions.Count - 1; i++)
            {
                var button = new Button();
                button.Name = "se" + i;
                button.Margin = new Thickness(5);
                button.AllowDrop = true;
                button.Click += buttonClick;
                button.PreviewDragOver += ButtonsPreviewDragOver;
                button.Drop += ButtonsDrop;
                button.FontSize = 20;
                button.Focusable = false;
                seButtons.Add(button);
                setButtonContent(button, i);
                Grid.SetRow(button, i + 1);
                MainG.Children.Add(button);
            }
        }

        private void setButtonContent(Button button, int index)
        {
            try
            {
                if (File.Exists(list.SePathList[index]))
                {
                    button.Content = "効果音" + index + " : " + System.IO.Path.GetFileName(list.SePathList[index]);
                }
                else
                {
                    button.Content = "効果音" + index + " : 未設定";
                }
            }
            catch
            {
                button.Content = "効果音" + index + " : 未設定";
            }
            
        }

        private void LoadSettings()
        {
            try
            {
                using (var reader = new StreamReader("config/config.json"))
                {
                    string s = reader.ReadLine();
                    list = JsonSerializer.Deserialize<SEList>(s);
                }
            }
            catch
            {
                SEList selist = new SEList();
                selist.SePathList = new List<string>(MainG.RowDefinitions.Count - 1);
                for (int i = 0; i < MainG.RowDefinitions.Count - 1; i++)
                {
                    selist.SePathList.Insert(i, "null" + i);
                }
                File.WriteAllText("config/config.json", JsonSerializer.Serialize(selist));
            }
        }

        private int getButtonIndex(Button button)
        {
            string id = button.Name;
            string a = id.Substring(2);
            return int.Parse(a);
        }

        public void buttonClick(object sender, EventArgs e)
        {
            Button sebutton;
            int pressedButIndex;
            if (sender is Button)
            {
                sebutton = (Button) sender;
                pressedButIndex = getButtonIndex(sebutton);
            }
            else
            {
                throw new ArgumentException("senderの型がButtonではありません");
            }

            if (settingMode.IsChecked == true)
            {
                //まあ要するにチェックがついてたら設定するってことよ
                Setting(pressedButIndex);
                return;
            }

            if (list.SePathList.Count > pressedButIndex && File.Exists(list.SePathList[pressedButIndex])){
                string path = list.SePathList[pressedButIndex];
                Play(path);
            }
        }

        private void Play(string path)
        {
            AudioFileReader reader = new AudioFileReader(path);
            waveOut.Init(reader);
            waveOut.Play();
        }

        private void Setting(int index)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "音声ファイル(*.mp3; *.wav)|*.mp3; *.wav|すべてのファイル(*.*)|*.*";

            if (fileDialog.ShowDialog() == true)
            {
                list.SePathList[index] = fileDialog.FileName;
                Update(index);
            }
        }

        private void fillList()
        {
            list.SePathList = new List<string>(MainG.RowDefinitions.Count - 1);
            for (int i = 0; i < MainG.RowDefinitions.Count - 1; i++)
            {
                list.SePathList.Insert(i, "null" + i);
            }
        }

        private void ButtonsDrop(object sender, DragEventArgs e)
        {
            var fi = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fi == null) return;
            if (!(sender is Button))
            {
                throw new ArgumentException("senderの型がButtonではありません");
            }

            if (System.IO.Path.GetExtension(fi[0]) == ".mp3" || System.IO.Path.GetExtension(fi[0]) == ".wav")
            {
                int index = getButtonIndex((Button)sender);
                list.SePathList[index] = fi[0];
                Update(index);
            }
        }

        private void ButtonsPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
               
            }
            e.Handled = true;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape/* || e.Key == System.Windows.Input.Key.S*/)
            {
                waveOut.Stop();
                return;
            }

            string a = e.Key.ToString();

            try {
                int ind;
                if (a.Length == 2)
                {
                    string aint = a.Substring(1);
                    ind = int.Parse(aint);
                }
                else if (a.Length == 7)
                {
                    string aint = a.Substring(6);
                    ind = int.Parse(aint);
                }
                else return;

                if (list.SePathList.Count > ind && File.Exists(list.SePathList[ind]))
                {
                    string path = list.SePathList[ind];
                    Play(path);
                }
            }
            catch(NAudio.MmException)
            {
                MessageBox.Show("再生上のエラーが発生しました");
            }
            catch
            {
                Console.WriteLine(a);
            }
        }

        private void StopBut_Click(object sender, RoutedEventArgs e)
        {
            waveOut.Stop();
        }
    }
}
