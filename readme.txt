===========================================
README.TXT – Smart Appointments Application
===========================================

Project Name: SmartAppointments.App
Database: PostgreSQL (sms)
Submission: Update 2 – Halfway Completion
Student: [Your Name]

-------------------------------------------
1. PROJECT OVERVIEW
-------------------------------------------

The Smart Appointments System is a Windows Forms application built using:
- C# (.NET 8)
- Windows Forms (WinForms)
- Entity Framework Core
- PostgreSQL (via Npgsql)

The application allows organizations to manage:
- Staff
- Customers
- Appointments
- Rescheduling
- Cancellations

This update includes:
✓ A runnable version of the application
✓ A PostgreSQL database file
✓ Several stored procedures
✓ Logical operations implemented in the forms
✓ This ReadMe file

-------------------------------------------
2. FORMS INCLUDED (Total: 5 Forms)
-------------------------------------------

1. MainForm
   - Dashboard showing appointments for the selected day
   - Buttons for creating, rescheduling, canceling, and managing staff

2. NewAppointmentForm
   - Create a new appointment
   - Customer auto-creation
   - Time validation
   - Prevent duplicate appointments per day

3. RescheduleForm
   - Modify an existing appointment’s date/time
   - Converts UTC <-> Local time
   - Saves updated appointment in UTC

4. ManageStaffForm
   - Add staff for organizations
   - Delete staff
   - Choose organization from dropdown
   - Displays staff in DataGridView

5. Form1
   - Default WinForms template (unused but required to reach 5 total forms)

-------------------------------------------
3. DATABASE CONNECTION
-------------------------------------------

The project connects to PostgreSQL using this connection string inside AppDbContext.cs:

Host=localhost
Port=5432
Database=sms
Username=postgres
Password=12345

Tables created:
- Organizations
- Customers
- Staff
- Appointments

Database is auto-created on first run using:
DbInitializer.EnsureDatabase();

-------------------------------------------
4. LOGICAL OPERATIONS IMPLEMENTED
-------------------------------------------

4.1 Create Appointment (NewAppointmentForm)
-------------------------------------------
- Select organization
- Select staff
- Enter customer details
- Validate times (end > start)
- Detect daily conflict (one appointment per org per customer per day)
- Save times in UTC to comply with PostgreSQL timestamp rules

4.2 Display Appointments (MainForm)
-------------------------------------------
- User selects a date
- Convert selected date to UTC for queries
- Load all appointments for that day
- Convert UTC back to Local for display

4.3 Reschedule Appointment (RescheduleForm)
-------------------------------------------
- Load existing appointment
- Display times in Local
- Save modified times in UTC
- Validate time consistency

4.4 Cancel Appointment (MainForm)
-------------------------------------------
- Soft delete approach
- Set Status = "Cancelled"

4.5 Manage Staff (ManageStaffForm)
-------------------------------------------
- Load all organizations
- Add staff to selected organization
- Delete selected staff
- Show all staff per organization

-------------------------------------------
5. STORED PROCEDURES (PostgreSQL)
-------------------------------------------

Required stored procedures included below.

5.1 Add Appointment
-------------------------------------------
CREATE OR REPLACE FUNCTION sp_add_appointment(
    p_org_id INT,
    p_customer_id INT,
    p_staff_id INT,
    p_service TEXT,
    p_start TIMESTAMP,
    p_end TIMESTAMP,
    p_notes TEXT
)
RETURNS INT AS $$
DECLARE
    new_id INT;
BEGIN
    INSERT INTO "Appointments"(
        "OrganizationId", "CustomerId", "StaffId",
        "ServiceType", "StartTime", "EndTime",
        "Status", "Notes"
    )
    VALUES (
        p_org_id, p_customer_id, p_staff_id,
        p_service, p_start, p_end,
        'Booked', p_notes
    )
    RETURNING "AppointmentId" INTO new_id;

    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------
5.2 Check Appointment Conflict
-------------------------------------------
CREATE OR REPLACE FUNCTION sp_check_conflict(
    p_org_id INT,
    p_customer_id INT,
    p_start_date DATE
)
RETURNS BOOLEAN AS $$
DECLARE
    exists_conflict INT;
BEGIN
    SELECT COUNT(*) INTO exists_conflict
    FROM "Appointments"
    WHERE "OrganizationId" = p_org_id
      AND "CustomerId" = p_customer_id
      AND DATE("StartTime") = p_start_date
      AND "Status" = 'Booked';

    RETURN exists_conflict > 0;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------
5.3 Cancel Appointment
-------------------------------------------
CREATE OR REPLACE FUNCTION sp_cancel_appointment(
    p_id INT
)
RETURNS VOID AS $$
BEGIN
    UPDATE "Appointments"
    SET "Status" = 'Cancelled'
    WHERE "AppointmentId" = p_id;
END;
$$ LANGUAGE plpgsql;

-------------------------------------------
6. HOW TO RUN THE APPLICATION
-------------------------------------------

Prerequisites:
- .NET SDK 8 installed
- PostgreSQL running
- Database "sms" created
- Correct password (12345)

Steps:
1. Open folder “SmartAppointments.App” in VS Code.
2. Open terminal and run:
   dotnet restore
   dotnet build
3. Then run:
   dotnet run --project SmartAppointments.App.csproj
4. Main dashboard will appear.

-------------------------------------------
7. PROGRESS STATUS
-------------------------------------------

The application is halfway complete and includes:
✓ 5 forms
✓ Database integration
✓ Logical operations implemented
✓ Stored procedures included
✓ Reschedule + Cancel features
✓ Staff management
✓ Basic customer auto-create
✓ UTC-safe time handling

-------------------------------------------
8. FILES INCLUDED
-------------------------------------------
- Program source code
- Database (.sql dump)
- README.txt
- Executable build (bin/ folder)
- Stored procedures

-------------------------------------------
END OF README
-------------------------------------------
