namespace InmobiliariaAPI.Models.DTO
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }            // total sin filtro
        public int TotalPages { get; set; }       // total de páginas
    }
}