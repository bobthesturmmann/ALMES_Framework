namespace Core.Bom.Lib
{
    public class BomViewEntity
    {
        // MOD 1 & 2 ÇIKTILARI (Ana Ürün Listesi ve Seçim Modalı)
        public int UrunRef { get; set; }
        public string UrunKodu { get; set; } = string.Empty;
        public string UrunAdi { get; set; } = string.Empty;
        public string UrunTuru { get; set; } = string.Empty;
        public string ReceteDurumu { get; set; } = string.Empty;

        // MOD 3 & 4 ÇIKTILARI (Reçete Satırları ve Bileşen Seçim Modalı)
        public int BilesenRef { get; set; }
        public string BilesenKodu { get; set; } = string.Empty;
        public string BilesenAdi { get; set; } = string.Empty;
        public string BilesenTuru { get; set; } = string.Empty;
        public int BirimRef { get; set; }

        // ORTAK ALANLAR (Miktar ve Birim her modda bu isimle döner)
        public decimal Miktar { get; set; }
        public string Birim { get; set; } = string.Empty;
    }

    public class BomManageResultEntity
    {
        public int IslemBasarili { get; set; }
        public int AnaUrunRef { get; set; }
        public int AltUrunRef { get; set; }
        public int EklenenSatirNo { get; set; }
    }
}