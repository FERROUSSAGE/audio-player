using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly System.Windows.Forms.FolderBrowserDialog oBD = new System.Windows.Forms.FolderBrowserDialog();
        private static readonly WebClient client = new WebClient();
        private static readonly OpenFileDialog oFD = new OpenFileDialog();
        private static readonly MediaPlayer player = new MediaPlayer { Volume = 0.5 };
        private static List<string> songs = new List<string>();
        private static List<string> songMassArray = new List<string>();
        private static string folderPath = String.Empty;
        private static readonly string filterPattern = ".";
        public static DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += Timer_Tick;

            UrlInput.KeyDown += UrlInput_KeyDown;
            btnMuteMusic.Click += BtnMuteMusic_Click;
            btnPauseMusic.Click += BtnPauseMusic_Click;
            btnRewindRight.Click += BtnRewindRight_Click;
            btnRewindLeft.Click += BtnRewindLeft_Click;
            btnStopMusic.Click += BtnStopMusic_Click;
            btnPlayMusic.Click += BtnPlayMusic_Click;
            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            sliderDuration.ValueChanged += SliderDuration_ValueChanged;
            playListMusic.MouseDoubleClick += PlayListMusic_MouseDoubleClick;
            btnDeleteSingleSong.Click += BtnDeleteSingleSong_Click;
            HideBtn.Click += HideBtn_Click;
            CloseBtn.Click += CloseBtn_Click;
            btnLoadSingleSong.Click += BtnLoadSingleSong_Click;
            btnMusicLoad.Click += Btn_Click;
        }

        private void BtnDeleteSingleSong_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UrlInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {
                    if (File.Exists(Environment.CurrentDirectory + @"\downloadMusic\test.mp3"))
                        File.Delete(Environment.CurrentDirectory + @"\downloadMusic\test.mp3");
                }
                catch (Exception ex) { }
                Task.Delay(2);
                playMusic(UrlInput.Text, true);
            }
        }

        private void BtnLoadSingleSong_Click(object sender, RoutedEventArgs e)
        {
            MusicLoad(true);
        }

        private void BtnMuteMusic_Click(object sender, RoutedEventArgs e)
        {
            player.IsMuted = !player.IsMuted;
        }

        private void BtnPauseMusic_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            if (player.CanPause)
            {
                player.Pause();
            }

        }

        private void BtnRewindRight_Click(object sender, RoutedEventArgs e)
        {
            player.Position += TimeSpan.FromSeconds(10);
        }

        private void BtnRewindLeft_Click(object sender, RoutedEventArgs e)
        {
            player.Position -= TimeSpan.FromSeconds(10);
        }

        private void BtnStopMusic_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            timer.Stop();
        }

        private void BtnPlayMusic_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(UrlInput.Text))
            {
                playMusic(UrlInput.Text, true);
            }
            else
            {
                timer.Start();
                player.Play();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            sliderDuration.Value = player.Position.TotalSeconds;
            durationStart.Text = player.Position.ToString(@"mm\:ss");
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = volumeSlider.Value;
        }

        private void SliderDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Pause();
            timer.Stop();
            player.Position = TimeSpan.FromSeconds(sliderDuration.Value);
            player.Play();
            timer.Start();
        }

        private void PlayListMusic_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string selectedMusic = playListMusic.SelectedItem.ToString();
            string path = songs.Where(x => x.Contains(selectedMusic)).First();
            if (playListMusic.SelectedItem == null) return;

            playMusic(path, false);
            currentPlayMusic.Text = $"Сейчас играет: {selectedMusic}";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void HideBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            MusicLoad(false);
        }

        private void MusicLoad(bool isSingle)
        {
            try
            {
                string[] song = null;
                string[] itemLastArray = null;

                if (isSingle == true)
                {
                    if (oFD.ShowDialog() != null)
                    {
                        string path = oFD.FileName;
                        itemLastArray = path.Replace("\\", "/").Split('/');
                        songMassArray.Add(itemLastArray.Last());
                        songs.Add(path);
                    }
                }
                else
                {
                    if (oBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        folderPath = oBD.SelectedPath;
                        song = Directory.GetFiles(folderPath, filterPattern).ToArray();
                        foreach (var item in song)
                        {
                            itemLastArray = item.Replace("\\", "/").Split('/');
                            songMassArray.Add(itemLastArray.Last());
                            songs.Add(item);
                        }
                    }
                }

                playListMusic.ItemsSource = songMassArray.ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void playMusic(string path, bool isUrl)
        {
            try
            {

                player.MediaOpened += Player_MediaOpened;
                player.MediaEnded += Player_MediaEnded;
                player.MediaFailed += Player_MediaFailed;

                if (isUrl == true)
                {
                    Task.Delay(5);
                    client.DownloadFileAsync(new Uri(UrlInput.Text.Trim()), Environment.CurrentDirectory + @"\downloadMusic\test.mp3");
                    client.DownloadFileCompleted += (s, e) =>
                    {
                        player.Open(new Uri(Environment.CurrentDirectory + @"\downloadMusic\test.mp3", UriKind.Relative));
                        currentPlayMusic.Text = "Сейчас играет песня из интернета";
                        UrlInput.Text = String.Empty;
                        player.Play();
                        timer.Start();
                    };
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        downloadProgress.Value = e.ProgressPercentage;
                    };
                }
                else
                {
                    player.Open(new Uri(path, UriKind.Relative));
                    player.Play();
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                MessageBox.Show($"{ ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            timer.Stop();
            int buffer = 0;
            for (int i = 0; i < songs.Count - 1; i++)
            {
                playMusic(songs[i + 1], false);
                buffer += 1;
                timer.Start();
                if (i == buffer - 1)
                {
                    MessageBox.Show("Текущий плейлист был полностью проигран");
                    buffer = 0;
                    player.Stop();
                    timer.Stop();
                }
            }
        }

        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            timer.Stop();
            currentPlayMusic.Text = "Ошибка в загрузке mp3";
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                sliderDuration.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
                durationEnd.Text = player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }
    }
}
