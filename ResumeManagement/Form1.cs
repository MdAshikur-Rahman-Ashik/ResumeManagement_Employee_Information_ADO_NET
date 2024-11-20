using ResumeManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResumeManagement
{
    public partial class Form1 : Form
    {
        string conStr = ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        int intEmpId = 0;
        string strPreviousImage = "";
        bool defaultImage = true;
        OpenFileDialog ofd=new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
            LoadDesignationCmb();
            LoaddgvEmployeeList();
            Clear();
        }
        private void LoaddgvEmployeeList()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllEmployees",con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt=new DataTable();
                sda.Fill(dt);
                dt.Columns.Add("Image", Type.GetType("System.Byte[]"));
                foreach (DataRow dr in dt.Rows)
                {
                    dr["Image"] = File.ReadAllBytes(Application.StartupPath + "\\images\\"+dr["ImagePath"].ToString());
                }
                dgvEmployeeList.RowTemplate.Height = 80;
                dgvEmployeeList.DataSource = dt;

                ((DataGridViewImageColumn)dgvEmployeeList.Columns[dgvEmployeeList.Columns.Count - 1]).ImageLayout = DataGridViewImageCellLayout.Stretch;

                sda.Dispose();               
            }
        }
        private void LoadDesignationCmb()
        {
            using (SqlConnection con=new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Designation", con);
                DataTable dt=new DataTable();
                sda.Fill(dt);
                DataRow topRow = dt.NewRow();
                topRow[0] = 0;
                topRow[1] = "--Select--";
                dt.Rows.InsertAt(topRow, 0);
                cmbDesignation.ValueMember = "DesignationId";
                cmbDesignation.DisplayMember = "DesignationTitle";
                cmbDesignation.DataSource= dt;
            }
        }
        private void Clear()
        {
            txtEmpCode.Text="";
            txtEmployeeName.Text = "";
            cmbDesignation.SelectedIndex = 0;
            dtpDOB.Value=DateTime.Now;
            rbtnMale.Checked = true;
            chkStatus.Checked=true;
            intEmpId = 0;
            btnDelete.Enabled = false;
            btnSave.Text = "Save";
            pictureBoxEmployee.Image = Image.FromFile(Application.StartupPath + "\\images\\noimage.png");
            defaultImage = true;
            if (dgvExperiences.DataSource == null)
            {
                dgvExperiences.Rows.Clear();
            }
            else
            {
                dgvExperiences.DataSource = (dgvExperiences.DataSource as DataTable).Clone();
            }
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png,.png)|*.png;*.jpg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBoxEmployee.Image=new Bitmap(ofd.FileName);
                if (intEmpId == 0)
                {
                    defaultImage = false;
                    strPreviousImage = "";
                }
                
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            pictureBoxEmployee.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.png");
            defaultImage = true;
            strPreviousImage = "";
        }
        bool ValidateMasterDetailForm()
        {
            bool isValid = true;
            if (txtEmployeeName.Text.Trim() == "")
            {
                MessageBox.Show("Employee name is required");
                isValid = false;
            }
            return isValid;
        }
        string SaveImage(string imgPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string ext = Path.GetExtension(imgPath);
            fileName = fileName.Length <= 15 ? fileName : fileName.Substring(0, 15);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + ext;
            pictureBoxEmployee.Image.Save(Application.StartupPath + "\\images\\"+ fileName);
            return fileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int empId = 0;
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("EmployeeAddOrEdit", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeId", intEmpId);
                    cmd.Parameters.AddWithValue("@EmployeeCode", txtEmpCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@EmployeeName", txtEmployeeName.Text.Trim());
                    cmd.Parameters.AddWithValue("@DesignationId", Convert.ToInt16(cmbDesignation.SelectedValue));
                    cmd.Parameters.AddWithValue("@DateOfBirth", dtpDOB.Value);
                    cmd.Parameters.AddWithValue("@IsPermanent", chkStatus.Checked? "True":"False");
                    cmd.Parameters.AddWithValue("@Gender", rbtnMale.Checked ? "Male" : "Female");
                    if (defaultImage)
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    }
                   
                    else if(intEmpId>0 && strPreviousImage!="")
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                        //if(ofd.FileName!= strPreviousImage)
                        //{
                        //    var filename = Application.StartupPath + "\\images\\" + strPreviousImage;
                        //    if (pictureBoxEmployee.Image != null)
                        //    {
                        //        pictureBoxEmployee.Image.Dispose();
                        //        pictureBoxEmployee.Image = null;
                        //        System.IO.File.Delete(filename);
                        //    }
                        //}
                      
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    }
                    empId=Convert.ToInt16( cmd.ExecuteScalar());
                }
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    foreach (DataGridViewRow item in dgvExperiences.Rows)
                    {
                        if (item.IsNewRow) break;
                        else
                        {
                            SqlCommand cmd = new SqlCommand("EmpExperienceAddAndEdit", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ExperienceId", Convert.ToInt32(item.Cells["dgvExperienceId"].Value == DBNull.Value? "0": item.Cells["dgvExperienceId"].Value));
                            cmd.Parameters.AddWithValue("@EmployeeId", empId);
                            cmd.Parameters.AddWithValue("@CompanyName", item.Cells["dgvCompanyName"].Value);
                            cmd.Parameters.AddWithValue("@YearsWorked", item.Cells["dgvYearsWorked"].Value);
                            cmd.ExecuteNonQuery();
                        }                        
                    }                    
                }
                LoaddgvEmployeeList();
                Clear();
                MessageBox.Show("Submitted Successfully");           
            }
        }

        private void dgvEmployeeList_DoubleClick(object sender, EventArgs e)
        {
            if (dgvEmployeeList.CurrentRow.Index != -1)
            {
                DataGridViewRow dgvRow = dgvEmployeeList.CurrentRow;
                intEmpId = Convert.ToInt32(dgvRow.Cells[0].Value);
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewEmployeeByEmployeeId",con);
                    sda.SelectCommand.CommandType= CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@EmployeeId", intEmpId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    //--Master---
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtEmpCode.Text = dr["EmployeeCode"].ToString();
                    txtEmployeeName.Text = dr["EmployeeName"].ToString();
                    cmbDesignation.SelectedValue = Convert.ToInt32(dr["DEsignationId"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DateOfBirth"].ToString());
                    if (Convert.ToBoolean(dr["IsPermanent"].ToString())){
                        chkStatus.Checked = true;
                    }
                    else
                    {
                        chkStatus.Checked = false;
                    }
                   if ((dr["Gender"].ToString().Trim())=="Male")
                    {
                        rbtnMale.Checked = true;
                    }
                    else
                    {
                        rbtnMale.Checked = false;
                    }
                    if ((dr["Gender"].ToString().Trim()) == "Female")
                    {
                        rbtnFemale.Checked = true;
                    }
                    else
                    {
                        rbtnFemale.Checked = false;
                    }
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pictureBoxEmployee.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.png");
                    }
                    else
                    {
                        string image = dr["ImagePath"].ToString();
                        pictureBoxEmployee.Image = new Bitmap(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());                 
                        strPreviousImage = dr["ImagePath"].ToString();
                        defaultImage = false;
                    }
                    //--Details---
                    dgvExperiences.AutoGenerateColumns = false;
                    dgvExperiences.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes) 
            {
                string image = "";
               using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewEmployeeByEmployeeId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@EmployeeId", intEmpId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr["ImagePath"] != DBNull.Value)
                    {
                        image = dr["ImagePath"].ToString();
                        var filename = Application.StartupPath + "\\images\\" + image;
                        if (pictureBoxEmployee.Image != null)
                        {
                            pictureBoxEmployee.Image.Dispose();
                            pictureBoxEmployee.Image = null;
                            System.IO.File.Delete(filename);
                        }
                      
                    }
                    SqlCommand cmd = new SqlCommand("EmployeeExperienceDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeId", intEmpId);
                    sda.Dispose();
                    cmd.ExecuteNonQuery();
                    LoaddgvEmployeeList();
                    Clear();
                    MessageBox.Show("Deleted Successfully");
                }
               // File.Delete(filePath);
            }

        }

        private void dgvExperiences_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvExperiences.CurrentRow;
            if (dgvRow.Cells["dgvExperienceId"].Value!=DBNull.Value)
            {
                if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("ExperienceDelete", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ExperienceId", dgvRow.Cells["dgvExperienceId"].Value);
                        cmd.ExecuteNonQuery();
                    }

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllEmployees", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                List<EmployeeViewModel> list = new List<EmployeeViewModel>();
                EmployeeViewModel employeeVm;
                if(dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        employeeVm=new EmployeeViewModel();
                        employeeVm.EmployeeId = Convert.ToInt32(dt.Rows[i]["EmployeeId"]);
                        employeeVm.EmployeeCode = dt.Rows[i]["EmployeeCode"].ToString();
                        employeeVm.EmployeeName = dt.Rows[i]["EmployeeName"].ToString();
                        employeeVm.DateOfBirth = Convert.ToDateTime(dt.Rows[i]["DateOfBirth"].ToString());
                        employeeVm.Gender = dt.Rows[i]["Gender"].ToString();
                        employeeVm.IsPermanent = Convert.ToBoolean(dt.Rows[i]["IsPermanent"].ToString());
                        employeeVm.TotalExperience = Convert.ToInt32(dt.Rows[i]["TotalExperience"]);
                        employeeVm.DesignationTitle = dt.Rows[i]["DesignationTitle"].ToString();
                        employeeVm.ImagePath = Application.StartupPath + "\\images\\"+dt.Rows[i]["ImagePath"].ToString();
                        list.Add(employeeVm);
                        
                    }
                    using (EmployeeReport report = new EmployeeReport(list))
                    {
                        report.ShowDialog();
                    }
                }


            }
        }
    }
}
