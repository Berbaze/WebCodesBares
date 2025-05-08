using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebCodesBares.Data
{
    public class Barcodes
    {
        [Key] // 🔥 Définit la clé primaire

        public int Id { get; set; } 
        public  int Id_Mitarbeiter { get; set; }
        public string BarcodeText { get; set; } = string.Empty;
        public string BarcodeImage { get; set;} = string.Empty;
        public string BarcodeLagerOrt {  get; set; } = string.Empty;
        public int KundenId { get; set; }
    }
}
