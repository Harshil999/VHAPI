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
        Task<IEnumerable<CommonProcOutputFields.Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(string tokenNo, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
        Task<IEnumerable<CommonProcOutputFields.Resp_AttDtl_EmpInfo>> GetEmpAttendanceDtl_EmpInfo(string tokenNo, MispunchDtl_EmpInfo mispunchDtl_EmpInfo);
    }
}
