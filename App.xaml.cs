using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using AttendanceManagement.DAL;
using AttendanceManagement.Dialog;

namespace AttendanceManagement
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			LocalDataBaseDataSet dac = new LocalDataBaseDataSet();

			dac.Load();

			Application.Current.Properties["DAC"] = dac;
			Application.Current.Properties["JpEnMode"] = "Jp";
			Application.Current.Properties["SelectLocationId"] = 0;

			AttendanceManagementWindow main = new AttendanceManagementWindow();
			//ScheduleEntryDialog main = new ScheduleEntryDialog();
			this.MainWindow = main;

			main.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			LocalDataBaseDataSet dac
				= Application.Current.Properties["DAC"] as LocalDataBaseDataSet;

			dac.Save();
		}
	}
}
