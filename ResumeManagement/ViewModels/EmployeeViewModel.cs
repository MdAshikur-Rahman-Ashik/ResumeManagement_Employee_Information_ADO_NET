using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeManagement.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool IsPermanent { get; set; }
        public int TotalExperience { get; set; }
        public string DesignationTitle { get; set; }
        public string ImagePath { get; set; }
    }
}
