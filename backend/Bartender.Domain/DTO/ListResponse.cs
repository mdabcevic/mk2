namespace Bartender.Domain.DTO;

public class ListResponse<T>
{
    public List<T>? Items { get; set; }
    public int Total { get; set; }
}
