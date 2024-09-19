﻿using Microsoft.AspNetCore.Mvc;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models.Repository;
using static VHEmpAPI.Shared.CommonProcOutputFields;
using VHMobileAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace VHEmpAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository? employeeRepository;
        private readonly IJwtAuth jwtAuth;
        public string Message = "";

        public EmployeeController(IEmployeeRepository employeeRepository, IJwtAuth jwtAuth)
        {
            this.employeeRepository = employeeRepository;
            this.jwtAuth = jwtAuth;
        }

        [HttpPost("authentication")]
        public async Task<ActionResult<dynamic>> Authentication([FromBody] MobileCreds mobileCreds)
        {
            if (mobileCreds.OTP == "" && mobileCreds.Password == "")
            {
                return Ok(new { statusCode = 401, isSuccess = "false", message = "Both Password and OTP cannot be empty!", data = new { } });
            }

            if (mobileCreds.OTP != "" && mobileCreds.Password != "")
            {
                return Ok(new { statusCode = 401, isSuccess = "false", message = "Any one from Password or OTP should be blank!", data = new { } });
            }

            if (mobileCreds.MobileNo != "" && mobileCreds.OTP != "" && mobileCreds.Password == "")
            {
                //SaveTokens_UserCreds saveTokens_UserCreds = new SaveTokens_UserCreds();
                //saveTokens_UserCreds.MobileNo = mobileCreds.MobileNo;

                DashBoardList dashboardList = new DashBoardList();
                dashboardList = await Save_Get_Token(mobileCreds);
                if (dashboardList != null)
                {
                    if (dashboardList.is_valid_token != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }

                    return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardList });
                }

                return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "false", message = "Bad Request", data = new { } });
            }

            #region password logic commented

            //else if (mobileCreds.MobileNo != "" && mobileCreds.OTP == "" && mobileCreds.Password != "")
            //{
            //    TokenData tokenData = new TokenData();

            //    string encodedPassword = "";
            //    //mobileCreds.Password = EncodeDecode.DecodeFrom64(mobileCreds.Password);
            //    mobileCreds.Password = EncodeDecode.EncodePasswordToBase64(mobileCreds.Password);

            //    var IsValidMobile = await employeeRepository.ValidateMobile_Pass(mobileCreds);

            //    string IsValid = "", TokenYN = "N";
            //    if (IsValidMobile != null && IsValidMobile.Count() > 0)
            //    {
            //        IsValid = IsValidMobile.Select(x => x.IsValidCreds).ToList()[0].ToString();
            //        TokenYN = IsValidMobile.Select(x => x.TokenNo).ToList()[0].ToString();

            //        if (IsValid.ToUpper() != "TRUE")
            //        {
            //            tokenData.IsValidCreds = IsValid;
            //            return Ok(new { statusCode = Ok(IsValidMobile).StatusCode, isSuccess = "false", message = "Invalid MobileNo or Password", data = new { } });
            //        }

            //        else if (IsValid.ToUpper() == "TRUE")
            //        {
            //            SaveTokens_UserCreds saveTokens_UserCreds = new SaveTokens_UserCreds();
            //            saveTokens_UserCreds.MobileNo = mobileCreds.MobileNo;

            //            DashBoardList dashboardList = new DashBoardList();
            //            dashboardList = await Save_Get_Token(mobileCreds);
            //            if (dashboardList != null)
            //            {
            //                if (dashboardList.is_valid_token != "Y")
            //                {
            //                    return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
            //                }

            //                return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardList });
            //            }

            //            return Ok(new { statusCode = Ok(dashboardList).StatusCode, isSuccess = "false", message = "Bad Request", data = new { } });
            //        }
            //    }
            //}

            #endregion

            return new EmptyResult();
        }

        [HttpPost("Save_Get_Token")]
        public async Task<DashBoardList> Save_Get_Token([FromBody] MobileCreds mobileCreds)
        {
            var Token = jwtAuth.Authentication();
            if (Token == null)
            {
                //tokenData.TokenNo = "";
                //return NotFound();
                return null;
            }

            //Call procedure with mobile, Token and OTPValidYN Y and save in table
            string IsValidToken = "", flag = "", LoginId = "";
            var ReturnToken = await employeeRepository.Save_Token_UserCreds_and_ReturnToken(mobileCreds, Token);
            if (ReturnToken != null && ReturnToken.Count() > 0)
            {
                IsValidToken = ReturnToken.Select(x => x.TokenNo).ToList()[0].ToString();
                LoginId = ReturnToken.Select(x => x.LoginId).ToList()[0].ToString();
                if (String.IsNullOrEmpty(IsValidToken) || IsValidToken == "N")
                {
                    //return NotFound();
                    return null;
                }
            }

            //var dashboardData = await DisplayDashboardList(Token, mobileCreds.MobileNo);
            var dashboardData = await DisplayDashboardList(Token, LoginId);
            if (dashboardData != null)
            {
                return dashboardData;
            }

            return dashboardData;
        }

        [HttpPost("GetDashboardList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetDashboardList([FromBody] LoginIdNum loginIdNum)
        {
            var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            string Token = WebUtility.UrlDecode(tokenNum);
            if (tokenNum != "")
            {
                var dashboardData = await DisplayDashboardList(tokenNum, loginIdNum.LoginId);
                if (dashboardData != null)
                {
                    if (dashboardData.is_valid_token != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }

                    return Ok(new { statusCode = Ok(dashboardData).StatusCode, isSuccess = "true", message = "Login Successful", data = dashboardData });
                }

                return Ok(new { statusCode = 400, isSuccess = "false", message = "Bad Request", data = new { } });
            }

            return new EmptyResult();
        }

        [HttpGet("DisplayDashboardList")]
        public async Task<DashBoardList> DisplayDashboardList(string Token, string LoginId)
        {
            try
            {
                string IsValid = "";
                string TokenNum = WebUtility.UrlDecode(Token);
                var isValidToken = await employeeRepository.IsTokenValid(TokenNum, LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return new DashBoardList { is_valid_token = "N" };
                    }
                }

                var result = await employeeRepository.DisplayDashboardList(TokenNum, LoginId);
                if (result == null)
                {
                    return null;
                }

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
            }
        }

        [HttpPost("GetMonthYr_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Ddl_Value_Nm>> GetMonthYr_EmpInfo(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetMonthYr_EmpInfo();
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetMisPunchDtl_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_MispunchDtl_EmpInfo>> GetMisPunchDtl_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetMisPunchDtl_EmpInfo(EmpId, mispunchDtl_EmpInfo);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetEmpAttendDtl_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_AttDtl_EmpInfo>> GetEmpAttendDtl_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpAttendanceDtl_EmpInfo(EmpId, mispunchDtl_EmpInfo);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetEmpAttendSumm_EmpInfo")]
        [Authorize]
        public async Task<ActionResult<Resp_AttSumm_EmpInfo>> GetEmpAttendSumm_EmpInfo(MispunchDtl_EmpInfo mispunchDtl_EmpInfo)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, mispunchDtl_EmpInfo.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpAttDtl_Summ(EmpId, mispunchDtl_EmpInfo);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetEmpSummary_Dashboard")]
        [Authorize]
        public async Task<ActionResult<ret_EmpSummary_Dashboard>> GetEmpSummary_Dashboard(LoginIdNum loginIdNum)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginIdNum.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetEmpSummary_Dashboard(EmpId, loginIdNum);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetAvlLvCount")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveDays(GetLeaveDays getLeaveDays)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, getLeaveDays.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveDays(EmpId, getLeaveDays);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetLeaveNames")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveNames(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveNames(EmpId, loginId_EmpId.LoginId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetLeaveReason")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveReason(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveReason(EmpId, loginId_EmpId.LoginId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("GetLeaveDelayReason")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetLeaveDelayReason(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.GetLeaveDelayReason(EmpId, loginId_EmpId.LoginId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("EmpApp_GetLeaveRelieverNm")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetLeaveRelieverNm(LoginId_EmpId loginId_EmpId)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetLeaveRelieverNm(EmpId, loginId_EmpId.LoginId);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("EmpApp_GetLeaveEntryList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_GetLeaveEntryList(LoginId_EmpId_Flag loginId_EmpId_Flag)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, loginId_EmpId_Flag.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_GetLeaveEntryList(EmpId, loginId_EmpId_Flag.LoginId, loginId_EmpId_Flag.Flag);
                if (result == null)
                    return NotFound();

                if (Ok(result).StatusCode != 200 || result.Count() == 0)
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                return Ok(new { statusCode = Ok(result).StatusCode, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

        [HttpPost("EmpApp_SaveLeaveEntryList")]
        [Authorize]
        public async Task<ActionResult<dynamic>> EmpApp_SaveLeaveEntryList(SaveLeaveEntry saveLeaveEntry)
        {
            try
            {
                string IsValid = "", EmpId = "";
                var tokenNum = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string Token = WebUtility.UrlDecode(tokenNum);

                var isValidToken = await employeeRepository.IsTokenValid(tokenNum, saveLeaveEntry.LoginId);
                if (isValidToken != null)
                {
                    IsValid = isValidToken.Select(x => x.IsValid).ToList()[0].ToString();
                    EmpId = isValidToken.Select(x => x.UserId).ToList()[0].ToString();
                    if (IsValid != "Y")
                    {
                        return Ok(new { statusCode = 401, isSuccess = "false", message = "Invalid Token!", data = new { } });
                    }
                }

                var result = await employeeRepository.EmpApp_SaveLeaveEntryList(EmpId, saveLeaveEntry);
                // Check if result is null
                if (result == null)
                    return NotFound(new { statusCode = 404, IsSuccess = "false", Message = "No data found!" });

                // Check if result is empty or status code is not 200
                if (Ok(result).StatusCode != 200 || !result.Any())
                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = "Bad Request or No data found!", data = new { } });

                // Get the SavedYN value from the result (assuming first entry has SavedYN)
                var output = result.FirstOrDefault()?.SavedYN;

                // Check if output is not "Y" (indicating an error or non-success response)
                if (output != "Y")
                {
                    // If \r\n exists in output, get the text before it
                    string finalMessage = output.Contains("\r\n") ? output.Substring(0, output.IndexOf("\r\n")) : output;

                    return Ok(new { statusCode = 400, IsSuccess = "false", Message = finalMessage, data = new { } });
                }

                // Return success response if everything is fine
                return Ok(new { statusCode = 200, IsSuccess = "true", Message = "Data fetched successfully", data = result });

            }
            catch (Exception ex)
            {
                return Ok(new { statusCode = 400, IsSuccess = "false", Message = ex.Message, data = new { } });
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message.ToString());
            }
            finally
            {
            }
        }

    }
}
