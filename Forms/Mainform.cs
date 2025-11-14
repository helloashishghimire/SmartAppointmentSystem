using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SmartAppointments.App.Data;

namespace SmartAppointments.App.Forms
{
    public class MainForm : Form
    {
        private DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private Button btnNew = new Button { Text = "New Appointment" };
        private Button btnRefresh = new Button { Text = "Refresh" };
        private Button btnReschedule = new Button { Text = "Reschedule" };
        private Button btnCancel = new Button { Text = "Cancel" };
        private Button btnStaff = new Button { Text = "Manage Staff" };

        private DateTimePicker dtFilter = new DateTimePicker { Format = DateTimePickerFormat.Short };

        public MainForm()
        {
            Text = "Smart Appointments - Dashboard";
            Width = 1100;
            Height = 650;

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight
            };

            topPanel.Controls.Add(new Label { Text = "Day:", AutoSize = true, Padding = new Padding(5, 10, 5, 0) });
            topPanel.Controls.Add(dtFilter);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnNew);
            topPanel.Controls.Add(btnReschedule);
            topPanel.Controls.Add(btnCancel);
            topPanel.Controls.Add(btnStaff);   // ✅ show the staff button

            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentId", HeaderText = "ID", Width = 50, Name = "AppointmentId" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Organization", HeaderText = "Organization", Width = 200 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Staff", HeaderText = "Staff", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Customer", HeaderText = "Customer", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceType", HeaderText = "Service", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Start", HeaderText = "Start", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "End", HeaderText = "End", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 80 });

            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(grid);

            Controls.Add(mainPanel);
            Controls.Add(topPanel);

            Load += (_, __) => LoadDay();
            btnRefresh.Click += (_, __) => LoadDay();
            btnNew.Click += (_, __) => NewAppointment();
            btnReschedule.Click += (_, __) => RescheduleSelected();
            btnCancel.Click += (_, __) => CancelSelected();
            btnStaff.Click += (_, __) => OpenStaffForm();   // ✅ handle click
        }

        private DateTime SelectedDay => dtFilter.Value.Date;

        private void LoadDay()
        {
            using var db = new AppDbContext();

            // local day from picker
            var localDay = SelectedDay;
            var localNextDay = localDay.AddDays(1);

            // convert to UTC because StartTime is stored as UTC
            var startUtc = localDay.ToUniversalTime();
            var endUtc = localNextDay.ToUniversalTime();

            var appts = db.Appointments
                .Include(a => a.Organization)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Where(a => a.StartTime >= startUtc && a.StartTime < endUtc)
                .OrderBy(a => a.StartTime)
                .ToList();

            // convert back to local for display in the grid
            var items = appts.Select(a => new
            {
                a.AppointmentId,
                Organization = a.Organization!.Name,
                Staff = a.Staff!.Name,
                Customer = a.Customer!.FirstName + " " + a.Customer!.LastName,
                a.ServiceType,
                Start = a.StartTime.ToLocalTime(),
                End = a.EndTime.ToLocalTime(),
                a.Status
            }).ToList();

            grid.DataSource = items;
        }

        private int? SelectedAppointmentId()
        {
            if (grid.CurrentRow == null) return null;
            if (grid.CurrentRow.Cells["AppointmentId"].Value is int id) return id;
            return null;
        }

        private void NewAppointment()
        {
            using var form = new NewAppointmentForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadDay();
            }
        }

        private void RescheduleSelected()
        {
            var id = SelectedAppointmentId();
            if (id == null)
            {
                MessageBox.Show("Please select an appointment to reschedule.");
                return;
            }

            using var form = new RescheduleForm(id.Value);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadDay();
            }
        }

        private void CancelSelected()
        {
            var id = SelectedAppointmentId();
            if (id == null)
            {
                MessageBox.Show("Please select an appointment to cancel.");
                return;
            }

            using var db = new AppDbContext();
            var appt = db.Appointments.Find(id.Value);
            if (appt == null)
            {
                MessageBox.Show("Appointment not found.");
                return;
            }

            appt.Status = "Cancelled";
            db.SaveChanges();
            LoadDay();
        }

        // ✅ opens the staff management form
        private void OpenStaffForm()
        {
            using var form = new ManageStaffForm();
            form.ShowDialog(this);
        }
    }
}
