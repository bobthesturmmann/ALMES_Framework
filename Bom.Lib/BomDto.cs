namespace Bom.Lib
{
    public class BomDto
    {
        public int LineNo { get; set; }
        public string MainProductCode { get; set; } = string.Empty;
        public string MainProductName { get; set; } = string.Empty;
        public decimal MainQuantity { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitSet { get; set; } = string.Empty;
        public string SubProductCode { get; set; } = string.Empty;
        public string SubProductName { get; set; } = string.Empty;
        public decimal SubQuantity { get; set; }
        public string SubUnit { get; set; } = string.Empty;
        public string SubUnitSet { get; set; } = string.Empty;
        public int MainProductRef { get; set; }
        public bool IsRecipeExists { get; set; }
        public int SubProductRef { get; set; }
        public int AltBirimRef { get; set; }
        public string ProductType { get; set; } = string.Empty;
    }

    public class PagedBomResult
    {
        public List<BomDto> Items { get; set; } = new List<BomDto>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class BomRecipeSaveDto
    {
        public string FirmaNo { get; set; } = string.Empty;
        public string DonemNo { get; set; } = string.Empty;
        public int IslemTipi { get; set; } = 1;
        public int SatirNo { get; set; }
        public int AnaUrunRef { get; set; }
        public decimal AnaMiktar { get; set; }
        public int AnaBirimRef { get; set; }
        public int AltUrunRef { get; set; }
        public decimal AltMiktar { get; set; }
        public int AltBirimRef { get; set; }
        public decimal LostFactor { get; set; }
    }

    public class BomManageResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AddedLineNo { get; set; }
        public int MainProductRef { get; set; }
        public int SubProductRef { get; set; }
    }

    public class BulkRecipeSaveDto
    {
        public int AnaUrunRef { get; set; }
        public decimal AnaMiktar { get; set; }
        public int AnaBirimRef { get; set; }
        public bool IsDeleteAll { get; set; }

        public List<RecipeLineDto> Lines { get; set; } = new List<RecipeLineDto>();
    }

    public class RecipeLineDto
    {
        public string Status { get; set; } = string.Empty;
        public int SatirNo { get; set; }
        public int AltUrunRef { get; set; }
        public int AltBirimRef { get; set; }
        public decimal AltMiktar { get; set; }
    }
}