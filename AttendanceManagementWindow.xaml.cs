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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;

using AttendanceManagement.DAL;
using AttendanceManagement.Converter;
using AttendanceManagement.Dialog;

namespace AttendanceManagement
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AttendanceManagementWindow : Window
	{
		#region プロパティ
		private LocalDataBaseDataSet Dac
		{
			get
			{
				return Application.Current.Properties["DAC"] as LocalDataBaseDataSet;
			}
		}

		private int SelectLocationID
		{
			get
			{
				return (int)Application.Current.Properties["SelectLocationId"];
			}
			set
			{
				Application.Current.Properties["SelectLocationId"] = value;
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

		private bool DispRetireMemberMode
		{
			get
			{
				var mode = Application.Current.Properties["DispRetireMemberMode"] as bool?;
				return (null == mode) ? false : mode.Value;
			}
			set
			{
				Application.Current.Properties["DispRetireMemberMode"] = value;
			}
		}
	
		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public AttendanceManagementWindow()
		{
			this.InitializeComponent();

			this.SelectLocationID = -1;

			this.ReloadSchedule();
		}

		#region メンバメソッド
		/// <summary>
		/// ステータスバー（ヘッダ）の書き換え
		/// </summary>
		private void WriteStatusBarHeader()
		{
			StringBuilder sb = new StringBuilder();

			foreach (LocalDataBaseDataSet.JobMasterRow row in this.Dac.JobMaster.Select("", "JobID"))
			{
				sb.Append(JobConverter.ToString(row.JobID));

				if ("En" == this.JpEnMode)
				{
					sb.Append("\t");
				}
			}

			this.stbHeader.Text = sb.ToString();
		}

		/// <summary>
		/// ステータスバー（コンテンツ）の書き換え
		/// </summary>
		/// <param name="locationIdParam"></param>
		private void WriteStatusBarContent(int locationIdParam)
		{
			StringBuilder sb = new StringBuilder();

			LocalDataBaseDataSet.LocationJobPartsMapperDataTable table
				= this.Dac.GetJobPartsList(locationIdParam);

			if (null != table)
			{
				foreach (LocalDataBaseDataSet.JobMasterRow jobRow
							in this.Dac.JobMaster.Select("", "JobID"))
				{
					LocalDataBaseDataSet.LocationJobPartsMapperRow partsRow = null;

					DataRow[] filterRows = table.Select(string.Format("JobID = {0}", jobRow.JobID));

					if (null != filterRows && 1 == filterRows.Length)
					{
						partsRow = filterRows[0] as LocalDataBaseDataSet.LocationJobPartsMapperRow;
					}

					sb.Append((null == partsRow) ? "☓" : PartsConverter.ToString(partsRow.PartsID));

					if ("En" == this.JpEnMode)
					{
						sb.Append("\t");
					}
				}

				this.SelectLocationID = locationIdParam;
			}

			this.stbContent.Text = sb.ToString();
		}

		/// <summary>
		/// スケジュール情報の再読込
		/// </summary>
		private void ReloadSchedule()
		{
			this.lsvSchedule.ItemsSource
				= this.Dac.ScheduleInfo.Select("LocationID IS NOT NULL", "Date DESC");

			this.WriteStatusBarHeader();

			this.ReloadMember();
		}

		/// <summary>
		/// メンバリストの再読込
		/// </summary>
		private void ReloadMember()
		{
			int selectIndex = this.lsvSchedule.SelectedIndex;

			LocalDataBaseDataSet.ScheduleInfoRow row = null;

			//メンバービュー
			if (0 > selectIndex || selectIndex >= this.lsvSchedule.Items.Count)
			{
				this.lsvMember.ItemsSource = null;
			}
			else
			{
				row = this.lsvSchedule.Items[selectIndex] as LocalDataBaseDataSet.ScheduleInfoRow;
				this.lsvMember.ItemsSource = this.Dac.GetWorkingMemberByWorkingNo(row.WorkingNo, this.DispRetireMemberMode);
			}

			this.WriteStatusBarContent((null == row || row.IsLocationIDNull()) ? 0 : row.LocationID);
		}

		/// <summary>
		/// メンバリストで選択された行を取得
		/// </summary>
		/// <returns></returns>
		private LocalDataBaseDataSet.WorkingManagementRow SelectedWorkingMemberRow()
		{
			int selectMemberIndex = this.lsvMember.SelectedIndex;
			int selectScheduleIndex = this.lsvSchedule.SelectedIndex;

			if (0 > selectScheduleIndex || selectScheduleIndex >= this.lsvSchedule.Items.Count
				|| 0 > selectMemberIndex || selectMemberIndex >= this.lsvMember.Items.Count)
			{
				return null;
			}

			//スケジュール
			LocalDataBaseDataSet.ScheduleInfoRow scheduleRow
				= this.lsvSchedule.Items[selectScheduleIndex]
						as LocalDataBaseDataSet.ScheduleInfoRow;

			//メンバー
			LocalDataBaseDataSet.WorkingMemberRow memberRow
				= ((DataRowView)this.lsvMember.Items[selectMemberIndex]).Row
						as LocalDataBaseDataSet.WorkingMemberRow;

			string filter = string.Format("WorkingNo = {0} AND MemberID = {1}",
										  scheduleRow.WorkingNo,
										  memberRow.MemberID);
			DataRow[] selectRows = this.Dac.WorkingManagement.Select(filter);

			LocalDataBaseDataSet.WorkingManagementRow rtnRow = null;

			if (null != selectRows && 0 != selectRows.Length)
			{
				rtnRow = selectRows[0] as LocalDataBaseDataSet.WorkingManagementRow;
			}
			else
			{
				rtnRow = this.Dac.WorkingManagement.NewWorkingManagementRow();

				rtnRow.WorkingNo = scheduleRow.WorkingNo;
				rtnRow.MemberID = memberRow.MemberID;
				rtnRow.WorkKbn = 0;
				rtnRow.IsGotMoney = false;

				this.Dac.WorkingManagement.AddWorkingManagementRow(rtnRow);
			}

			return rtnRow;
		}

		#endregion

		#region イベントハンドラ
		/// <summary>
		/// スケジュール行選択ハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lsvSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.ReloadMember();
		}

		/// <summary>
		/// メニューの再読込押下時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Reload_Click(object sender, RoutedEventArgs e)
		{
			this.Dac.Load();

			this.ReloadSchedule();
		}

		/// <summary>
		/// メニューのバージョン情報押下時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void VersionInfo_Click(object sender, RoutedEventArgs e)
		{
			AboutBox dlg = new AboutBox();
			dlg.ShowDialog();
		}

		/// <summary>
		/// 日本語／英語表示切替
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void JpEn_Click(object sender, RoutedEventArgs e)
		{
			this.JpEnMode = ("Jp" == this.JpEnMode) ? "En" : "Jp";
			this.ReloadSchedule();
		}

		/// <summary>
		/// 取得部位編集ウィンドウのモーダル表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GotEquipEdit_Click(object sender, RoutedEventArgs e)
		{
			int selectMemberIndex = this.lsvMember.SelectedIndex;
			int selectScheduleIndex = this.lsvSchedule.SelectedIndex;

			if (0 > selectScheduleIndex || selectScheduleIndex >= this.lsvSchedule.Items.Count
				|| 0 > selectMemberIndex || selectMemberIndex >= this.lsvMember.Items.Count)
			{
				return;
			}

			//スケジュール
			LocalDataBaseDataSet.ScheduleInfoRow scheduleRow
				= this.lsvSchedule.Items[selectScheduleIndex]
						as LocalDataBaseDataSet.ScheduleInfoRow;

			//メンバー
			LocalDataBaseDataSet.WorkingMemberRow memberRow
				= ((DataRowView)this.lsvMember.Items[selectMemberIndex]).Row
						as LocalDataBaseDataSet.WorkingMemberRow;

			var dlg = new GotEquipDialog(memberRow, scheduleRow)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			bool? result = dlg.ShowDialog();

			if (null != result && true == result)
			{
				this.Dac.GotEquipInfo.AcceptChanges();

				this.ReloadMember();
			}
		}

		/// <summary>
		/// コンテキストメニューを開く前に呼び出されます
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lsvMember_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (null == this.lsvMember.ItemsSource)
			{
				this.lsvMember.ContextMenu = null;
			}
			else
			{
				this.lsvMember.ContextMenu = (ContextMenu)this.Resources["lsvMemberContextMenu"];
			}
		}

		/// <summary>
		/// 出欠ON・OFF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WorkingOnOff_Click(object sender, RoutedEventArgs e)
		{
			LocalDataBaseDataSet.WorkingManagementRow targetDataRow = this.SelectedWorkingMemberRow();

			if (null != targetDataRow)
			{
				if (targetDataRow.RowState != DataRowState.Added)
				{
					targetDataRow.Delete();
				}
			}

			this.Dac.WorkingManagement.AcceptChanges();

			this.ReloadSchedule();
		}

		/// <summary>
		/// 100貨幣取得ON・OFF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GotMoneyOnOff_Click(object sender, RoutedEventArgs e)
		{
			LocalDataBaseDataSet.WorkingManagementRow targetDataRow = this.SelectedWorkingMemberRow();

			if (null != targetDataRow)
			{
				if (targetDataRow.RowState != DataRowState.Added)
				{
					targetDataRow.IsGotMoney = (targetDataRow.IsGotMoney) ? false : true;
					this.Dac.WorkingManagement.AcceptChanges();
				}
				else
				{
					this.Dac.WorkingManagement.RemoveWorkingManagementRow(targetDataRow);
				}
			}

			this.ReloadMember();
		}

		/// <summary>
		/// メンバの詳細情報
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberInfo_Click(object sender, RoutedEventArgs e)
		{
			int selectMemberIndex = this.lsvMember.SelectedIndex;

			if (0 > selectMemberIndex || selectMemberIndex >= this.lsvMember.Items.Count)
			{
				return;
			}

			//メンバー
			LocalDataBaseDataSet.WorkingMemberRow memberRow
				= ((DataRowView)this.lsvMember.Items[selectMemberIndex]).Row
						as LocalDataBaseDataSet.WorkingMemberRow;

			LocalDataBaseDataSet.MemberInfoRow memberInfoRow
				= this.Dac.GetMemberInfo(memberRow.MemberID);

			var dlg = new MemberInfoReferenceDialog(memberInfoRow)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			dlg.ShowDialog();
		}

		/// <summary>
		/// スケジュール登録
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ScheduleEntry_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new ScheduleEntryDialog()
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			
			bool? result = dlg.ShowDialog();

			if (null != result && true == result)
			{
				this.Dac.ScheduleInfo.AcceptChanges();

				this.ReloadSchedule();
			}
		}

		/// <summary>
		/// スケジュール変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ScheduleModify_Click(object sender, RoutedEventArgs e)
		{
			int selectScheduleIndex = this.lsvSchedule.SelectedIndex;

			if (0 > selectScheduleIndex
				|| selectScheduleIndex >= this.lsvSchedule.Items.Count)
			{
				return;
			}

			//スケジュール
			LocalDataBaseDataSet.ScheduleInfoRow scheduleRow
				= this.lsvSchedule.Items[selectScheduleIndex]
						as LocalDataBaseDataSet.ScheduleInfoRow;

			var dlg = new ScheduleEntryDialog(scheduleRow, true)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};


			bool? result = dlg.ShowDialog();

			if (null != result && true == result)
			{
				this.Dac.ScheduleInfo.AcceptChanges();

				this.ReloadSchedule();
			}
		}

		/// <summary>
		/// スケジュール削除
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ScheduleDelete_Click(object sender, RoutedEventArgs e)
		{
			int selectScheduleIndex = this.lsvSchedule.SelectedIndex;

			if (0 > selectScheduleIndex
				|| selectScheduleIndex >= this.lsvSchedule.Items.Count)
			{
				return;
			}

			//スケジュール
			LocalDataBaseDataSet.ScheduleInfoRow scheduleRow
				= this.lsvSchedule.Items[selectScheduleIndex]
						as LocalDataBaseDataSet.ScheduleInfoRow;

			var dlg = new ScheduleEntryDialog(scheduleRow, false)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};


			bool? result = dlg.ShowDialog();

			if (null != result && true == result)
			{
				this.Dac.ScheduleInfo.AcceptChanges();

				this.ReloadSchedule();
			}
		}

		/// <summary>
		/// クリップボードにコピー
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
		{
			int selectScheduleIndex = this.lsvSchedule.SelectedIndex;

			if (0 > selectScheduleIndex
				|| selectScheduleIndex >= this.lsvSchedule.Items.Count)
			{
				return;
			}

			//スケジュール
			LocalDataBaseDataSet.ScheduleInfoRow scheduleRow
				= this.lsvSchedule.Items[selectScheduleIndex]
						as LocalDataBaseDataSet.ScheduleInfoRow;

			//対象日の参加メンバーを取得する
			LocalDataBaseDataSet.WorkingMemberDataTable wkMbrTbl
				= this.Dac.GetWorkingMemberByWorkingNo(scheduleRow.WorkingNo, false);

			//クリップボード書出し用
			StringBuilder sb = new StringBuilder();
			StringBuilder equipContent = new StringBuilder();
			StringBuilder moneyContent = new StringBuilder();
			StringBuilder memberContent = new StringBuilder();
			int workCount = 0;
			int vacationCount = 0;

			string hr = "--------------------------------------------------";

			//全体
			sb.Append("ドロップ内容").Append(Environment.NewLine).Append(Environment.NewLine);

			//AF取得者
			equipContent.Append(hr).Append(Environment.NewLine);
			equipContent.Append("AF取得者").Append(Environment.NewLine);
			equipContent.Append(hr).Append(Environment.NewLine);

			//100貨幣取得者
			moneyContent.Append(hr).Append(Environment.NewLine);
			moneyContent.Append("100貨幣取得者").Append(Environment.NewLine);
			moneyContent.Append(hr).Append(Environment.NewLine);

			//参加メンバー
			memberContent.Append(hr).Append(Environment.NewLine);
			memberContent.Append("参加メンバー").Append(Environment.NewLine);
			memberContent.Append(hr).Append(Environment.NewLine);

			foreach (LocalDataBaseDataSet.WorkingMemberRow wkMbrRow in wkMbrTbl.Select("", "Name"))
			{
				//AF
				LocalDataBaseDataSet.GotEquipInfoDataTable tbl
					= this.Dac.GetGotEquipInfoByLocationIdMemberId(scheduleRow.WorkingNo,
																   wkMbrRow.MemberID);
				if (null != tbl)
				{
					//名前
					equipContent.Append(wkMbrRow.Name);
					equipContent.Append("：");

					//取得部位
					for (int index = 0; index < tbl.Count; index++)
					{
						LocalDataBaseDataSet.GotEquipInfoRow equipRow = tbl[index];

						equipContent.Append(this.Dac.GetJob(equipRow.JobID).JName);
						equipContent.Append(this.Dac.GetParts(equipRow.PartsID).JName);

						if (index != tbl.Count - 1)
						{
							equipContent.Append("、");
						}
					}

					equipContent.Append(Environment.NewLine);
				}

				LocalDataBaseDataSet.WorkingManagementRow wkMngRow
					= this.Dac.GetWorkingManagementRow(scheduleRow.WorkingNo,
													   wkMbrRow.MemberID);

				//100
				if (null != wkMngRow && wkMngRow.IsGotMoney)
				{
					moneyContent.Append(wkMbrRow.Name);
					moneyContent.Append(Environment.NewLine);
				}

				//出欠
				if (null != wkMngRow)
				{
					memberContent.Append("○").Append("　");
					workCount++;
				}
				else
				{
					memberContent.Append("×").Append("　");
					vacationCount++;
				}

				memberContent.Append(wkMbrRow.Name);
				memberContent.Append(Environment.NewLine);
			}

			sb.Append(equipContent.ToString()).Append(Environment.NewLine);
			sb.Append(moneyContent.ToString()).Append(Environment.NewLine);
			sb.Append(memberContent.ToString()).Append(Environment.NewLine);

			//21名出席/8名欠席
			sb.Append(string.Format("{0}名出席/{1}名欠席", workCount, vacationCount));

			//クリップボードに書き出す
			Clipboard.Clear();
			Clipboard.SetText(sb.ToString());
		}

		/// <summary>
		/// データをXMLに保存する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Save_Click(object sender, RoutedEventArgs e)
		{
			this.Dac.Save();
		}

		/// <summary>
		/// 退会者情報表示のON/OFF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DispRetireMember_Click(object sender, RoutedEventArgs e)
		{
			this.DispRetireMemberMode = (!this.DispRetireMemberMode);

			this.menuDispRetireMember.IsChecked = this.DispRetireMemberMode;

			this.ReloadMember();
		}

		#endregion
	}
}
