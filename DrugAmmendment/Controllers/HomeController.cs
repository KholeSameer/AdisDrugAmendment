using DrugAmmendment.Models;
using DrugAmmendment.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI;
using DrugAmmendment.Services;


namespace DrugAmmendment.Controllers
{
    public class HomeController : Controller, ISecureController
    {
        private IDrugAmendmentConnectionService _drugAmendmentConnectionService = null;
        string siteUrl = System.Configuration.ConfigurationManager.AppSettings["Dashboard"] as string;

        public IAuthorizationProvider AuthorisationProvider { get; private set; }

        public HomeController(IAuthorizationProvider authorisationProvider ,
            IDrugAmendmentConnectionService drugAmendmentConnectionService)
        {
            AuthorisationProvider = authorisationProvider;
            _drugAmendmentConnectionService = drugAmendmentConnectionService;
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult AddDrugView(FormCollection form)
        {
            if (form.Count == 0 && TempData["Client"] == null && TempData["CriteriaType"] == null)
                return Redirect(siteUrl);

            string client = form["Client"];
            string criteriaType = form["criteriaType"];

            if (TempData["Client"] == null)
            {
                TempData["Client"] = client;
            }
            if (TempData["CriteriaType"] == null)
            {
                TempData["CriteriaType"] = criteriaType;
            }
            return View();
        }
        [HttpPost]
        public void AddDrug(FormCollection form)
        {
            string criteriaFromUser = form["criteria"].Trim();
            string delivery = form["client"];
            string criteriaType = form["criteriaType"];
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;
            try
            {
                if (criteriaType == "BrandName")
                {
                    if (string.IsNullOrWhiteSpace(criteriaFromUser))
                    {
                        Response.Write("<script>window.alert(\'Blank drug name is not allowed.\');window.location='AddDrugView'</script>");
                    }
                    bool isAvailableNonActive = false;
                    try
                    {
                        isAvailableNonActive = CheckIsAvailableNonActive(delivery, criteriaType, criteriaFromUser);
                    }
                    catch (Exception)
                    {
                        Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                    }

                    if (isAvailableNonActive)
                    {
                        UpdateToActive(delivery, criteriaType, criteriaFromUser);
                    }
                    else
                    {
                        CheckIsAvailableActive(delivery, criteriaType, criteriaFromUser, null);
                    }

                }
                else
                {
                    bool isValidLeadTerm = false;

                    try
                    {
                        isValidLeadTerm = ValidateLeadTerm(criteriaFromUser);
                    }
                    catch (Exception)
                    {
                        Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                    }

                    if (isValidLeadTerm)
                    {
                        ArrayList ThesData = new ArrayList();
                        try
                        {
                            ThesData = GetDataFromThesTerm(criteriaFromUser);
                        }
                        catch (Exception)
                        {
                            Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                        }

                        string criteria = ThesData[0].ToString();
                        int? termID = Convert.ToInt32(ThesData[1]);
                        bool isAvailableNonActive = false;
                        try
                        {
                            isAvailableNonActive = CheckIsAvailableNonActive(delivery, criteriaType, criteria);
                        }
                        catch (Exception)
                        {
                            Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                        }

                        if (isAvailableNonActive)
                        {
                            UpdateToActive(delivery, criteriaType, criteria);
                        }
                        else
                        {
                            CheckIsAvailableActive(delivery, criteriaType, criteria, termID);
                        }
                    }
                    else
                    {
                        TempData["Client"] = delivery;
                        TempData["CriteriaType"] = criteriaType;
                        Response.Write("<script>window.alert(\'This drug is either not approved or not present in the thesaurus.\');window.location='AddDrugView'</script>");
                    }
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
            }
        }
        private void CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;


            try
            {
                if (_drugAmendmentConnectionService.CheckIsAvailableActive(delivery, criteriaType, criteria, termID) > 0)
                {
                    Response.Write("<script>window.alert(\'This drug is already exists in active drug list.');window.location='AddDrugView'</script>");
                }
                else
                {
                    AddDrugToDB(delivery, criteriaType, criteria, termID);
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
            }


        }
        private void UpdateToActive(string delivery, string criteriaType, string criteria)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            try
            {
                if (_drugAmendmentConnectionService.UpdateToActive(delivery, criteriaType, criteria) > 0)
                {
                    AuditLogger(delivery, criteriaType, criteria, "Active");
                    Response.Write("<script>window.alert(\'The drug have been successfully added.\');window.location='AddDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug Not Updated...! May be this drug is not present in DB...!\');window.location='AddDrugView';</script>");
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView';</script>");
            }
        }
        private bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria)
        {
            bool _flag = false;
            try
            {
                _flag = _drugAmendmentConnectionService.CheckIsAvailableNonActive(delivery, criteriaType, criteria);
            }
            catch (Exception)
            {
                throw;
            }
            return _flag;
        }
        private bool ValidateLeadTerm(string criteria)
        {
            bool _flag = false;

            try
            {
                _flag = _drugAmendmentConnectionService.ValidateLeadTerm(criteria);

            }
            catch (Exception)
            {
                throw;
            }
            return _flag;
        }
        private ArrayList GetDataFromThesTerm(string criteria)
        {
            ArrayList data = new ArrayList();

            try
            {
                data = _drugAmendmentConnectionService.GetDataFromThesTerm(criteria);
            }
            catch (Exception)
            {
                throw;
            }
            return data;
        }
        private void AddDrugToDB(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            try
            {
                if (_drugAmendmentConnectionService.AddDrugToDB(delivery, criteriaType, criteria, termID) > 0)
                {
                    AuditLogger(delivery, criteriaType, criteria, "Add");
                    Response.Write("<script>window.alert(\'The drug have been successfully added.\');window.location='AddDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug is not added.Something went wrong.\');window.location='AddDrugView';</script>");
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView';</script>");
            }

        }
        private void AuditLogger(string Delivery, string CriteriaType, string Criteria, string ActionType)
        {
            TempData["Client"] = Delivery;
            TempData["CriteriaType"] = CriteriaType;


            try
            {
                _drugAmendmentConnectionService.AuditLogger(Delivery, CriteriaType, Criteria, ActionType);

            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView';</script>");
            }
        }

        [HttpGet]
        public JsonResult PopulateClients()
        {
            List<SelectListItem> _clients = new List<SelectListItem>();
            try
            {
                _clients = _drugAmendmentConnectionService.PopulateClients();
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_clients, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PopulateCriteriaType(string ClientName)
        {
            List<SelectListItem> _criteriaType = new List<SelectListItem>();
            try
            {
                _criteriaType = _drugAmendmentConnectionService.PopulateCriteriaType(ClientName);

            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_criteriaType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveDrugList(string Delivery, string CriteriaType)
        {
            List<ExportToExcel> _ddList = new List<ExportToExcel>();

            try
            {
                _ddList = _drugAmendmentConnectionService.GetActiveDrugList(Delivery, CriteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_ddList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> _ddList = new List<DrugDetails>();

            try
            {
                _ddList = _drugAmendmentConnectionService.GetDrugList(ClientName, CriteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_ddList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteDrugView()
        {
            if (Request.QueryString.Count == 0 && TempData["Client"] == null && TempData["CriteriaType"] == null)
                return Redirect(siteUrl);

            if (TempData["Client"] == null)
            {
                TempData["Client"] = Request.QueryString["clientName"];
            }
            if (TempData["CriteriaType"] == null)
            {
                TempData["CriteriaType"] = Request.QueryString["criteriaType"];
            }
            return View();
        }

        public void DeleteDrug(FormCollection form)
        {
            string Criteria = form["criteria"].Trim();
            string CriteriaType = form["criteriaType"];
            string Delivery = form["client"];

            TempData["Client"] = Delivery;
            TempData["CriteriaType"] = CriteriaType;
            bool isAvailableNonActive = false;

            try
            {
                isAvailableNonActive = CheckIsAvailableNonActive(Delivery, CriteriaType, Criteria);
                if (isAvailableNonActive)

                {
                    Response.Write("<script>window.alert(\'This drug is already inactive.\');window.location='DeleteDrugView';</script>");
                }
                else
                {
                    DeleteDrugFromDB(Delivery, CriteriaType, Criteria);
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }

        }

        private void DeleteDrugFromDB(string Delivery, string CriteriaType, string Criteria)
        {

            try
            {
                if (_drugAmendmentConnectionService.DeleteDrugFromDB(Delivery, CriteriaType, Criteria) > 0)
                {
                    AuditLogger(Delivery, CriteriaType, Criteria, "InActive");
                    Response.Write("<script>window.alert(\'The drug have been successfully deleted.\');window.location='DeleteDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'This drug does not exist.\');window.location='DeleteDrugView';</script>");
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }

        }


        public JsonResult GetAutoCriteria(string criteria, string delivery, string criteriaType)
        {
            List<string> _criteriaList = new List<string>();

            try
            {
                _criteriaList = _drugAmendmentConnectionService.GetAutoCriteria(criteria, delivery, criteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            return Json(_criteriaList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAutoTHSTerm(string criteria, string delivery, string criteriaType)
        {
            criteria = criteria.TrimEnd();
            List<string> _leadTermList = new List<string>();

            try
            {
                _leadTermList = _drugAmendmentConnectionService.GetAutoTHSTerm(criteria, delivery, criteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            return Json(_leadTermList, JsonRequestBehavior.AllowGet);
        }

        // Export to Excel
        private void ExportToExcel(DataTable table, string filePath)
        {
            //filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
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

        private void ExporttoExcel(DataTable table)
        {
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.Buffer = true;
            System.Web.HttpContext.Current.Response.ContentType = "application/ms-excel";
            System.Web.HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");
            var FileName = "DrugList - " + DateTime.Now.ToString("dd - mm - yyyy HH - mm - ss tt").Replace(" ", " - ") + ".xls";
            System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName);

            System.Web.HttpContext.Current.Response.Charset = "utf-8";
            System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            System.Web.HttpContext.Current.Response.Write("<font style='font-size:10.0pt; font-family:Calibri;'>");
            System.Web.HttpContext.Current.Response.Write("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            System.Web.HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff ' " +
              "borderColor='#000000 ' cellSpacing='0' cellPadding='0' " +
              "style='font-size:10.0pt; font-family:Calibri; background:white;'> <TR>");
            //am getting my grid's column headers
            int columnscount = table.Columns.Count;

            for (int j = 0; j < columnscount; j++)
            {      //write in new column
                System.Web.HttpContext.Current.Response.Write("<Td>");
                //Get column headers  and make it as bold in excel columns
                System.Web.HttpContext.Current.Response.Write("<B>");
                System.Web.HttpContext.Current.Response.Write(table.Columns[j].ColumnName.ToString());
                System.Web.HttpContext.Current.Response.Write("</B>");
                System.Web.HttpContext.Current.Response.Write("</Td>");
            }
            System.Web.HttpContext.Current.Response.Write("</TR>");
            foreach (DataRow row in table.Rows)
            {//write in new row
                System.Web.HttpContext.Current.Response.Write("<TR>");
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    System.Web.HttpContext.Current.Response.Write("<Td>");
                    System.Web.HttpContext.Current.Response.Write(row[i].ToString());
                    System.Web.HttpContext.Current.Response.Write("</Td>");
                }

                System.Web.HttpContext.Current.Response.Write("</TR>");
            }
            System.Web.HttpContext.Current.Response.Write("</Table>");
            System.Web.HttpContext.Current.Response.Write("</font>");
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
        }

        public JsonResult ExportUsingDatatable(string Delivery, string CriteriaType)
        {
            string _exportMsg = "";
            try
            {
                _exportMsg = _drugAmendmentConnectionService.ExportUsingDatatable(Delivery, CriteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard'</script>");
            }
            if (_exportMsg == "")
            {
                _exportMsg = "Error";
                return Json(_exportMsg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(_exportMsg, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

