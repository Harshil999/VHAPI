﻿using VHEmpAPI.Shared;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Models.Repository
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<CommonProcOutputFields.IsValidToken>> IsTokenValid(string TokenNo, string LoginId);
        Task<IEnumerable<LoginId_TokenData>> Save_Token_UserCreds_and_ReturnToken(MobileCreds model, string TokenNo);
        Task<IEnumerable<CommonProcOutputFields.DashBoardList>> DisplayDashboardList(string TokenNo, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Ddl_Value_Nm>> GetMonthYr_EmpInfo();
        Task<IEnumerable<CommonProcOutputFields.Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.Resp_AttDtl_EmpInfo>> GetEmpAttendanceDtl_EmpInfo(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.Resp_AttSumm_EmpInfo>> GetEmpAttDtl_Summ(string EmpId, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(string EmpId, LoginIdNum loginIdNum);
        Task<IEnumerable<CommonProcOutputFields.OutSingleString>> GetLeaveDays(string EmpId, GetLeaveDays getLeaveDays);
        Task<IEnumerable<CommonProcOutputFields.Resp_value_name>> GetLeaveNames(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_name>> GetLeaveReason(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LvDelayReason>> GetLeaveDelayReason(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_id_name>> EmpApp_GetLeaveRelieverNm(string EmpId, string LoginId);
        Task<IEnumerable<CommonProcOutputFields.Resp_LvEntryList>> EmpApp_GetLeaveEntryList(string EmpId, string LoginId, string Flag);
        Task<IEnumerable<CommonProcOutputFields.SavedYesNo>> EmpApp_SaveLeaveEntryList(string EmpId, SaveLeaveEntry saveLeaveEntry);
    }
}
