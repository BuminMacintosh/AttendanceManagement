using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AttendanceManagement.DAL;
using System.Data;

namespace AttendanceManagement.Dialog
{
	/// <summary>
	/// ScheduleEntryDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ScheduleEntryDialog : Window
	{
		#region メンバフィールド
		private Label _mouseLeftButtonDownCell = null;
		private Label _selectedCell = null;
		private DateTime _targetYearMonth = new DateTime(DateTime.Now.Year,
														 DateTime.Now.Month, 1);
		private LocalDataBaseDataSet.ScheduleInfoRow _targetSchedule = null;
		private bool _isUpdateParam = true;
		#endregion

		#region プロパティ
		private LocalDataBaseDataSet Dac
		{
			get
			{
				return Application.Current.Properties["DAC"] as LocalDataBaseDataSet;
			}
		}

		private string JpEnMode
		{
			get
			{
				return Application.Current.Properties["JpEnMode"] as string;
			}
			set
			{
				Application.Current.Properties["JpEnMode"] = value;
			}
		}
		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ScheduleEntryDialog() : this (null, true)
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="scheduleInfoRowParam"></param>
		public ScheduleEntryDialog(LocalDataBaseDataSet.ScheduleInfoRow scheduleInfoRowParam,
								   bool isUpdatePram)
		{
			this.InitializeComponent();


			if (null != scheduleInfoRowParam)
			{
				this._targetSchedule = scheduleInfoRowParam;
				this._isUpdateParam = isUpdatePram;
				this._targetYearMonth
					= new DateTime(scheduleInfoRowParam.Date.Year,
								   scheduleInfoRowParam.Date.Month, 1);
			}

			this.ShowCalendar();

			this.cmbLocation.ItemsSource = this.Dac.LocationMaster;

			if (null != scheduleInfoRowParam)
			{
				if (isUpdatePram)
				{
					this.btnOK.Content = "Update";
					this.btnOK.Click += this.btnUpdate_Click;
				}
				else
				{
					this.btnOK.Content = "Delete";
					this.btnOK.Click += this.btnDelete_Click;
				}

				this.cmbLocation.SelectedIndex = scheduleInfoRowParam.LocationID - 1;
				this.cmbLocation.IsEnabled = isUpdatePram;
			}
			else
			{
				this.btnOK.Content = "Entry";
				this.btnOK.Click += this.btnEntry_Click;
			}
		}

		#region メンバメソッド
		/// <summary>
		/// カレンダーの指定位置にラベルを追加します
		/// </summary>
		/// <param name="cellParam"></param>
		/// <param name="rowIndexParam"></param>
		/// <param name="columnIndexParam"></param>
		private void AddCellForCalendar(Label cellParam,
										int rowIndexParam,
										int columnIndexParam)
		{
			cellParam.FontSize = 9;
			cellParam.HorizontalContentAlignment = HorizontalAlignment.Center;
			cellParam.VerticalContentAlignment = VerticalAlignment.Center;

			Grid.SetRow(cellParam, rowIndexParam);
			Grid.SetColumn(cellParam, columnIndexParam);

			this.grdCalendar.Children.Add(cellParam);
		}

		/// <summary>
		/// カレンダーを描画します
		/// </summary>
		private void ShowCalendar()
		{
			//カレンダーに張り付いた子要素を全て削除
			this.grdCalendar.Children.Clear();

			//カレンダーの書込先行
			int rowIndex = 0;

			//カレンダーに貼り付けるラベルオブジェクト
			Label cell = new Label();

			//カレンダーのヘッダタイトル
			this.lblCalenderHeader.Content = this._targetYearMonth.ToString("yyyy/MM");

			//曜日ラベルの作成
			foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
			{
				cell = new Label();
				cell.Content = dayOfWeek.ToString().Substring(0, 3);
				cell.Background = this.Resources["DayOfWeekBrush"] as Brush;
				this.AddCellForCalendar(cell, rowIndex, (int)dayOfWeek);
			}

			//日付ラベルへ遷移
			rowIndex++;

			//日付ラベルの作成
			for (DateTime writeDay = this._targetYearMonth;
				 writeDay < this._targetYearMonth.AddMonths(1);
				 writeDay = writeDay.AddDays(1))
			{
				if (1 == rowIndex && this._targetYearMonth.DayOfWeek > writeDay.DayOfWeek)
				{
					continue;
				}

				cell = new Label();
				cell.Background = this.Resources["NonSelectedDayBrush"] as Brush;

				if (this._isUpdateParam)
				{
					cell.MouseLeftButtonDown += this.CalendarCell_MouseLeftButtonDown;
					cell.MouseLeftButtonUp += this.CalendarCell_MouseLeftButtonUp;
				}

				if (null != this._targetSchedule
					&& this._targetSchedule.Date.Date == writeDay.Date)
				{
					cell.Background = this.Resources["SelectedDayBrush"] as Brush;
					this._selectedCell = cell;
				}

				cell.Content = writeDay.Day.ToString();

				this.AddCellForCalendar(cell, rowIndex, (int)writeDay.DayOfWeek);

				if (writeDay.DayOfWeek == DayOfWeek.Saturday)
				{
					rowIndex++;
				}
			}
		}
		#endregion

		#region イベントハンドラ
		private void CalendarCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_mouseLeftButtonDownCell = (Label)sender;
		}

		private void CalendarCell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Label buttonUpedCell = (Label)sender;

			if (buttonUpedCell.Content == this._mouseLeftButtonDownCell.Content)
			{
				if (null != this._selectedCell)
				{
					this._selectedCell.Background
						= this.Resources["NonSelectedDayBrush"] as Brush;
				}
				
				if (null == this._selectedCell
					|| this._selectedCell.Content != buttonUpedCell.Content)
				{
					buttonUpedCell.Background = this.Resources["SelectedDayBrush"] as Brush;

					this._selectedCell = buttonUpedCell;
				}
				else
				{
					this._selectedCell = null;
				}
			}
		}

		private void btnPrevMonth_Click(object sender, RoutedEventArgs e)
		{
			this._targetYearMonth = this._targetYearMonth.AddMonths(-1);
			this.ShowCalendar();
		}

		private void btnNextMonth_Click(object sender, RoutedEventArgs e)
		{
			this._targetYearMonth = this._targetYearMonth.AddMonths(1);
			this.ShowCalendar();
		}

		private void btnEntry_Click(object sender, RoutedEventArgs e)
		{
			int selectLocation = this.cmbLocation.SelectedIndex;

			if (0 > selectLocation)
			{
				MessageBox.Show("場所を選択してください。");
				return;
			}

			if (null == this._selectedCell)
			{
				MessageBox.Show("日付を選択してください。");
				return;
			}

			LocalDataBaseDataSet.LocationMasterRow locationRow
				= ((DataRowView)this.cmbLocation.Items[selectLocation]).Row
						as LocalDataBaseDataSet.LocationMasterRow;

			DateTime selectDate
				= new DateTime(this._targetYearMonth.Year,
							   this._targetYearMonth.Month,
							   int.Parse(this._selectedCell.Content.ToString()));

			string filter = string.Format("LocationID = {0} AND Date = #{1}#",
										  locationRow.LocationID,
										  selectDate);
			DataRow[] selectRows = this.Dac.ScheduleInfo.Select(filter);
			if (null != selectRows && 0 != selectRows.Length)
			{
				MessageBox.Show("既に登録済です。");
				return;
			}

			LocalDataBaseDataSet.ScheduleInfoRow row
				= this.Dac.ScheduleInfo.NewScheduleInfoRow();

			row.LocationID = locationRow.LocationID;
			row.Date = selectDate;
			row.RestFlg = false;

			this.Dac.ScheduleInfo.AddScheduleInfoRow(row);
			this.DialogResult = true;
			this.Close();
		}

		private void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			int selectLocation = this.cmbLocation.SelectedIndex;

			if (0 > selectLocation)
			{
				MessageBox.Show("場所を選択してください。");
				return;
			}

			if (null == this._selectedCell)
			{
				MessageBox.Show("日付を選択してください。");
				return;
			}

			LocalDataBaseDataSet.LocationMasterRow locationRow
				= ((DataRowView)this.cmbLocation.Items[selectLocation]).Row
						as LocalDataBaseDataSet.LocationMasterRow;

			string filter = string.Format("LocationID = {0} AND Date = #{1}#",
										  this._targetSchedule.LocationID,
										  this._targetSchedule.Date);

			DataRow[] selectRows = this.Dac.ScheduleInfo.Select(filter);
			if (null == selectRows || 0 == selectRows.Length)
			{
				MessageBox.Show("対象データが存在しませんでした。");
				return;
			}

			LocalDataBaseDataSet.ScheduleInfoRow row
				= selectRows[0] as LocalDataBaseDataSet.ScheduleInfoRow;

			row.Date = new DateTime(this._targetYearMonth.Year,
									this._targetYearMonth.Month,
									int.Parse(this._selectedCell.Content.ToString()));
			row.LocationID = locationRow.LocationID;

			this.DialogResult = true;
			this.Close();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			string filter = string.Format("LocationID = {0} AND Date = #{1}#",
										  this._targetSchedule.LocationID,
										  this._targetSchedule.Date);

			DataRow[] selectRows = this.Dac.ScheduleInfo.Select(filter);
			if (null == selectRows || 0 == selectRows.Length)
			{
				MessageBox.Show("対象データが存在しませんでした。");
				return;
			}

			LocalDataBaseDataSet.ScheduleInfoRow row
				= selectRows[0] as LocalDataBaseDataSet.ScheduleInfoRow;

			row.Delete();

			this.DialogResult = true;
			this.Close();
		}
		#endregion
	}
}
