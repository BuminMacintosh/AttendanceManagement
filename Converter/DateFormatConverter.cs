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
	public class DateFormatConverter : IValueConverter
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

			return ("En" == (string)Application.Current.Properties["JpEnMode"])
					? ((DateTime)value).ToString("MM/dd/yyyy")
					: ((DateTime)value).ToString("yyyy年MM月dd日");
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
