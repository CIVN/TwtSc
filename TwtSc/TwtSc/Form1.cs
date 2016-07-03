using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CoreTweet;
using static CoreTweet.OAuth;
using static TwtSc.Properties.Settings;

namespace TwtSc
{
	public partial class Form1 : Form
	{
		private HotKey hotKey;

		private Tokens token;
		private OAuthSession session;

		private string PATH = Environment.CurrentDirectory + "\\" + "ScreenShots";

		private const string CONSUMER_KEY = "ARvucpPIcosQBdehQKKH93HhT";
		private const string CONSUMER_SECRET = "5LJh1LNuUrx6C8bXdxBceFYYQZdM5YeHodR9mym3Q1zfix5IG3";

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			session = Authorize(CONSUMER_KEY, CONSUMER_SECRET);

			if (!(Default.AccessToken == "" && Default.AccessTokenSecret == "" && Default.userID == 0 && Default.screenName == ""))
			{
				token = Tokens.Create(CONSUMER_KEY, CONSUMER_SECRET, Default.AccessToken, Default.AccessTokenSecret, Default.userID, Default.screenName);

				if (token != null)
				{
					ShowInfomations(token);
					HideControls();
				}
			}

			hotKey = new HotKey(MOD_KEY.SHIFT, Keys.PrintScreen);
			hotKey.HotKeyPush += new EventHandler(hotKey_HotKeyPush);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			hotKey.Dispose();
		}

		private void hotKey_HotKeyPush(object sender, EventArgs e)
		{
			if (!(Directory.Exists(PATH)))
			{
				var info = Directory.CreateDirectory(PATH);

				if (info.Exists == false)
				{
					MessageBox.Show("ファイルを保存するフォルダが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			var g = Graphics.FromImage(bmp);
			g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp.Size);
			g.Dispose();

			var file = PATH + "\\" + DateTime.Now.ToFileTime() + ".png";

			bmp.Save(file, ImageFormat.Png);

			var player = new System.Media.SoundPlayer(Properties.Resources.シャッター音);
			player.Play();

			token.Statuses.UpdateWithMediaAsync(status => "", media => new FileInfo(file));
		}

		private void HideControls()
		{
			menuStrip1.Items.Remove(認証ToolStripMenuItem);
			label2.Hide();
			textBox1.Hide();
			button1.Hide();
		}

		private void ShowInfomations(Tokens token)
		{
			label1.Text = "@" + token.ScreenName;
			pictureBox1.ImageLocation = token.Users.Show(token.ScreenName).ProfileImageUrl;
		}

		private void 認証ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(session.AuthorizeUri.OriginalString);
		}

		private void リセットToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Default.Reset();
			Default.Save();

			token = null;

			Application.Restart();
		}

		private void twitteroktmstyleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("https://twitter.com/oktm_style/");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var pin = textBox1.Text;

			try
			{
				token = session.GetTokens(pin);
			}

			catch
			{
				MessageBox.Show("認証に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (token != null)
			{
				MessageBox.Show("認証に成功しました。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);

				Default.AccessToken = token.AccessToken;
				Default.AccessTokenSecret = token.AccessTokenSecret;
				Default.userID = token.UserId;
				Default.screenName = token.ScreenName;
				Default.Save();

				ShowInfomations(token);
				HideControls();
			}
		}
	}
}