using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yama.Mp3AttributeEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string _selectedPath = string.Empty;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void btn_browse_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			System.Windows.Forms.DialogResult result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK && !String.IsNullOrEmpty(dialog.SelectedPath))
			{
				_selectedPath = dialog.SelectedPath;
				txt_selectedFolderPath.Text = _selectedPath;
				LogMsg("Selected Path: " + _selectedPath);
			}
			else
			{
				LogMsg("Please select a valid folder path");
			}
		}

		private void LogMsg(string txt)
		{
			lb_log.Items.Insert(0, txt);
		}

		private void btn_start_Click(object sender, RoutedEventArgs e)
		{
			btn_start.IsEnabled = false;
			busyIndicator.IsBusy = true;
			if (!String.IsNullOrEmpty(_selectedPath))
			{
				ChangeAlbumAttribute(_selectedPath);
			}
			else
			{
				busyIndicator.IsBusy = false;
				btn_start.IsEnabled = true;
				LogMsg("You have to select a folder before you click start.");
				MessageBox.Show("Please select a valid folder before you click start.", "Check", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		private void ChangeAlbumAttribute(string selectedPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(selectedPath);
			try
			{
				var mp3Files = directoryInfo.GetFiles("*.mp3", SearchOption.AllDirectories);
				LogMsg(mp3Files.Length.ToString() + " mp3 file(s) found in " + selectedPath);

				int modifiedFileCount = 0;
				
				// Modify attribute
				Task T = new Task(() =>
					{	
						foreach (FileInfo file in mp3Files)
						{
							try
							{
								TagLib.File f = TagLib.File.Create(file.FullName);
								if (f.Tag.Album != file.Directory.Name && !file.IsReadOnly)
								{
									f.Tag.Album = file.Directory.Name;
									f.Save();
									modifiedFileCount++;
								}
							}
							catch { }
						}
					});

				// display result
				Task T2 = T.ContinueWith((antecedent) =>
					{
						LogMsg(modifiedFileCount.ToString() + " file(s) have been fixed.");
						busyIndicator.IsBusy = false;
						btn_start.IsEnabled = true;
						MessageBox.Show("Successfully Completed!");
					},
					TaskScheduler.FromCurrentSynchronizationContext()
					);
				T.Start();
			}
			catch (Exception ex)
			{
				LogMsg(ex.Message);
			}
		}
	}
}
