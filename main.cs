using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace Music_Player
{
    public partial class main : Form
    {
        List<SongModel> SongList = new List<SongModel>();
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        System.Timers.Timer timer = new System.Timers.Timer();
        public main()
        {
            InitializeComponent();

            player.settings.volume = 100; // 0-100
            timer.Interval = 100;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timerEvent);
            timer.Start();

            // 加载字体图标库
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile("./Resources/iconfont.ttf");
            // 给按钮指定图标
            this.button2.Text = "\ue6a7";
            this.button2.Font = new Font(pfc.Families[0], 13);

            button3.Text = "\ue69d";
            button3.Font = new Font(pfc.Families[0], 13);

            button4.Text = "\ue694";
            button4.Font = new Font(pfc.Families[0], 13);

            button5.Text = "\ue693";
            button5.Font = new Font(pfc.Families[0], 13);

            button6.Text = "\ue695";
            button6.Font = new Font(pfc.Families[0], 13);

            // 初始化歌曲列表数据
            // 获取Json文件数据
            var jsonStr = System.IO.File.ReadAllText("./songs.json");
            // 反序列化 ：从Json到List集合的过程
            SongList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SongModel>>(jsonStr);
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.DataSource = SongList;

            // 进度条
            this.label5.Width = 0;
        }

        private void timerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (this.player.playState.Equals(WMPLib.WMPPlayState.wmppsPlaying))
                {
                    setLabelProgress();
                }
            }
            catch (Exception)
            {
            }
        }

        // 事件处理器
        // 修改窗体控件 - 创建 使用Invoke
        private void setLabelProgress()
        {
            Action invokeAction = new Action(setLabelProgress);
            if (this.InvokeRequired)
            {
                this.Invoke(invokeAction);
            }
            else
            {
                double _mWidth = this.player.controls.currentPosition / this.player.currentMedia.duration * (double)this.label4.Width;
                this.label5.Width = (int)Math.Ceiling(_mWidth);
                this.label3.Text = $"{this.player.controls.currentPositionString}/{this.player.currentMedia.durationString}";
            }
        }

        #region 窗口移动
        private Point mPoint;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }
        #endregion

        #region 画圆
        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            this.DrawCircle(e.Graphics, new SolidBrush(Color.FromArgb(255, 0, 0, 10)),
                (sender as Control).ClientRectangle);

        }
        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            this.DrawCircle(e.Graphics, Brushes.White, ((Control)sender).ClientRectangle);
        }

        private void DrawCircle(Graphics graphics, Brush brush, Rectangle rectangle)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.FillEllipse(brush, rectangle);
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int _playModel = 0;
        // 播放模式
        private void button2_Click(object sender, EventArgs e)
        {
            _playModel = (_playModel + 1) % 3;
            switch (_playModel)
            {
                case 0:
                    (sender as Button).Text = "\ue6a7";
                    break;
                case 1:
                    (sender as Button).Text = "\ue6a6";
                    break;
                case 2:
                    (sender as Button).Text = "\ue6a4";
                    break;
                default:
                    (sender as Button).Text = "\ue6a7";
                    break;
            }
        }

        int _playState = 0;
        // 播放 | 暂停
        private void button5_Click(object sender, EventArgs e)
        {
            if (this._playState == 0)
            {
                this.play(false);
            }
            else
            {
                this._playState = 0;
                (sender as Button).Text = "\ue693";
                this.player.controls.pause();
            }
        }

        private void play(bool flag)
        {
            this._playState = 1;
            this.button5.Text = "\ue690";

            SongModel songModel = SongList[_playIndex];

            if (flag || this.player.playState != WMPPlayState.wmppsPaused)
            {
                this.player.URL = this.SongList[_playIndex].SongPath;
            }
            this.player.controls.play();

            // 歌手
            this.label1.Text = songModel.SongName;
            this.label2.Text = songModel.Singer;

            // 列表重新选中
            this.dataGridView1.ClearSelection();
            this.dataGridView1.CurrentCell = null;
            this.dataGridView1.Rows[_playIndex].Selected = true;

            this.player.PlayStateChange += Player_PlayStateChange;
        }

        // 当前播放结束后，继续播放下一首
        private void Player_PlayStateChange(int NewState)
        {
            try
            {
                if(NewState == 8)
                {
                    Action invokeAction = new Action(setLabelProgress);
                    if (this.InvokeRequired)
                    {
                        this.Invoke(invokeAction);
                    }
                    else
                    {
                        this.button6.PerformClick();
                    }
                }
            }
            catch(Exception)
            {
            }
        }

        int _playIndex = 0;
        Random _random = new Random();
        // 上一首
        private void button4_Click(object sender, EventArgs e)
        {
            int _lastIndex = this._playIndex - 1;
            this._playIndex = _lastIndex <= 0 ? 0 : _lastIndex;
            this.play(true);
        }
        // 下一首
        private void button6_Click(object sender, EventArgs e)
        {
            if (this._playModel == 0)
            {
                this._playIndex += 1;
                if (this._playIndex >= (this.SongList.Count - 1))
                {
                    this._playIndex = 0;
                }
                this.play(true);
            }
            else if (this._playModel == 1)
            {
                this.play(false);
            }
            else
            {
                _playIndex = _random.Next(0,this.SongList.Count - 1);
                this.play(true);
            }
        }

        // 列表中双击
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _playIndex = e.RowIndex;
            this.play(true);
        }

        // 窗口收缩|展开
        private void button3_Click(object sender, EventArgs e)
        {
            this.Height = 140 + 450 - this.Height;
        }
    }
}
