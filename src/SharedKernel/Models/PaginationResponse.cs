using System.Text.Json.Serialization;

namespace SharedKernel.Models;

public class PaginationResponse<T>
{
    public IEnumerable<T>? Data { get; private set; }

    public Paging<T>? Paging { get; private set; }

    public PaginationResponse(
        IEnumerable<T> data,
        int totalItemCount,
        int currentPage,
        int pageSize
    )
    {
        Data = data;
        Paging = new Paging<T>(totalItemCount, currentPage, pageSize);
    }

    public PaginationResponse(
        IEnumerable<T> data,
        int totalItemCount,
        int pageSize,
        string? previousCursor = null,
        string? nextCursor = null
    )
    {
        Data = data;
        Paging = new Paging<T>(totalItemCount, pageSize, previousCursor, nextCursor);
    }
}

public class Paging<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalPage { get; set; }

    public bool? HasNextPage { get; set; }

    public bool? HasPreviousPage { get; set; }

    public string? Before { get; set; }

    public string? After { get; set; }

    public Paging(int totalItemCount, int currentPage = 1, int pageSize = 10)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPage = (int)Math.Ceiling(totalItemCount / (double)pageSize);

        HasNextPage = CurrentPage < TotalPage;
        HasPreviousPage = currentPage > 1;
    }

    public Paging(
        int totalItemCount,
        int pageSize = 10,
        string? previousCursor = null,
        string? nextCursor = null
    )
    {
        PageSize = pageSize;
        TotalPage = (int)Math.Ceiling(totalItemCount / (double)pageSize);
        After = nextCursor;
        HasNextPage = nextCursor != null;
        Before = previousCursor;
        HasPreviousPage = previousCursor != null;
    }
}
