using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class OpportunitySearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public List<string>? Skills { get; set; }
        public string? SortBy { get; set; } = "Date"; // Date, Title, Duration, Volunteers
        public string? SortOrder { get; set; } = "Desc"; // Asc, Desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class VolunteerSearchDto
    {
        public string? SearchTerm { get; set; }
        public List<string>? Skills { get; set; }
        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }
        public string? Location { get; set; }
        public string? Availability { get; set; }
        public double? MinRating { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "Rating"; // Name, Rating, Experience, JoinDate
        public string? SortOrder { get; set; } = "Desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class OrganizationSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; }
        public bool? IsVerified { get; set; }
        public string? SortBy { get; set; } = "Name"; // Name, Rating, CreatedDate
        public string? SortOrder { get; set; } = "Asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SearchResultDto<T>
    {
        public List<T> Results { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        public SearchFilters? AppliedFilters { get; set; }
    }

    public class SearchFilters
    {
        public List<FilterOption> Categories { get; set; } = new List<FilterOption>();
        public List<FilterOption> Locations { get; set; } = new List<FilterOption>();
        public List<FilterOption> Skills { get; set; } = new List<FilterOption>();
        public List<FilterOption> Organizations { get; set; } = new List<FilterOption>();
        public DateRange? DateRange { get; set; }
        public NumberRange? DurationRange { get; set; }
        public NumberRange? ExperienceRange { get; set; }
    }

    public class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DateRange
    {
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
    }

    public class NumberRange
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class QuickSearchDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Query { get; set; } = string.Empty;
        
        public string? Type { get; set; } = "all"; // all, opportunities, volunteers, organizations
        public int Limit { get; set; } = 5;
    }

    public class QuickSearchResultDto
    {
        public List<SearchSuggestion> Opportunities { get; set; } = new List<SearchSuggestion>();
        public List<SearchSuggestion> Volunteers { get; set; } = new List<SearchSuggestion>();
        public List<SearchSuggestion> Organizations { get; set; } = new List<SearchSuggestion>();
        public int TotalResults { get; set; }
    }

    public class SearchSuggestion
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        public string Type { get; set; } = string.Empty; // opportunity, volunteer, organization
        public string Url { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
    }
}