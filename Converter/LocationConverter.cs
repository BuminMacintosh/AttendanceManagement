using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Data;
using AttendanceManagement.DAL;

namespace AttendanceManagement.Converter
{
	/// <summary>
	/// 場所コンバータクラス
	/// </summary>
	public class LocationConverter : IValueConverter
	{
		/// <summary>
		/// ID値から名称の変換
		/// </summary>
		/// <param name="idParam"></param>
		/// <returns></returns>
		public static string ToString(int idParam)
		{
			LocalDataBaseDataSet dac
				= Application.Current.Properties["DAC"] as LocalDataBaseDataSet;
			LocalDataBaseDataSet.LocationMasterRow row = dac.GetLocation(idParam);

			if (null == row)
			{
				return string.Empty;
			}

			return ("En" == (string)Application.Current.Properties["JpEnMode"]) ? row.EName : row.JName;
		}

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
			
			return ToString(int.Parse(value.ToString()));
		}

		public object ConvertBack(object value,
								  Type targetType,
								  object parameter,
								  CultureInfo culture)
		{
			return string.Empty;
		}

		#endregion
	}
}
