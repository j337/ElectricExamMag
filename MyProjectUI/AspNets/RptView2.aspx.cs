using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyProjectUI.Areas.InformationManage.Models;
namespace MyProjectUI.AspNets
{
    public partial class RptView2 : System.Web.UI.Page
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

            reportdatsource.Name = "SolveRpt";

            List<DataEliminate> list = new List<DataEliminate>();
            List<DataEliminate> dmlist = new List<DataEliminate>();

            if (Session["solvelist"] != null)
            {
                //将session转为集合 
                list = (List<DataEliminate>)Session["solvelist"];
                foreach (var v in list)
                {
                   
                    DataEliminate dmm = new DataEliminate();
                    dmm.solveTaskCode = v.solveTaskCode;
                    dmm.solveTaskName = v.solveTaskName;
                    dmm.lineCode = v.lineCode;
                    dmm.poleCode = v.poleCode;
                    dmm.startPoleCode = v.startPoleCode;
                    dmm.endPoleCode = v.endPoleCode;
                    dmm.bugLevelName = v.bugLevelName;
                    dmm.bugTypeName = v.bugTypeName;
                    if (v.isBug == 1)
                    {
                        dmm.BugName = "有";
                    }
                    else
                    {
                        dmm.BugName = "无";
                    }
                    dmm.finishTime2 = v.finishTime.ToString();
                    dmm.discoverTime2 = v.discoverTime.ToString();
                    dmm.bugDesc = v.bugDesc;

                    dmlist.Add(dmm);
                }
            }

            reportdatsource.Value = dmlist;

            this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Models/SolveReport.rdlc");

            this.ReportViewer1.LocalReport.DataSources.Add(reportdatsource);

            this.ReportViewer1.LocalReport.Refresh();
        }
    }
}