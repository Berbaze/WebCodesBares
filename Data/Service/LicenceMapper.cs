namespace WebCodesBares.Data.Service
{
    public static class LicenceMapper
    {
        public static TypeLicence? FromString(string rawType)
        {
            if (string.IsNullOrWhiteSpace(rawType))
                return null;

            rawType = rawType.Trim().ToLowerInvariant();

            return rawType switch
            {
                "basic" => TypeLicence.Basic,
                "pro" => TypeLicence.Pro,
                "enterprise" => TypeLicence.Enterprise,
                _ => null
            };
        }


    }
}

