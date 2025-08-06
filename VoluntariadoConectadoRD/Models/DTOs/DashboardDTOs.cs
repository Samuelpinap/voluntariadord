namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalOrganizations { get; set; }
        public int TotalOpportunities { get; set; }
        public int ActiveOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int CompletedOpportunities { get; set; }
        public double AverageRating { get; set; }
        public int TotalVolunteerHours { get; set; }
    }

    public class UserDashboardDto
    {
        public int UserId { get; set; }
        public int ApplicationsCount { get; set; }
        public int ApprovedApplications { get; set; }
        public int CompletedActivities { get; set; }
        public int VolunteerHours { get; set; }
        public double AverageRating { get; set; }
        public int BadgesCount { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
        public List<OpportunityListDto> SuggestedOpportunities { get; set; } = new List<OpportunityListDto>();
    }

    public class OrganizationDashboardDto
    {
        public int OrganizationId { get; set; }
        public int OpportunitiesCreated { get; set; }
        public int ActiveOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int TotalVolunteers { get; set; }
        public double AverageRating { get; set; }
        public List<RecentApplicationDto> RecentApplications { get; set; } = new List<RecentApplicationDto>();
        public List<OpportunityListDto> RecentOpportunities { get; set; } = new List<OpportunityListDto>();
    }

    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // "applied", "approved", "completed", "created"
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string OpportunityTitle { get; set; } = string.Empty;
    }

    public class RecentApplicationDto
    {
        public int Id { get; set; }
        public string VolunteerName { get; set; } = string.Empty;
        public string OpportunityTitle { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public ApplicationStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public class DashboardChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "bar"; // bar, line, pie, etc.
    }

    public class OrganizationStatsDto
    {
        public int ActiveDonors { get; set; }
        public int EventsRealized { get; set; }
        public decimal TotalDonations { get; set; }
        public int ActiveProjects { get; set; }
        public decimal AverageDonation { get; set; }
        public int NewDonors { get; set; }
        public decimal RecurrentDonorsPercentage { get; set; }
        public int PeopleBenefited { get; set; }
        public int CommunitiesReached { get; set; }
        public int GrowthPercentage { get; set; }
        public int NewCommunities { get; set; }
        public List<MonthlyDonationDto> MonthlyDonations { get; set; } = new List<MonthlyDonationDto>();
        public List<ImpactDistributionDto> ImpactDistribution { get; set; } = new List<ImpactDistributionDto>();
    }

    public class MonthlyDonationDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ImpactDistributionDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class OrganizationEventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public int Participants { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
    }

    public class UserEventDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public int DurationHours { get; set; }
        public string ApplicationStatus { get; set; } = string.Empty;
        public string EventStatus { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}