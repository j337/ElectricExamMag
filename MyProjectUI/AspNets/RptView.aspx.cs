using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyProjectUI.Areas.InformationManage.Models;
namespace MyProjectUI.AspNets
{
    public partial class RptView : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ShowData();
            }
        }
        public void ShowData()
        {
            this.ReportViewer1.Reset();
            this.ReportViewer1.LocalReport.Dispose();
            this.ReportViewer1.LocalReport.DataSources.Clear();

            Microsoft.Reporting.WebForms.ReportDataSource reportdatsource = new Microsoft.Reporting.WebForms.ReportDataSource();

            reportdatsource.Name = "DataSet1";

          
            List<DataManager> dmlist = new List<DataManager>();
            List<DataManager> dmlist2 = new List<DataManager>();

            if (Session["inspectlist"] != null)
            {
                //将session转为集合 
                dmlist = (List<DataManager>)Session["inspectlist"];

               foreach(var v in dmlist)
                {
                    v.discoverTime2 = v.discoverTime.ToString();
                }

            }

            reportdatsource.Value = dmlist;

            this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Models/InspectReport.rdlc");

            this.ReportViewer1.LocalReport.DataSources.Add(reportdatsource);

            this.ReportViewer1.LocalReport.Refresh();

        }
    }
}