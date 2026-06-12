namespace Backend.Application.DTOs;

public sealed record FeedbackRequest(string Country, int Rating, string Name, string Comment = "");
