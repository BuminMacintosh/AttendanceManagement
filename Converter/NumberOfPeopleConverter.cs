using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using AttendanceManagement.DAL;
using System.Windows;
using System.Data;

namespace AttendanceManagement.Converter
{
	/// <summary>
	/// 画面表示用テキストコンバータクラス
	/// </summary>
	public class NumberOfPeopleConverter : IValueConverter
	{
		#region IValueConverter メンバ

		public object Convert(object value,
							  Type targetType,
							  object parameter,
							  CultureInfo culture)
		{
			if (null == value || string.Empty == value.ToString())
			{
				return string.Empty;
			}

			LocalDataBaseDataSet dac
				= Application.Current.Properties["DAC"] as LocalDataBaseDataSet;

			//WorkingNoから対象日の人数を抽出する
			return dac.WorkingManagement.Select(string.Format("WorkingNo = {0}", value.ToString())).Length;
		}

		public object ConvertBack(object value,
								  Type targetType,
								  object parameter,
								  CultureInfo culture)
		{
			return value;
		}

		#endregion
	}
}
