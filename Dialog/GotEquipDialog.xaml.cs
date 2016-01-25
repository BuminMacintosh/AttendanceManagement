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
using AttendanceManagement.Converter;
using System.Data;

namespace AttendanceManagement.Dialog
{
	/// <summary>
	/// GotEquipDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class GotEquipDialog : Window
	{
		#region メンバフィールド
		private Int64 _memberID;
		private int _locationID;
		private Int64 _workingNo;
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
		/// <param name="memberInfoParam"></param>
		/// <param name="scheduleInfoParam"></param>
		public GotEquipDialog(LocalDataBaseDataSet.WorkingMemberRow memberInfoParam,
							  LocalDataBaseDataSet.ScheduleInfoRow scheduleInfoParam)
		{
			this.InitializeComponent();

			this._memberID = memberInfoParam.MemberID;
			this._locationID = scheduleInfoParam.LocationID;
			this._workingNo = scheduleInfoParam.WorkingNo;

			if ("Jp" == this.JpEnMode)
			{
				this.lblBui.Content = "取得部位 :";
			}
			else
			{
				this.lblBui.Content = "Job And Parts :";
			}

			this.SetJobPartsComboBox();
		}

		#region メンバメソッド
		/// <summary>
		/// ジョブパーツコンボボックスの内容セット
		/// </summary>
		private void SetJobPartsComboBox()
		{
			LocalDataBaseDataSet.JobPartsDataTable tbl
				= new LocalDataBaseDataSet.JobPartsDataTable();

			foreach (LocalDataBaseDataSet.JobMasterRow jobRow in this.Dac.JobMaster)
			{
				int partsID = this.Dac.GetPartsByLocationJob(this._locationID, jobRow.JobID);

				if (0 == partsID)
				{
					continue;
				}

				LocalDataBaseDataSet.PartsMasterRow partsRow = this.Dac.GetParts(partsID);

				tbl.AddJobPartsRow(jobRow.JobID,
								   partsRow.PartsID,
								   string.Format("{0} - {1}", jobRow.JName, partsRow.JName),
								   string.Format("{0} - {1}", jobRow.EName, partsRow.EName));
			}

			this.cmbJobParts.ItemsSource = tbl;

			if ("Jp" == this.JpEnMode)
			{
				this.cmbJobParts.ItemTemplate = this.Resources["JNameTemplate"] as DataTemplate;
			}
			else
			{
				this.cmbJobParts.ItemTemplate = this.Resources["ENameTemplate"] as DataTemplate;
			}
		}

		/// <summary>
		/// 選択中のジョブパーツデータを返す
		/// </summary>
		/// <returns></returns>
		private LocalDataBaseDataSet.JobPartsRow GetSelectedJobParts()
		{
			int selectJobParts = this.cmbJobParts.SelectedIndex;

			LocalDataBaseDataSet.JobPartsRow selectRow = null;

			if (0 <= selectJobParts)
			{
				selectRow = ((DataRowView)this.cmbJobParts.Items[selectJobParts]).Row
													as LocalDataBaseDataSet.JobPartsRow;
			}

			return selectRow;
		}

		#endregion

		#region イベントハンドラ
		/// <summary>
		/// 登録
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRegist_Click(object sender, RoutedEventArgs e)
		{
			LocalDataBaseDataSet.JobPartsRow jobPartsRow = this.GetSelectedJobParts();

			if (null != jobPartsRow)
			{
				LocalDataBaseDataSet.GotEquipInfoRow row
					= this.Dac.GetGotEquipInfo(this._memberID,
											   jobPartsRow.JobID,
											   jobPartsRow.PartsID);

				if (null != row)
				{
					MessageBox.Show("既に登録済です。");
					return;
				}

				row = this.Dac.GotEquipInfo.NewGotEquipInfoRow();

				row.JobID = jobPartsRow.JobID;
				row.PartsID = jobPartsRow.PartsID;
				row.MemberID = this._memberID;
				row.WorkingNo = this._workingNo;

				this.Dac.GotEquipInfo.AddGotEquipInfoRow(row);

				this.DialogResult = true;

				this.Close();
			}
		}

		/// <summary>
		/// 削除
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			LocalDataBaseDataSet.JobPartsRow jobPartsRow = this.GetSelectedJobParts();

			if (null != jobPartsRow)
			{
				LocalDataBaseDataSet.GotEquipInfoRow row
					= this.Dac.GetGotEquipInfo(this._memberID,
											   jobPartsRow.JobID,
											   jobPartsRow.PartsID);

				if (null == row)
				{
					MessageBox.Show("登録されていないため削除できません。");
					return;
				}

				row.Delete();

				this.DialogResult = true;

				this.Close();
			}
		}
		#endregion
	}
}
