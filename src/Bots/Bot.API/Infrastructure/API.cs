using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bots.Bot.API.Infrastructure
{
  
    public static class CatalogAPI
        {
            public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type)
            {
                var filterQs = "";

                if (brand.HasValue || type.HasValue)
                {
                    var brandQs = (brand.HasValue) ? brand.Value.ToString() : "null";
                    var typeQs = (type.HasValue) ? type.Value.ToString() : "null";
                    filterQs = $"/type/{typeQs}/brand/{brandQs}";
                }

                return $"{baseUri}items{filterQs}?pageIndex={page}&pageSize={take}";
            }

            public static string GetAllBrands(string baseUri)
            {
                return $"{baseUri}catalogBrands";
            }

            public static string GetAllTypes(string baseUri)
            {
                return $"{baseUri}catalogTypes";
            }
        }

    public static class Order
    {
        public static string GetOrder(string baseUri, string orderId)
        {
            return $"{baseUri}/id/{orderId}";
        }

        public static string GetAllMyOrders(string baseUri, string userId)
        {
            return $"{baseUri}/user/{userId}";
        }

        public static string AddNewOrder(string baseUri)
        {
            return $"{baseUri}/new";
        }

        public static string CancelOrder(string baseUri)
        {
            return $"{baseUri}/cancel";
        }

        public static string ShipOrder(string baseUri)
        {
            return $"{baseUri}/ship";
        }
    }

    public static class Basket
        {
            public static string GetBasket(string baseUri, string basketId)
            {
                return $"{baseUri}/{basketId}";
            }

            public static string UpdateBasket(string baseUri)
            {
                return baseUri;
            }

            public static string CheckoutBasket(string baseUri)
            {
                return $"{baseUri}/checkout";
            }

            public static string CleanBasket(string baseUri, string basketId)
            {
                return $"{baseUri}/{basketId}";
            }
        }

    public static class CatalogAI
    {
        public static string GetProducSetDetailsByIDs(string baseUri, string productId, IEnumerable<string> productIDs)
        {
            return $"{baseUri}productSetDetailsByIDs?productId={productId}&productIDs={String.Join(",", productIDs)}";
        }

        public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type, IEnumerable<string> tags)
        {
            var brandQs = (brand.HasValue) ? $"&catalogBrandId={brand.Value.ToString()}" : String.Empty;
            var typeQs = (type.HasValue) ? $"&catalogTypeId={type.Value.ToString()}" : String.Empty;
            var tagsQs = (tags != null && tags.Any()) ? $"&tags={String.Join(",", tags)}" : String.Empty;

            return $"{baseUri}items?pageIndex={page}&pageSize={take}{brandQs}{typeQs}{tagsQs}";
        }
    }

    public static class ProductSearchImageService
    {
        public static string ClassifyImage(string baseUri)
        {
            return $"{baseUri}classifyImage";
        }
    }
}