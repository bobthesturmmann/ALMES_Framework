namespace Core.Bom.Lib
{
    public class BomViewEntity
    {
        public int SatirNo { get; set; }
        public string AnaUrunKodu { get; set; } = string.Empty;
        public string AnaUrunAdi { get; set; } = string.Empty;
        public decimal AnaMiktar { get; set; }
        public string AnaBirimi { get; set; } = string.Empty;
        public string AnaBirimSeti { get; set; } = string.Empty;
        public string AltUrunKodu { get; set; } = string.Empty;
        public string AltUrunAdi { get; set; } = string.Empty;
        public decimal AltMiktar { get; set; }
        public string AltBirimi { get; set; } = string.Empty;
        public string AltBirimSeti { get; set; } = string.Empty;
    }

    public class BomManageResultEntity
    {
        public int IslemBasarili { get; set; }
        public int AnaUrunRef { get; set; }
        public int AltUrunRef { get; set; }
        public int EklenenSatirNo { get; set; }
    }
}