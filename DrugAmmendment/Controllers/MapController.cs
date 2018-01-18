using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DrugAmmendment.Controllers
{
    public class MapController : Controller
    {
        public Dictionary<string, string> dict = new Dictionary<string, string>();

        private string _connectionString;
        private SqlConnection _conn = null;
        private SqlCommand _cmd = null;

        public JsonResult DeliveryMapping()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            _conn = new SqlConnection(_connectionString);

            _cmd = new SqlCommand("select Description,Delivery from [dbo].[ADFeedDeliveryMapping] where IsActive = 1", _conn);
            try
            {
                _conn.Open();
                SqlDataReader reader = _cmd.ExecuteReader();
                while (reader.Read())
                {
                    dict[(string)reader["Delivery"]] = (string)reader["Description"];
                }
            }
            catch (Exception e)
            {
                Response.Write("Mapping Exception");
            }
            finally
            {
                _conn.Close();
                _cmd.Dispose();
            }
            JsonResult jsonData = Json(dict, JsonRequestBehavior.AllowGet);
            return jsonData;
        }
    }
}