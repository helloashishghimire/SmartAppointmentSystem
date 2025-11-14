namespace SmartAppointments.App.Models
{
    // Any business using the system (bank branch, hospital, DMV office, etc.)
    public class Organization
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string? Location { get; set; }  // city/address
    }

    // The person booking (patient, client, citizen, customer)
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public override string ToString() => $"{FirstName} {LastName}";
    }

    // The staff member who provides the service (teller, nurse, examiner, etc.)
    public class Staff
    {
        public int StaffId { get; set; }
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = ""; // e.g., Teller, Nurse, Examiner
        public override string ToString() => $"{Name} ({Role})";
    }

    // Bookings with simple rescheduling
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int StaffId { get; set; }
        public Staff? Staff { get; set; }

        public string ServiceType { get; set; } = ""; // e.g., "Account Opening", "Eye Exam", "Road Test"

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Status { get; set; } = "Booked"; // Booked|Cancelled|Completed
        public string? Notes { get; set; }
    }
}
