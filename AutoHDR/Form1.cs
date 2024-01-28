
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AutoHDR
{
    public partial class AutoHDR : Form
    {
        private NotifyIcon notifyIcon;
        public AutoHDR()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoHDR));
            // 创建 NotifyIcon 控件
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon"))); // 设置图标文件路径
            notifyIcon.Text = "AutoHDR"; // 设置鼠标悬停时显示的文本
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            // 添加右键菜单项
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            openMenuItem.Click += OpenMenuItem_Click;
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenuStrip.Items.Add(openMenuItem);
            contextMenuStrip.Items.Add(exitMenuItem);
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // 显示任务栏小图标
            notifyIcon.Visible = true;
            InitializeComponent();
            LoadGame();
            panel.AllowDrop = true;
            panel.DragEnter += Panel_DragEnter;
            panel.DragDrop += Panel_DragDrop;
            FormClosing += AutoHDR_FormClosing;
            textBox1.KeyPress += TextBox1_KeyPress;
            textBox2.KeyPress += TextBox2_KeyPress;
            WindowState = FormWindowState.Minimized;
            Visible = false;
            ShowInTaskbar = false;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && this.WindowState == FormWindowState.Minimized)//当程序是最小化的状态时显示程序页面
                {
                    this.WindowState = FormWindowState.Normal;
                }
                this.Activate();
                this.Visible = true;
                this.ShowInTaskbar = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // 设置 WS_EX_TOOLWINDOW 扩展样式
                return cp;
            }
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || char.IsDigit(e.KeyChar)))
            {
                e.Handled = true;
            }
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || char.IsDigit(e.KeyChar)))
            {
                e.Handled = true;
            }
        }

        private void AutoHDR_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { 
                e.Cancel = true; this.Visible = false;
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Minimized)//当程序是最小化的状态时显示程序页面
                { 
                    this.WindowState = FormWindowState.Normal;
                }
                this.Activate();
                this.Visible = true;
                this.ShowInTaskbar = true;
            } catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message); 
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Program.working = false;
            notifyIcon.Visible = false;
            this.Dispose();
            this.Close();
            Application.Exit();
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();       //获得路径
            AddGame(path);
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("drop");
            if (((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString().EndsWith("exe"))
                e.Effect = DragDropEffects.Link;                                                              //重要代码：表明是所有类型的数据，比如文件路径
            else
                e.Effect = DragDropEffects.None;

        }

        void LoadGame()
        {
            foreach (string name in Program.programName)
            {
                panel.Controls.Add(new Game(name));
            }
            textBox1.Text = Program.holdtime.ToString();
            textBox2.Text = Program.waittime.ToString();
        }
        void AddGame(string name)
        {
            if (name.EndsWith(".exe"))
            {
                int line = name.LastIndexOf("\\");
                string tmp = name.Substring(line + 1, name.Length - 5 - line);
                name = tmp;
            }
            if (Program.programName.Contains(name))
                return;
            Program.programName.Add(name);
            panel.Controls.Add(new Game(name));
        }

        public void DeleteGame(Game game)
        {
            panel.Controls.Remove(game);
            Program.programName.Remove(game.Text);
        }

        private void Button_Add(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = dialog.FileName;
                Console.WriteLine(path);
                if (!path.EndsWith("exe"))
                    return;
                AddGame(path);
            }
        }

        private void Button_Save(object sender, EventArgs e)
        {
            Program.Write();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Program.holdtime = Convert.ToInt32(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Program.waittime = Convert.ToInt32(textBox2.Text);
        }
    }

    public class Game : Label
    {
        public Game(string name)
        {
            Text = name;
            AutoSize = false;
            Width = 230;
            Height = 20;
            ContextMenuStrip = new RightMenu();
            BorderStyle = BorderStyle.FixedSingle;
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        public void Delete()
        {
            AutoHDR autoHDR = (AutoHDR)Form.ActiveForm;
            autoHDR.DeleteGame(this);
        }
    }

    public class RightMenu:ContextMenuStrip
    {
        public RightMenu()
        {
            ToolStripMenuItem delete = new ToolStripMenuItem("删除");
            Items.Add(delete);
            delete.Click += Delete_Click;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            (((sender as ToolStripMenuItem).Owner as RightMenu).SourceControl as Game).Delete();
        }
    }
}
