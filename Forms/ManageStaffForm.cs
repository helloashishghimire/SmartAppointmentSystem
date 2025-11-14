using System;
using System.Linq;
using System.Windows.Forms;
using SmartAppointments.App.Data;
using SmartAppointments.App.Models;

namespace SmartAppointments.App.Forms
{
    public class ManageStaffForm : Form
    {
        private ComboBox cboOrg = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private Button btnAdd = new Button { Text = "Add Staff" };
        private Button btnDelete = new Button { Text = "Delete Staff" };

        public ManageStaffForm()
        {
            Text = "Manage Staff";
            Width = 700;
            Height = 450;

            // top bar
            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight
            };

            topPanel.Controls.Add(new Label
            {
                Text = "Organization:",
                AutoSize = true,
                Padding = new Padding(5, 10, 5, 0)
            });
            topPanel.Controls.Add(cboOrg);
            topPanel.Controls.Add(btnAdd);
            topPanel.Controls.Add(btnDelete);

            // grid columns
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StaffId",
                HeaderText = "ID",
                Width = 60,
                Name = "StaffId"
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Name",
                Width = 250
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Role",
                HeaderText = "Role",
                Width = 250
            });

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(grid);

            Controls.Add(mainPanel);
            Controls.Add(topPanel);

            // events
            Load += (_, __) => LoadOrganizations();
            cboOrg.SelectedIndexChanged += (_, __) => LoadStaffForSelectedOrg();
            btnAdd.Click += (_, __) => AddStaff();
            btnDelete.Click += (_, __) => DeleteSelectedStaff();
        }

        // Load all organizations into combo box
        private void LoadOrganizations()
        {
            using var db = new AppDbContext();
            var orgs = db.Organizations
                .OrderBy(o => o.Name)
                .ToList();

            cboOrg.DataSource = orgs;
            cboOrg.DisplayMember = "Name";
            cboOrg.ValueMember = "OrganizationId";

            if (orgs.Count > 0)
                cboOrg.SelectedIndex = 0;

            LoadStaffForSelectedOrg();
        }

        // Load staff belonging to the selected organization
        private void LoadStaffForSelectedOrg()
        {
            if (cboOrg.SelectedItem is not Organization org)
            {
                grid.DataSource = null;
                return;
            }

            using var db = new AppDbContext();
            var staff = db.Staff
                .Where(s => s.OrganizationId == org.OrganizationId)
                .OrderBy(s => s.Name)
                .ToList();

            grid.DataSource = staff;
        }

        // Add new staff for selected organization
        private void AddStaff()
        {
            if (cboOrg.SelectedItem is not Organization org)
            {
                MessageBox.Show("Please select an organization first.");
                return;
            }

            // simple input dialogs (ok for assignment-level project)
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter staff name:",
                "Add Staff",
                "");

            if (string.IsNullOrWhiteSpace(name))
                return;

            var role = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter staff role (e.g., Teller, Doctor, Examiner):",
                "Add Staff",
                "");

            if (string.IsNullOrWhiteSpace(role))
                return;

            using var db = new AppDbContext();
            var staff = new Staff
            {
                OrganizationId = org.OrganizationId,
                Name = name.Trim(),
                Role = role.Trim()
            };

            db.Staff.Add(staff);
            db.SaveChanges();

            LoadStaffForSelectedOrg();
        }

        // Delete selected staff row
        private void DeleteSelectedStaff()
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Please select a staff member to delete.");
                return;
            }

            if (grid.CurrentRow.Cells["StaffId"].Value is not int staffId)
            {
                MessageBox.Show("Unable to determine selected staff record.");
                return;
            }

            var confirm = MessageBox.Show(
                "Are you sure you want to delete this staff member?\n" +
                "Existing appointments may no longer have a valid staff reference.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            using var db = new AppDbContext();
            var staff = db.Staff.Find(staffId);
            if (staff == null)
            {
                MessageBox.Show("Staff not found.");
                return;
            }

            db.Staff.Remove(staff);
            db.SaveChanges();

            LoadStaffForSelectedOrg();
        }
    }
}
