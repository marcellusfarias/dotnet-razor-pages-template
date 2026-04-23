namespace MyBuyingList.Application.Common.Exceptions;

public class ErrorResponse
{
    public required List<ErrorDetail> Errors { get; init; }

    public static ErrorResponse CreateSingleErrorDetail(string title, string detail)
    {
        ErrorDetail item = new()
        {
            Title = title,
            Detail = detail,
        };

        return new ErrorResponse
        {
            Errors = [item]
        };
    }
}

public class ErrorDetail
{
    public required string Title { get; set; }

    public required string Detail { get; set; }
}