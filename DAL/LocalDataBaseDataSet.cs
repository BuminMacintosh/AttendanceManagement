using System;
using System.Data;
using System.Linq;

namespace AttendanceManagement.DAL
{
	public partial class LocalDataBaseDataSet
	{	
		public void Load()
		{
			this.Clear();
			this.ReadXml("XML\\DataBase.xml");
		}

		public void Save()
		{
			this.WriteXml("XML\\DataBase.xml", XmlWriteMode.WriteSchema);
		}

		public WorkingManagementRow GetWorkingManagementRow(Int64 workingNoParam,
															Int64 memberIdParam)
		{
			WorkingManagementRow selectedRow = null;

			string filter = string.Format("WorkingNo = {0} AND MemberID = {1}",
										  workingNoParam,
										  memberIdParam);

			DataRow[] selectRows = this.WorkingManagement.Select(filter);

			if (null != selectRows && 0 != selectRows.Length)
			{
				selectedRow = selectRows[0] as WorkingManagementRow;
			}

			return selectedRow;
		}

		/// <summary>
		/// 対象メンバIDの出席数を算出する
		/// </summary>
		/// <param name="memberIdParam"></param>
		/// <returns></returns>
		public int GetMemberAttendance(Int64 memberIdParam)
		{
			string filter = string.Format("MemberID = {0}", memberIdParam);

			DataRow[] selectRows = this.WorkingManagement.Select(filter);

			return (null != selectRows) ? selectRows.Length : 0;
		}

		/// <summary>
		/// 対象メンバIDの出席率を算出する
		/// </summary>
		/// <param name="memberIdParam"></param>
		/// <returns></returns>
		public double GetMemberAttendanceRate(Int64 memberIdParam)
		{
			var attendanceRows = this.WorkingManagement.Where(e => e.MemberID == memberIdParam)
													   .OrderBy(e => e.WorkingNo);

			var scheduleRows = this.ScheduleInfo.Where(e => false == e.IsLocationIDNull())
												.Where(e => e.WorkingNo >= attendanceRows.First().WorkingNo);
											
			return (null != attendanceRows && null != scheduleRows)
					? ((double)attendanceRows.Count() / (double)scheduleRows.Count()) * 100 : 0;
		}

		public bool IsGotEquip(Int64 workingNoParam, Int64 memberIdParam)
		{
			string filter = string.Format("WorkingNo = {0} AND MemberID = {1}",
										  workingNoParam,
										  memberIdParam);

			DataRow[] selectRows = this.GotEquipInfo.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? false : true;
		}

		public bool IsGotParts(Int64 memberIdParam, int jobIdParam, int partsIdParam)
		{
			string filter = string.Format("MemberID = {0} AND JobID = {1} AND PartsID = {2}",
										  memberIdParam,
										  jobIdParam,
										  partsIdParam);

			DataRow[] selectRows = this.GotEquipInfo.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? false : true;
		}

		/// <summary>
		/// 対象WorkingNoのメンバー情報を抽出する
		/// </summary>
		/// <param name="workingNoParam"></param>
		/// <param name="incloudeRetireParam"></param>
		/// <returns></returns>
		public WorkingMemberDataTable GetWorkingMemberByWorkingNo(Int64 workingNoParam,
																  bool incloudeRetireParam)
		{
			var tbl = new WorkingMemberDataTable();
			var whereCause = (incloudeRetireParam) 
								? string.Empty
								: string.Format("RetireFlg = 0");

			foreach (MemberInfoRow memberRow in this.MemberInfo.Select(whereCause, "Name"))
			{
				var newRow = tbl.NewWorkingMemberRow();

				newRow.MemberID = memberRow.MemberID;
				newRow.Name = memberRow.Name;
				newRow.Work = "-";
				newRow.IsGotEquip = "-";
				newRow.IsGotMoney = "-";

				var workingRow = this.GetWorkingManagementRow(workingNoParam,
															  memberRow.MemberID);
				if (null != workingRow)
				{
					newRow.Work = "○";
					newRow.IsGotMoney = (workingRow.IsGotMoney) ? "○" : "-";
					newRow.IsGotEquip = (this.IsGotEquip(workingNoParam, memberRow.MemberID)) ? "○" : "-";
				}

				tbl.AddWorkingMemberRow(newRow);
			}

			return tbl;
		}

		public GotEquipListDataTable GetGotEquipListByMemberID(Int64 memberIdParam)
		{
			GotEquipListDataTable tbl = new GotEquipListDataTable();

			foreach (JobMasterRow jobRow in this.JobMaster.Select("", "JobID"))
			{
				GotEquipListRow newRow = tbl.NewGotEquipListRow();

				newRow.EName = jobRow.EName;
				newRow.JName = jobRow.JName;
				newRow.Head = (this.IsGotParts(memberIdParam, jobRow.JobID, 1)) ? "○" : "-";
				newRow.Body = (this.IsGotParts(memberIdParam, jobRow.JobID, 2)) ? "○" : "-";
				newRow.Hands = (this.IsGotParts(memberIdParam, jobRow.JobID, 3)) ? "○" : "-";
				newRow.Legs = (this.IsGotParts(memberIdParam, jobRow.JobID, 4)) ? "○" : "-";
				newRow.Feet = (this.IsGotParts(memberIdParam, jobRow.JobID, 5)) ? "○" : "-";

				tbl.AddGotEquipListRow(newRow);
			}

			return tbl;
		}

		public MemberInfoRow GetMemberInfo(Int64 memberIdParam)
		{
			string filter = string.Format("MemberID = {0}", memberIdParam);

			DataRow[] selectRows = this.MemberInfo.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? null : selectRows[0] as MemberInfoRow;
		}

		public JobMasterRow GetJob(int jobIdParam)
		{
			string filter = string.Format("JobID = {0}", jobIdParam);

			DataRow[] selectRows = this.JobMaster.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? null : selectRows[0] as JobMasterRow;
		}

		public LocationMasterRow GetLocation(int locationIdParam)
		{
			string filter = string.Format("LocationID = {0}", locationIdParam);

			DataRow[] selectRows = this.LocationMaster.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? null : selectRows[0] as LocationMasterRow;
		}

		public PartsMasterRow GetParts(int partsIdParam)
		{
			string filter = string.Format("PartsID = {0}", partsIdParam);

			DataRow[] selectRows = this.PartsMaster.Select(filter);

			return (null == selectRows || 0 == selectRows.Length) ? null : selectRows[0] as PartsMasterRow;
		}

		public GotEquipInfoRow GetGotEquipInfo(Int64 memberIdParam,
											   int jobIdParam,
											   int partsIdParam)
		{
			string filter = string.Format("MemberID = {0} AND JobID = {1} AND PartsID = {2}",
										  memberIdParam,
										  jobIdParam,
										  partsIdParam);
			DataRow[] selectRows = this.GotEquipInfo.Select(filter);

			return (null != selectRows && 0 != selectRows.Length)
				? selectRows[0] as LocalDataBaseDataSet.GotEquipInfoRow : null;
		}

		public GotEquipInfoDataTable GetGotEquipInfoByLocationIdMemberId(Int64 workingNoParam,
																		 Int64 memberIdParam)
		{
			GotEquipInfoDataTable tbl = null;

			string filter = string.Format("WorkingNo = {0} AND MemberID = {1}",
										  workingNoParam,
										  memberIdParam);
			DataRow[] selectRows = this.GotEquipInfo.Select(filter);

			if (null != selectRows && 0 != selectRows.Length)
			{
				tbl = new GotEquipInfoDataTable();

				foreach (GotEquipInfoRow row in selectRows)
				{
					tbl.ImportRow(row);
				}
			}

			return tbl;
		}

		public LocationJobPartsMapperDataTable GetJobPartsList(int locationIdParam)
		{
			LocationJobPartsMapperDataTable table = null;

			string filter = string.Format("LocationID = {0}", locationIdParam);

			DataRow[] selectRows = this.LocationJobPartsMapper.Select(filter, "JobID");

			if (null != selectRows && 0 != selectRows.Length)
			{
				table = new LocationJobPartsMapperDataTable();

				foreach (DataRow row in selectRows)
				{
					table.ImportRow(row);
				}
			}

			return table;
		}

		public int GetPartsByLocationJob(int locationIdParam, int jobIdParam)
		{
			string filter = string.Format("LocationID = {0} AND JobID = {1}", locationIdParam, jobIdParam);

			DataRow[] selectRows = this.LocationJobPartsMapper.Select(filter);

			return (null != selectRows && 0 != selectRows.Length)
					? (selectRows[0] as LocationJobPartsMapperRow).PartsID : 0;
		}
	}
}

