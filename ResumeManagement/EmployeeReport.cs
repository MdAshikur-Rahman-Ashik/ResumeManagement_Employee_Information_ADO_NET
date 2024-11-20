using ResumeManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResumeManagement
{
    public partial class EmployeeReport : Form
    {
        List<EmployeeViewModel> _list;
        public EmployeeReport(List<EmployeeViewModel> list)
        {
            InitializeComponent();
            _list= list;
        }

        private void EmployeeReport_Load(object sender, EventArgs e)
        {
            RptEmployeeInfo rpt=new RptEmployeeInfo();
            rpt.SetDataSource(_list);
            crystalReportViewer1.ReportSource = rpt;
            crystalReportViewer1.Refresh();
        }
    }
}
