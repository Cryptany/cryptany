/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Cryptany.Common.Constants;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Services;

namespace Cryptany.Core
{
    /// <summary>
    /// ������ �������������� ��������� �������� �������� �� ����. ������������ ���� �� ������������ �������� ����� � �� ������������ ����� � �����.
    /// <remarks>
    /// ��� ������ ����������� ��������� ���������
    /// <list type="">
    /// <item>
    ///		System.Int32 ContentCodeService_PrintableAdwLimit - ������� ������� ����������� �������� ����� � ������ ����� ��������
    /// </item>
    /// <item>
    ///		System.Int32 ContentCodeService_ContentCodeLength - ����� ���� ��������
    /// </item>
    /// <item>
    ///		System.Int32 ContentCodeService_DefaultPartnerCode - ��� ������� "�� ���������"
    /// </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public class ContentCodeService : AbstractService
    {
        public ContentCodeService(IRouter router, string name) : base(router, name)
        {
        }

		protected override bool ProcessMessageInner(Message msg, AnswerMap map )
		{
			string digitCode = SeparateDigitCode(msg.Text);
			Trace.Write(string.Format("CCS: PM: got digit code {0}", digitCode));
			int partnerCode;
			int contentCode;
			GetCodeParts(digitCode, out partnerCode, out contentCode);
			Trace.Write(string.Format("CCS: PM: got code parts: partner [{0}], content [{1}]", partnerCode, contentCode));
			ContentCodeType cctid;
				cctid = ContentCodeType.Printable;
			int ccId = GetContentCatalogId(contentCode, cctid);
			if ( ccId != -1 ) // ��� �������� � ������ ����� ������, ��������� ContentCatalogId
			{
				// ��������� ������ �� ���������� ��������
				if ( cctid == ContentCodeType.Printable )
				{
					// fixate into printables table
					fixPrintable(contentCode, partnerCode, msg.InboxId);
				}
				Trace.Write(string.Format("CCS: PM: got content catalog id: {0}, contentcodetype is : {1}", ccId, cctid));
				LinkToContent contentToSend = new LinkToContent(ccId);

                MessageSender.SendContent(contentToSend, msg,map.Channel, Guid.Empty, _messages);
				
                return true;
			}
            return false;
			
		}

        /// <summary>
        /// �������� �������� ��� �� ������
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual string SeparateDigitCode(string input)
        {
            string result = string.Empty;
            bool codeStarted = false;
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    codeStarted = true;
                    result += c;
                }
                else if (codeStarted)
                {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// ��������� �������� ��� �� ��� �������� � ��� ��������
        /// </summary>
        /// <param name="input">�������� ���, ���������� ���������</param>
        /// <param name="partnerCode">��� ��������</param>
        /// <param name="contentCode">��� ��������</param>
        protected virtual void GetCodeParts(string input, out int partnerCode, out int contentCode)
        {
            contentCode = 0;
            partnerCode = 0;// (int)CoreClassFactory.Settings["ContentCodeService_DefaultPartnerCode"];
            int ccLen = 5;// (int)CoreClassFactory.Settings["ContentCodeService_ContentCodeLength"];
            if (input.Length > ccLen)
            {
                int.TryParse(input.Substring(0, ccLen), out contentCode);
                int.TryParse(input.Substring(ccLen), out partnerCode);
            }
        }

        /// <summary>
        /// ���������� ������������� �������� � ��������, ���� ��� �������� � ��� ����
        /// </summary>
        /// <param name="contentCode">��� ��������, ��� ���� ������</param>
        /// <param name="cct">��� ����</param>
        /// <returns>������� ������: ������������� �������� � ��������, ������� �� ������: -1</returns>
        protected virtual int GetContentCatalogId(int contentCode, ContentCodeType cct)
        {
            int ccId = -1;
            string query =
                "select ContentCatalogId from content.ContentCodes where code = @code and ContentCodeTypeId = @cctid";
            using (SqlConnection conn = Database.Connection)
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@code", contentCode);
                    cmd.Parameters.AddWithValue("@cctid", cct);
                    cmd.CommandTimeout = 0;
                    object tmp = cmd.ExecuteScalar();
                    if (tmp != null)
                    {
                        ccId = (int) tmp;
                    }
                    return ccId;
                }
            }
        }

        /// <summary>
        /// ���������� ������������� ������ ������, ���� ��� ������ � ��� ���� (������� ��� B2B). ������ ������ ������ -- SMS payment
        /// </summary>
        /// <param name="partnerCode">��� ������ (1-4 �����)</param>
        /// <param name="cct">��� ����: ������� ���� B2B</param>
        /// <returns>����� ������: Guid ������ ������; ����� �� ������: Guid ������ ��-���������</returns>
        protected virtual Guid? GetSaleChannelId(int partnerCode, ContentCodeType cct)
        {
            Guid? result = null;
            try
            {
                using (SqlConnection conn = Database.Connection)
                {
                    using (SqlCommand cmd = new SqlCommand("Kernel.GetSaleChannelID", conn))
                    {
                        cmd.Parameters.AddWithValue("@PartnerCode", partnerCode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        object tmp = cmd.ExecuteScalar();
                        if (tmp == null)
                        {
                            throw (new ApplicationException("Unknown code. Sale channel not found."));
                        }
                        result = (Guid) tmp;
                    }
                }
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in ContentCodeService GetSaleChannelId method: " + e,
                                                LogSeverity.Error));
                }
            }
            return result;
        }

        protected void fixPrintable(int code, int partner, Guid inboxId)
        {
            string query = "content.fixPrintable";
            try
            {
                using (SqlConnection conn = Database.Connection)
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@partner", partner);
                        cmd.Parameters.AddWithValue("@inboxId", inboxId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new ApplicationException("Cannot save printable code info", ex));
            }
        }
    }
}