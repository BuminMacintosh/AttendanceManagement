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

namespace AttendanceManagement.Dialog
{
	/// <summary>
	/// MemberInfoReferenceDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class MemberInfoReferenceDialog : Window
	{
		#region メンバフィールド
		private Int64 _memberID;
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
		public MemberInfoReferenceDialog(LocalDataBaseDataSet.MemberInfoRow memberInfoParam)
		{
			this.InitializeComponent();

			this._memberID = memberInfoParam.MemberID;

			if ("Jp" == this.JpEnMode)
			{
				this.grpBox.Header = "取得AF一覧";
			}
			else
			{
				this.grpBox.Header = "Got AF Summary";
			}

			this.chkRetireFlg.IsChecked = memberInfoParam.RetireFlg;
			this.txtName.Text = memberInfoParam.Name;
			this.txtNickName.Text = memberInfoParam.NickName;
			this.txtAttendance.Text
				= this.Dac.GetMemberAttendance(memberInfoParam.MemberID).ToString("#回");
			this.txtAttendanceRate.Text
				= this.Dac.GetMemberAttendanceRate(memberInfoParam.MemberID).ToString("#.#％");

			this.lsvSummary.ItemsSource
				= this.Dac.GetGotEquipListByMemberID(memberInfoParam.MemberID);
		}

		#region イベントハンドラ
		/// <summary>
		/// OKボタン
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// 編集ボタン
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{
			if (true == this.btnOK.IsEnabled)
			{
				//ダイアログを編集モードにする
				this.txtName.IsReadOnly = false;
				this.txtNickName.IsReadOnly = false;
				this.chkRetireFlg.IsEnabled = true;
				this.btnEdit.Content = "編集終了";

				this.btnOK.IsEnabled = false;
			}
			else
			{
				//ダイアログを参照モードにする
				this.txtName.IsReadOnly = true;
				this.txtNickName.IsReadOnly = true;
				this.chkRetireFlg.IsEnabled = false;
				this.btnEdit.Content = "編集する";

				this.btnOK.IsEnabled = true;

				//編集完了したのでレコードを保存する
				var memberInfo = this.Dac.GetMemberInfo(this._memberID);

				memberInfo.RetireFlg = this.chkRetireFlg.IsChecked.Value;
				memberInfo.Name = this.txtName.Text;
				memberInfo.NickName = this.txtNickName.Text;
			}
		}
		#endregion
	}
}
