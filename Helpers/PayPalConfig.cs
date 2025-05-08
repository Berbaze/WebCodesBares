using PayPalCheckoutSdk.Core;

namespace WebCodesBares.Helpers
{
    public class PayPalConfig
    {
        public static PayPalEnvironment GetEnvironment()
        {
            string clientId = "AX9wNMocMlYKuvPLMKX3bnYDJAW25ygM2In4NP1Gs9Ktq7BGH_Pg0O6Ys2_Eh-lqPpa_XS60I3FDOdbT";
            string clientSecret = "EKd2JVsPjRUTQfxkSXFL6j5Iffx_smlMevzQDgkij-NCv_q0kt4Qpmtl-dx7rKYdKo_4GXDuLmYiBI1T";

            return new SandboxEnvironment(clientId, clientSecret);
        }

        public static PayPalHttpClient GetPayPalClient()
        {
            return new PayPalHttpClient(GetEnvironment());
        }
    }
}
