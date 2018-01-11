using DrugAmmendment.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DrugAmmendment.Services
{
    public interface IDrugAmendmentConnectionService
    {
        string Username { get; }

        int UpdateToActive(string delivery, string criteriaType, string criteria);
        bool ValidateLeadTerm(string criteria);
        ArrayList GetDataFromThesTerm(string criteria);
        bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria);
        int CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int? termID);
        int AddDrugToDB(string delivery, string criteriaType, string criteria, int? termID);
        int? GetTermID(string delivery, string criteriaType, string criteria);
        void AuditLogger(string Delivery, string CriteriaType, string Criteria, string ActionType);

        int DeleteDrugFromDB(string Delivery, string CriteriaType, string Criteria);

        List<SelectListItem> PopulateClients();
        List<SelectListItem> PopulateCriteriaType(string ClientName);
        List<ExportToExcel> GetActiveDrugList(string ClientName, string CriteriaType);
        List<DrugDetails> GetDrugList(string ClientName, string CriteriaType);
        List<string> GetAutoCriteria(string criteria, string delivery, string criteriaType);
        List<string> GetAutoTHSTerm(string criteria, string delivery, string criteriaType);

        string ExportUsingDatatable(string Delivery, string CriteriaType);
        void ExportToExcelSheet(DataTable table, string filePath);
    }
}
