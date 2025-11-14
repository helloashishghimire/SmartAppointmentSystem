using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SmartAppointments.App.Data;
using SmartAppointments.App.Models;

namespace SmartAppointments.App.Forms
{
    public class NewAppointmentForm : Form
    {
        ComboBox cboOrg = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        ComboBox cboStaff = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        TextBox txtFirst = new TextBox();
        TextBox txtLast = new TextBox();
        TextBox txtPhone = new TextBox();
        TextBox txtEmail = new TextBox();
        TextBox txtService = new TextBox();
        DateTimePicker dtStart = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };
        DateTimePicker dtEnd = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };
        TextBox txtNotes = new TextBox();
        Button btnSave = new Button { Text = "Save" };
        Button btnCancel = new Button { Text = "Close" };

        public NewAppointmentForm()
        {
            Text = "New Appointment";
            Width = 560; Height = 520;

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            void row(string label, Control c, int h = 34)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
                grid.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, grid.RowCount - 1);
                c.Dock = DockStyle.Fill;
                grid.Controls.Add(c, 1, grid.RowCount - 1);
            }

            row("Organization", cboOrg);
            row("Staff", cboStaff);
            row("Customer First", txtFirst);
            row("Customer Last", txtLast);
            row("Phone", txtPhone);
            row("Email", txtEmail);
            row("Service Type", txtService);
            row("Start Time", dtStart);
            row("End Time", dtEnd);
            row("Notes", txtNotes, 60);

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(12) };
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnCancel);

            Controls.Add(grid);
            Controls.Add(buttons);

            LoadLookups();
            cboOrg.SelectedIndexChanged += (_, __) => LoadStaff();
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;
            btnSave.Click += (_, __) => Save();
        }

        private void LoadLookups()
        {
            using var db = new AppDbContext();
            cboOrg.DataSource = db.Organizations.OrderBy(o => o.Name).ToList();
            cboOrg.DisplayMember = "Name";
            cboOrg.ValueMember = "OrganizationId";

            dtStart.Value = DateTime.Today.AddHours(9);
            dtEnd.Value = DateTime.Today.AddHours(9).AddMinutes(30);

            LoadStaff();
        }

        private void LoadStaff()
        {
            using var db = new AppDbContext();
            var orgId = (int)(cboOrg.SelectedValue ?? 0);
            cboStaff.DataSource = db.Staff.Where(s => s.OrganizationId == orgId).OrderBy(s => s.Name).ToList();
            cboStaff.DisplayMember = "Name";
            cboStaff.ValueMember = "StaffId";
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
            {
                MessageBox.Show("Enter customer's first and last name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtService.Text))
            {
                MessageBox.Show("Enter a service type (e.g., Eye Exam, Road Test, Account Opening).");
                return;
            }
            if (dtEnd.Value <= dtStart.Value)
            {
                MessageBox.Show("End time must be after start time.");
                return;
            }

            // Convert to UTC before touching the database
            var startLocal = dtStart.Value;
            var endLocal = dtEnd.Value;
            var startUtc = startLocal.ToUniversalTime();
            var endUtc = endLocal.ToUniversalTime();

            using var db = new AppDbContext();
            var orgId = (int)cboOrg.SelectedValue;
            var staffId = (int)cboStaff.SelectedValue;

            // find-or-create customer by (first,last,phone)
            var cust = db.Customers.FirstOrDefault(c =>
                c.FirstName == txtFirst.Text.Trim() &&
                c.LastName == txtLast.Text.Trim() &&
                c.Phone == txtPhone.Text.Trim());

            if (cust == null)
            {
                cust = new Customer
                {
                    FirstName = txtFirst.Text.Trim(),
                    LastName = txtLast.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim()
                };
                db.Customers.Add(cust);
                db.SaveChanges();
            }

            // one per day per org per customer (soft check) using UTC day window
            var dayStartUtc = startUtc.Date;
            var dayEndUtc = dayStartUtc.AddDays(1);

            var conflict = db.Appointments.Any(a =>
                a.OrganizationId == orgId &&
                a.CustomerId == cust.CustomerId &&
                a.StartTime >= dayStartUtc &&
                a.StartTime < dayEndUtc &&
                a.Status == "Booked");

            if (conflict)
            {
                MessageBox.Show("This customer already has a booked appointment that day for this organization.");
                return;
            }

            var appt = new Appointment
            {
                OrganizationId = orgId,
                CustomerId = cust.CustomerId,
                StaffId = staffId,
                ServiceType = txtService.Text.Trim(),
                // store UTC in DB
                StartTime = startUtc,
                EndTime = endUtc,
                Status = "Booked",
                Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
            };
            db.Appointments.Add(appt);
            db.SaveChanges();

            MessageBox.Show($"Appointment #{appt.AppointmentId} saved.");
            DialogResult = DialogResult.OK;
        }
    }
}
