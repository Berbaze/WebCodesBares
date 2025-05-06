namespace WebCodesBares.Data.Service
{
    public enum TypeLicence
    {
        Basic,
        Pro,
        Enterprise
    }

    public class LicenceInfo
    {
        public TypeLicence Type { get; set; }
        public int NombreUtilisateurs { get; set; }
        public int NombreBarcodes { get; set; }
        public decimal Prix { get; set; }
        public decimal PrixMaintenance { get; set; }
        public int DureeValiditeEnMois { get; set; }
    }

    public class LicenceConfiguration
    {
        public static LicenceInfo GetConfiguration(TypeLicence type)
        {
            return type switch
            {
                TypeLicence.Basic => new LicenceInfo
                {
                    Type = TypeLicence.Basic,
                    NombreUtilisateurs = 1,
                    NombreBarcodes = 10,
                    Prix = 990,
                    PrixMaintenance = 190,
                    DureeValiditeEnMois = 12
                },
                TypeLicence.Pro => new LicenceInfo
                {
                    Type = TypeLicence.Pro,
                    NombreUtilisateurs = 3,
                    NombreBarcodes = 10000,
                    Prix = 2990,
                    PrixMaintenance = 390,
                    DureeValiditeEnMois = 12
                },
                TypeLicence.Enterprise => new LicenceInfo
                {
                    Type = TypeLicence.Enterprise,
                    NombreUtilisateurs = 10,
                    // Pour "illimité", vous pouvez utiliser int.MaxValue ou une valeur qui a du sens dans votre contexte
                    NombreBarcodes = int.MaxValue,
                    Prix = 9990,
                    PrixMaintenance = 990,
                    DureeValiditeEnMois = 12
                },
                _ => throw new ArgumentException("Type de licence inconnu")
            };
        }
    }
}

