using DrugAmmendment.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Collections;
using System.Web.Mvc;
using System.IO;

namespace DrugAmmendment.Services
{
    public class DrugAmendmentConnectionService : IDrugAmendmentConnectionService
    {
        private string _connectionString;
        private SqlConnection _conn = null;
        private SqlCommand _cmd = null;

        public string Username
        {
            get
            {
                string username = HttpContext.Current.User.Identity.Name;
                if (username != null)
                {
                    int i = username.LastIndexOf(@"\");
                    if (i >= 0 && i < username.Length - 1)
                        username = username.Substring(i + 1);
                }
                return username;
            }
        }

        public DrugAmendmentConnectionService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            _conn = new SqlConnection(_connectionString);
        }

        public int UpdateToActive(string delivery, string criteriaType, string criteria)
        {
            int _rowsAffected = 0;
            _cmd = new SqlCommand($"update [dbo].[ADFeedSelectionCriteriaLookup] set IsActive = 1, ModificationDate = GETDATE() where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria.Replace("'", "''")}'", _conn);
            try
            {
                _conn.Open();
                _rowsAffected = _cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Updating the existing drug to Active");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _rowsAffected;
        }

        public bool ValidateLeadTerm(string criteria)
        {
            bool _flag = false;
            _cmd = new SqlCommand($"select LeadTerm FROM [dbo].[THSTerm] where LeadTerm  = '{criteria.Replace("'","''")}' and IsApproved = 'Y'", _conn);
            try
            {
                SqlDataAdapter dAdapter = new SqlDataAdapter(_cmd);
                DataTable dTable = new DataTable();
                dAdapter.Fill(dTable);
                if (dTable.Rows.Count > 0)
                    _flag = true;
                else
                    _flag = false;
            }
            catch(SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while Validating the Criteria");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Validating the Criteria");
                throw;
            }
            finally
            {
                _cmd.Dispose();
            }
            return _flag;
        }

        public ArrayList GetDataFromThesTerm(string criteria)
        {
            ArrayList data = new ArrayList();
            _cmd = new SqlCommand($"select LeadTerm, TermID from [dbo].[THSTerm] where LeadTerm = '{criteria.Replace("'", "''")}'", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader[0].ToString());
                    data.Add(reader[1]);
                }
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while fetching the data from THSTerm");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while fetching the data from THSTerm");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return data;
        }

        public bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria)
        {
            bool _flag = false;
            _cmd = new SqlCommand($"select Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] where Criteria  = '{criteria.Replace("'", "''")}' and CriteriaType = '{criteriaType}' and Delivery = '{delivery}' and IsActive = 0", _conn);
            try
            {
                SqlDataAdapter dAdapter = new SqlDataAdapter(_cmd);
                DataTable dTable = new DataTable();
                dAdapter.Fill(dTable);
                if (dTable.Rows.Count > 0)
                    _flag = true;
                else
                    _flag = false;
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while checking the Available and Non-Active");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while checking the Available and Non-Active");
                throw;
            }
            finally
            {
                _cmd.Dispose();
            }
            return _flag;
        }

        public int CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int? termID)
        {
            int _rowCount = 0;
            _cmd = new SqlCommand($"select Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria.Replace("'", "''")}' and IsActive = 1 ", _conn);
            try
            {
                SqlDataAdapter dAdapter = new SqlDataAdapter(_cmd);
                DataTable dTable = new DataTable();
                dAdapter.Fill(dTable);
                if (dTable.Rows.Count > 0)
                    _rowCount = 1;
                else
                    _rowCount = 0;
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while checking the Available and Active");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while checking the Available and Active");
                throw;
            }
            finally
            {
                _cmd.Dispose();
            }
            return _rowCount;
        }

        public int AddDrugToDB(string delivery, string criteriaType, string criteria, int? termID)
        {
            int _rowsAffected = 0;
            if (termID == null)
            {
                _cmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaLookup] values ('{delivery}','{criteriaType}','{criteria.Replace("'", "''")}',null,{1},GETDATE(),GETDATE())", _conn);
            }
            else
            {
                _cmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaLookup] values ('{delivery}','{criteriaType}','{criteria.Replace("'", "''")}',{termID},{1},GETDATE(),GETDATE())", _conn);
            }

            try
            {
                _conn.Open();
                _rowsAffected = _cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while adding drug to database.");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while adding drug to database.");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _rowsAffected;
        }

        public int? GetTermID(string delivery, string criteriaType, string criteria)
        {
            int? _termID = 0;
            _cmd = new SqlCommand($"select TermID from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria.Replace("'", "''")}'", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[0].Equals(System.DBNull.Value))
                    {
                        _termID = 0;
                    }
                    else
                    {
                        _termID = Convert.ToInt32(reader[0]);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _termID;
        }

        public void AuditLogger(string Delivery, string CriteriaType, string Criteria, string ActionType)
        {
            SqlCommand _logCmd = null;
            int? _termID = 0;
            try
            {
                _termID = GetTermID(Delivery, CriteriaType, Criteria);

                int _rowsAffected = 0;
                
                if (_termID == 0)
                {
                    _logCmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaHistory] values('{Delivery}','{CriteriaType}','{Criteria.Replace("'", "''")}',null,'{ActionType}',GETDATE(),'{Username}')", _conn);
                }
                else
                {
                    _logCmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaHistory] values('{Delivery}','{CriteriaType}','{Criteria.Replace("'", "''")}',{_termID},'{ActionType}',GETDATE(),'{Username}')", _conn);
                }

                _conn.Open();
                _rowsAffected = _logCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while logging the events.");
                throw;
            }
            finally
            {
                _conn.Close();
                _logCmd.Dispose();
            }
        }

        public int DeleteDrugFromDB(string Delivery, string CriteriaType, string Criteria)
        {
            int _rowsAffected = 0;
            _cmd = new SqlCommand($"update [dbo].[ADFeedSelectionCriteriaLookup] set IsActive = 0 , ModificationDate = GETDATE() where Delivery = '{Delivery}' and CriteriaType = '{CriteriaType}' and Criteria = '{Criteria.Replace("'", "''")}'", _conn);
            try
            {
                _conn.Open();
                _rowsAffected = _cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while Deleting the Drug");
                throw;
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Deleting the Drug");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _rowsAffected;
        }

        public List<SelectListItem> PopulateClients()
        {
            List<SelectListItem> _clients = new List<SelectListItem>();
            _cmd = new SqlCommand("select Distinct Delivery from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery not in ('nl.novartis','nl.novartis_nonicsr')", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    _clients.Add(new SelectListItem { Text = reader[0].ToString() });
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Populating the Clients List.");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _clients;
        }

        public List<SelectListItem> PopulateCriteriaType(string ClientName)
        {
            List<SelectListItem> _criteriaType = new List<SelectListItem>();
            _cmd = new SqlCommand($"select Distinct CriteriaType from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{ClientName}'", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    _criteriaType.Add(new SelectListItem { Text = reader[0].ToString() });
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Populating the Criteria Type List.");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _criteriaType;
        }

        public List<ExportToExcel> GetActiveDrugList(string Delivery, string CriteriaType)
        {
            List<ExportToExcel> _ddList = new List<ExportToExcel>();
            _cmd = new SqlCommand($"select * from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{Delivery}' and CriteriaType = '{CriteriaType}' and IsActive = 1", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    ExportToExcel dd = new ExportToExcel();
                    dd.Delivery = reader[0].ToString();
                    dd.CriteriaType = reader[1].ToString();
                    dd.Criteria = reader[2].ToString();
                    if (reader[3].Equals(System.DBNull.Value))
                    {
                        dd.TermID = null;
                    }
                    else
                    {
                        dd.TermID = Convert.ToInt32(reader[3]);
                    }
                    if (reader[5].Equals(System.DBNull.Value))
                    {
                        dd.ModificationDate = "Null";
                    }
                    else
                    {
                        dd.ModificationDate = reader[5].ToString();
                    }
                    if (reader[6].Equals(System.DBNull.Value))
                    {
                        dd.CreationDate = "Null";
                    }
                    else
                    {
                        dd.CreationDate = reader[6].ToString();
                    }
                    _ddList.Add(dd);
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while Populating the Active Drug List.");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _ddList;
        }

        public List<DrugDetails> GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> _ddList = new List<DrugDetails>();
            _cmd = new SqlCommand($"select * from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{ClientName}' and CriteriaType = '{CriteriaType}'", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    DrugDetails dd = new DrugDetails();
                    dd.Delivery = reader[0].ToString();
                    dd.CriteriaType = reader[1].ToString();
                    dd.Criteria = reader[2].ToString();
                    dd.IsActive = Convert.ToInt32(reader[4]);
                    if (reader[3].Equals(System.DBNull.Value))
                    {
                        dd.TermID = null;
                    }
                    else
                    {
                        dd.TermID = Convert.ToInt32(reader[3]);
                    }
                    if (reader[5].Equals(System.DBNull.Value))
                    {
                        dd.ModificationDate = null;
                    }
                    else
                    {
                        dd.ModificationDate = Convert.ToDateTime(reader[5]);
                    }
                    if (reader[6].Equals(System.DBNull.Value))
                    {
                        dd.CreationDate = null;
                    }
                    else
                    {
                        dd.CreationDate = Convert.ToDateTime(reader[6]);
                    }
                    _ddList.Add(dd);
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while fetching the Drug List");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _ddList;
        }

        public List<string> GetAutoCriteria(string criteria, string delivery, string criteriaType)
        {
            List<string> _criteriaList = new List<string>();
            _cmd = new SqlCommand($"SELECT distinct Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] WHERE Delivery = '{delivery}' and  CriteriaType = '{criteriaType}' and Criteria LIKE '%{criteria.Replace("'", "''")}%' and IsActive = 1", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    _criteriaList.Add(reader[0].ToString());
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while fetching the Criteria List");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _criteriaList;
        }

        public List<string> GetAutoTHSTerm(string criteria, string delivery, string criteriaType)
        {
            List<string> _leadTermList = new List<string>();
            _cmd = new SqlCommand($"select leadterm from [dbo].[THSTerm] where LeadTerm like '%{criteria.Replace("'", "''")}%' and IsApproved = 'Y'", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    _leadTermList.Add(reader[0].ToString());
                }
            }
            catch (Exception)
            {
                UserFriendlyMessage.setMessage("Exception while fetching the THSTerm List");
                throw;
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            return _leadTermList;
        }

        public string ExportUsingDatatable(string Delivery, string CriteriaType)
        {
            string pathDownload = "";
            _cmd = new SqlCommand($"select Delivery,CriteriaType,Criteria,TermID,ModificationDate,CreationDate from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{Delivery}' and CriteriaType = '{CriteriaType}' and IsActive = 1", _conn);
            try
            {
                SqlDataAdapter dAdapter = new SqlDataAdapter(_cmd);
                DataTable dTable = new DataTable();
                dAdapter.Fill(dTable);
                string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                pathDownload = Path.Combine(pathUser, "Downloads");

                String ServerPath = System.Web.HttpContext.Current.Server.MapPath("Logs");
                StreamWriter sw = new StreamWriter(ServerPath + @"\LogFile.log", false);
                sw.Write(pathDownload);
                sw.Flush();
                sw.Close();
                

                string FileName = Delivery.Substring(Delivery.IndexOf('.') + 1);
                FileName = char.ToUpper(FileName[0]) + FileName.Substring(1);
                FileName += "_" + CriteriaType + "-" + DateTime.Now.ToString("dd-mm-yyyy-HH-mm-ss-tt") + ".xls";
                ExportToExcelSheet(dTable, pathDownload + @"\" + FileName);
            }
            catch (SqlException)
            {
                UserFriendlyMessage.setMessage("SQL Exception while Exporting to Excel Format");
                throw;
            }
            catch (Exception e)
            {
                UserFriendlyMessage.setMessage("Exception while Exporting to Excel Format");
                throw;
            }
            finally
            {
                _cmd.Dispose();
            }
            return pathDownload;
        }

        public void ExportToExcelSheet(DataTable table, string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath, false);
            sw.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            sw.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
            sw.Write("<BR><BR><BR>");
            sw.Write("<Table border='1' bgColor='#ffffff ' borderColor='#000000 ' cellSpacing='0' cellPadding='0' style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
            int columnscount = table.Columns.Count;

            for (int j = 0; j < columnscount; j++)
            {
                sw.Write("<Td>");
                sw.Write("<B>");
                sw.Write(table.Columns[j].ToString());
                sw.Write("</B>");
                sw.Write("</Td>");
            }
            sw.Write("</TR>");
            foreach (DataRow row in table.Rows)
            {
                sw.Write("<TR>");
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    sw.Write("<Td>");
                    sw.Write(row[i].ToString());
                    sw.Write("</Td>");
                }
                sw.Write("</TR>");
            }
            sw.Write("</Table>");
            sw.Write("</font>");
            sw.Close();
        }
    }
}