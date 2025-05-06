using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;



namespace WebCodesBares.Middleware
{
    public class StripeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StripeMiddleware> _logger;
        private const string StripeWebhookSecret = "ton_secret_webhook"; // 🔒 Remplace par ta clé dans appsettings.json

        public StripeMiddleware(RequestDelegate next, ILogger<StripeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/stripe-webhook"))
            {
                _logger.LogInformation("📩 Réception d'un webhook Stripe...");

                try
                {
                    // ✅ Lire le corps de la requête
                    var json = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();

                    // ✅ Vérification de la signature du webhook Stripe
                    var stripeSignature = context.Request.Headers["Stripe-Signature"];
                    if (string.IsNullOrEmpty(stripeSignature))
                    {
                        _logger.LogWarning("⚠️ Webhook Stripe sans signature !");
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    Stripe.Event stripeEvent;
                    try
                    {
                        stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, StripeWebhookSecret);
                    }
                    catch (StripeException e)
                    {
                        _logger.LogError($"❌ Erreur de vérification du webhook Stripe : {e.Message}");
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    _logger.LogInformation($"✅ Événement Stripe reçu : {stripeEvent.Type}");

                    // 🔹 Gérer les événements Stripe ici
                    if (stripeEvent.Type == "checkout.session.completed")
                    // ✅ Correction ici
                    {
                        _logger.LogInformation("✅ Paiement validé !");
                        // 👉 Ici, tu peux mettre à jour la commande dans la BDD
                    }

                    context.Response.StatusCode = StatusCodes.Status200OK;
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Erreur interne Stripe Middleware : {ex.Message}");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }
            }

            await _next(context);
        }
    }
}
