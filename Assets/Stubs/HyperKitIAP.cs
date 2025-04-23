using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace Services
{
    namespace IAP
    {
        public class IAPService
        {
            private readonly Dictionary<string, IPurchaseHandler> _handlers = new();

            public Action<string> OnPurchaseStarted { get; set; }
            public Action<string> OnPurchaseCompleted { get; set; }
            public Action<string> PurchaseFailed { get; set; }

            public object Validator { get; set; }

            public void Register(string productId, IPurchaseHandler handler)
            {
                _handlers[productId] = handler;
            }

            public bool IsReady()
            {
                return true;
            }

            public void Purchase(string productId)
            {
                if (!_handlers.TryGetValue(productId, out var handler))
                {
                    return;
                }

                OnPurchaseStarted?.Invoke(productId);
                handler.OnPurchase(productId);
                OnPurchaseCompleted?.Invoke(productId);
            }

            public void RestorePurchases()
            {
            }

            public string GetPrice(string productId)
            {
                var products = HyperKit.Data.IapProducts;
                foreach (var product in products)
                {
                    if (product.key == productId)
                    {
                        return product.price;
                    }
                }

                return "--";
            }

            public interface IPurchaseHandler
            {
                public void OnPurchase(string productId);
            }
        }
    }
}
