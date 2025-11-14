using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SmartAppointments.App.Data;

namespace SmartAppointments.App.Forms
{
    public class RescheduleForm : Form
    {
        private readonly int _appointmentId;
        private Label lblInfo = new Label { AutoSize = true };
        private DateTimePicker dtStart = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };
        private DateTimePicker dtEnd = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };
        private Button btnSave = new Button { Text = "Save" };
        private Button btnClose = new Button { Text = "Close" };

        public RescheduleForm(int appointmentId)
        {
            _appointmentId = appointmentId;

            Text = "Reschedule Appointment";
            Width = 450;
            Height = 250;

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            void row(string label, Control c, int h = 34)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
                grid.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, grid.RowCount - 1);
                c.Dock = DockStyle.Fill;
                grid.Controls.Add(c, 1, grid.RowCount - 1);
            }

            row("Appointment", lblInfo);
            row("Start Time", dtStart);
            row("End Time", dtEnd);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);

            Controls.Add(grid);
            Controls.Add(buttons);

            Load += (_, __) => LoadAppointment();
            btnSave.Click += (_, __) => Save();
            btnClose.Click += (_, __) => Close();
        }

        private void LoadAppointment()
        {
            using var db = new AppDbContext();

            var appt = db.Appointments
                .Include(a => a.Organization)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .FirstOrDefault(a => a.AppointmentId == _appointmentId);

            if (appt == null)
            {
                MessageBox.Show("Appointment not found.");
                Close();
                return;
            }

            lblInfo.Text = $"{appt.Organization!.Name} / {appt.Staff!.Name} / {appt.Customer!.FirstName} {appt.Customer!.LastName}";

            // convert UTC from DB to local time for UI
            dtStart.Value = appt.StartTime.ToLocalTime();
            dtEnd.Value = appt.EndTime.ToLocalTime();
        }

        private void Save()
        {
            if (dtEnd.Value <= dtStart.Value)
            {
                MessageBox.Show("End time must be after start time.");
                return;
            }

            // convert back to UTC before saving
            var startUtc = dtStart.Value.ToUniversalTime();
            var endUtc = dtEnd.Value.ToUniversalTime();

            using var db = new AppDbContext();

            var appt = db.Appointments.Find(_appointmentId);
            if (appt == null)
            {
                MessageBox.Show("Appointment not found.");
                return;
            }

            appt.StartTime = startUtc;
            appt.EndTime = endUtc;

            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
