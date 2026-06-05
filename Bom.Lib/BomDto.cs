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
    }
}